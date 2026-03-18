using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NoAccessibleConstructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD010";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No accessible constructor found",
            messageFormat: "Class '{0}' has no public or internal constructors accessible to the builder. Add an accessible constructor or use [FluentIgnore] on existing constructors.",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var builderAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentBuilderAttribute");
                var factoryMethodAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentBuilderFactoryMethodAttribute");

                if (builderAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, factoryMethodAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol builderAttribute, INamedTypeSymbol? factoryMethodAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only classes/records (not interfaces, enums, etc.)
            if (type.TypeKind != TypeKind.Class && !type.IsRecord)
                return;

            // Must have the FluentBuilder attribute
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            // If a factory method attribute is present, we assume the user will provide a factory,
            // so no warning about constructors.
            if (factoryMethodAttribute != null &&
                type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, factoryMethodAttribute)))
            {
                return;
            }

            // Check for at least one accessible constructor.
            // Accessible constructors: public, internal, protected internal.
            var accessibleConstructors = type.Constructors.Where(ctor =>
                ctor.DeclaredAccessibility == Accessibility.Public ||
                ctor.DeclaredAccessibility == Accessibility.Internal ||
                ctor.DeclaredAccessibility == Accessibility.ProtectedOrInternal); // protected internal

            if (accessibleConstructors.Any())
                return;

            // No accessible constructor found – report warning.
            var location = type.Locations.FirstOrDefault();
            if (location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name));
            }
        }
    }
}
