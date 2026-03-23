using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReadOnlyPropertyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD018";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Read-only property detected",
            messageFormat: "Property '{0}' is read-only. Builder methods won't be generated. Consider adding a setter or using [Ignore] attribute.",
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
                var ignoreAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentIgnoreAttribute");

                if (builderAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, ignoreAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(
            SymbolAnalysisContext context,
            INamedTypeSymbol builderAttribute,
            INamedTypeSymbol? ignoreAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only types with [FluentBuilder]
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            foreach (var member in type.GetMembers())
            {
                if (member is not IPropertySymbol property)
                    continue;

                // Only public instance properties
                if (property.IsStatic || property.DeclaredAccessibility != Accessibility.Public)
                    continue;

                // Skip if ignored
                if (ignoreAttribute != null && HasIgnoreAttribute(property, ignoreAttribute))
                    continue;

                // Check if property has a settable setter (non‑init)
                // IsInitOnly is a property (not a method)
                bool hasSettableSetter = property.SetMethod != null && !property.SetMethod.IsInitOnly;

                if (!hasSettableSetter)
                {
                    var location = property.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    if (location != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, property.Name));
                    }
                }
            }
        }

        private static bool HasIgnoreAttribute(ISymbol member, INamedTypeSymbol ignoreAttribute)
        {
            return member.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttribute));
        }
    }
}
