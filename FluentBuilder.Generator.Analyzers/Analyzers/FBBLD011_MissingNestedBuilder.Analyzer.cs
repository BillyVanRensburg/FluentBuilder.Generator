using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MissingNestedBuilderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD011";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Nested builder type not found",
            messageFormat: "Type '{0}' referenced in property '{1}' does not have a builder class. Ensure it has [FluentBuilder] attribute.",
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

            // Only classes/records (not interfaces, enums, etc.)
            if (type.TypeKind != TypeKind.Class && !type.IsRecord)
                return;

            // Must have the FluentBuilder attribute
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            // Examine each public instance property
            foreach (var member in type.GetMembers())
            {
                if (member is IPropertySymbol property &&
                    !property.IsStatic &&
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyType = property.Type;
                    if (propertyType is INamedTypeSymbol namedType &&
                        (namedType.TypeKind == TypeKind.Class || namedType.IsRecord))
                    {
                        // Only warn if the property type is defined in source (i.e., user code)
                        if (namedType.Locations.Any(loc => loc.IsInSource))
                        {
                            // Check if it has the builder attribute
                            if (!namedType.GetAttributes().Any(a =>
                                SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                            {
                                var location = property.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                                if (location != null)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(
                                        Rule, location, namedType.Name, property.Name));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
