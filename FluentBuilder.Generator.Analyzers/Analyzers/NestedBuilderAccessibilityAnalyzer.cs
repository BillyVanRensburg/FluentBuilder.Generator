using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NestedBuilderAccessibilityAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FB049";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Nested builder accessibility mismatch",
            messageFormat: "The builder for property '{0}' of type '{1}' has accessibility '{2}', which is less accessible than the containing builder '{3}'. The nested builder method cannot be generated.",
            category: "FluentBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "When a property type has its own builder, the generated method that accepts an Action<PropertyBuilder> must have compatible accessibility. If the parent builder is more accessible than the nested builder, the method cannot be generated.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var attributeSymbol = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentBuilderAttribute");
                if (attributeSymbol == null)
                    return;

                var compilation = compilationContext.Compilation;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, attributeSymbol, compilation),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol attributeSymbol, Compilation compilation)
        {
            var parentType = (INamedTypeSymbol)context.Symbol;

            if (parentType.TypeKind != TypeKind.Class && !parentType.IsRecord)
                return;

            var parentAttr = parentType.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
            if (parentAttr == null)
                return;

            // Determine parent builder accessibility
            string parentBuilderAccessibility = GetBuilderAccessibilityFromAttribute(parentAttr, out _) ?? "Public";

            // Examine all buildable members that are property types with their own builder
            foreach (var member in parentType.GetMembers())
            {
                if (member is IPropertySymbol property && !property.IsStatic && property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyType = property.Type;
                    if (propertyType is INamedTypeSymbol namedPropertyType &&
                        HasBuilderAttribute(namedPropertyType, attributeSymbol))
                    {
                        // Get the nested builder's accessibility
                        var nestedAttr = namedPropertyType.GetAttributes()
                            .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
                        if (nestedAttr == null) continue;

                        string nestedBuilderAccessibility = GetBuilderAccessibilityFromAttribute(nestedAttr, out _) ?? "Public";

                        if (!IsAccessibilityCompatible(parentBuilderAccessibility, nestedBuilderAccessibility))
                        {
                            var location = property.Locations.FirstOrDefault() ?? parentType.Locations.FirstOrDefault();
                            context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                                property.Name,
                                namedPropertyType.Name,
                                nestedBuilderAccessibility,
                                parentBuilderAccessibility));
                        }
                    }
                }
            }
        }

        private static bool HasBuilderAttribute(INamedTypeSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            return typeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        }

        private static string? GetBuilderAccessibilityFromAttribute(AttributeData attribute, out Location? location)
        {
            location = attribute.ApplicationSyntaxReference?.GetSyntax()?.GetLocation();
            var accessibilityArg = attribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "BuilderAccessibility");
            if (accessibilityArg.Key == null)
                return null; // default Public

            var typedConstant = accessibilityArg.Value;
            if (typedConstant.Kind != TypedConstantKind.Enum || !(typedConstant.Type is INamedTypeSymbol enumType))
                return null;

            var intValue = typedConstant.Value is int intVal ? intVal : Convert.ToInt32(typedConstant.Value);
            var member = enumType.GetMembers().OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue?.Equals(intValue) == true);
            return member?.Name;
        }

        private static bool IsAccessibilityCompatible(string parentAccessibility, string nestedAccessibility)
        {
            int parentLevel = GetAccessibilityLevel(parentAccessibility);
            int nestedLevel = GetAccessibilityLevel(nestedAccessibility);
            return nestedLevel >= parentLevel; // Nested builder must be at least as accessible as parent.
        }

        private static int GetAccessibilityLevel(string name) => name switch
        {
            "File" => 0,
            "Private" => 1,
            "Protected" => 2,
            "Internal" => 3,
            "PrivateProtected" => 3,
            "ProtectedInternal" => 4,
            "Public" => 5,
            _ => 5
        };
    }
}