using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PrivateSetterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD017";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Property has private setter",
            messageFormat: "Property '{0}' has a private setter. Builder methods won't be generated. Consider making the setter public or adding [Ignore] attribute.",
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

                // Only consider public instance properties (non‑static)
                if (property.IsStatic || property.DeclaredAccessibility != Accessibility.Public)
                    continue;

                // Skip if property has [FluentIgnore]
                if (ignoreAttribute != null &&
                    property.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttribute)))
                    continue;

                // Check if the setter exists and is private
                var setter = property.SetMethod;
                if (setter != null && setter.DeclaredAccessibility == Accessibility.Private)
                {
                    var location = property.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    if (location != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, property.Name));
                    }
                }
            }
        }
    }
}
