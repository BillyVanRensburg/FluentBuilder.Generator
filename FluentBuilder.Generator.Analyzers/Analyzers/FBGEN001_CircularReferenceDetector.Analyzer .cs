using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CircularReferenceDetectorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBGEN001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Circular reference detected",
            messageFormat: "Circular reference detected in builder chain for '{0}'. Check nested builder references.",
            category: "General",
            DiagnosticSeverity.Error,
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
                if (builderAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol builderAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only process types that have the [FluentBuilder] attribute.
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            // Detect cycles starting from this type.
            var path = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            if (DetectCycle(type, path, builderAttribute, out var cycleLocation, out var cycleTypeName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, cycleLocation ?? type.Locations.FirstOrDefault(), cycleTypeName ?? type.Name));
            }
        }

        private static bool DetectCycle(
            INamedTypeSymbol current,
            HashSet<INamedTypeSymbol> path,
            INamedTypeSymbol builderAttribute,
            out Location? cycleLocation,
            out string? cycleTypeName)
        {
            if (!path.Add(current))
            {
                // Found a cycle: current is already in the path.
                cycleLocation = null;  // will be set by the caller that follows the edge
                cycleTypeName = current.Name;
                return true;
            }

            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic || property.DeclaredAccessibility != Accessibility.Public)
                    continue;

                if (property.Type is INamedTypeSymbol propType &&
                    (propType.TypeKind == TypeKind.Class || propType.IsRecord))
                {
                    if (!HasFluentBuilderAttribute(propType, builderAttribute))
                        continue;

                    if (DetectCycle(propType, path, builderAttribute, out var innerLocation, out var innerName))
                    {
                        // Propagate the cycle upward, setting the location to the property that leads to it.
                        cycleLocation = property.Locations.FirstOrDefault() ?? innerLocation;
                        cycleTypeName = innerName;
                        path.Remove(current);
                        return true;
                    }
                }
            }

            path.Remove(current);
            cycleLocation = null;
            cycleTypeName = null;
            return false;
        }

        private static bool HasFluentBuilderAttribute(INamedTypeSymbol type, INamedTypeSymbol builderAttribute)
        {
            return type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute));
        }
    }
}
