using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileScopedBuilderUsageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FB052";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "File-scoped builder used in broader context",
            messageFormat: "The builder for type '{0}' is file-scoped, but it is referenced from property '{1}' in type '{2}', which may be used outside the file. This will cause a compilation error.",
            category: "FluentBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A file-scoped builder cannot be used as a nested builder in another type because the generated method would be accessible outside the file.");

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

            // For each buildable member that is a property of a type that itself has a file-scoped builder
            foreach (var member in parentType.GetMembers())
            {
                if (member is IPropertySymbol property && !property.IsStatic && property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyType = property.Type;
                    if (propertyType is INamedTypeSymbol namedPropertyType &&
                        HasBuilderAttribute(namedPropertyType, attributeSymbol))
                    {
                        // Check if that type's builder is file-scoped
                        if (IsFileScopedBuilder(namedPropertyType, attributeSymbol))
                        {
                            var location = property.Locations.FirstOrDefault() ?? parentType.Locations.FirstOrDefault();
                            context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                                namedPropertyType.Name,
                                property.Name,
                                parentType.Name));
                        }
                    }
                }
            }
        }

        private static bool HasBuilderAttribute(INamedTypeSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            return typeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        }

        private static bool IsFileScopedBuilder(INamedTypeSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            var attr = typeSymbol.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
            if (attr == null)
                return false;

            var accessibilityArg = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "BuilderAccessibility");
            if (accessibilityArg.Key == null)
                return false; // default is Public

            var typedConstant = accessibilityArg.Value;
            if (typedConstant.Kind != TypedConstantKind.Enum || !(typedConstant.Type is INamedTypeSymbol enumType))
                return false;

            var intValue = typedConstant.Value is int intVal ? intVal : Convert.ToInt32(typedConstant.Value);
            var member = enumType.GetMembers().OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue?.Equals(intValue) == true);
            return member?.Name == "File";
        }
    }
}