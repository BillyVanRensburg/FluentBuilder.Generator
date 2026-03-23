using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CollectionWithoutAddMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD019";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Collection type without Add method",
            messageFormat: "Type '{0}' appears to be a collection but doesn't have an Add method. Collection helper methods won't be generated.",
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

            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            foreach (var member in type.GetMembers())
            {
                // Skip ignored members
                if (ignoreAttribute != null && HasIgnoreAttribute(member, ignoreAttribute))
                    continue;

                ITypeSymbol? memberType = null;
                if (member is IPropertySymbol property && !property.IsStatic && property.DeclaredAccessibility == Accessibility.Public)
                {
                    memberType = property.Type;
                }
                else if (member is IFieldSymbol field && !field.IsStatic && field.DeclaredAccessibility == Accessibility.Public)
                {
                    memberType = field.Type;
                }

                if (memberType != null && IsProblematicCollection(memberType, out string typeName))
                {
                    var location = member.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    if (location != null)
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, typeName));
                }
            }
        }

        private static bool IsProblematicCollection(ITypeSymbol typeSymbol, out string typeName)
        {
            typeName = typeSymbol.ToString(); // fallback

            // Arrays are collections but have no usable Add method.
            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                typeName = arrayType.ToString(); // e.g., "int[]"
                return true;
            }

            if (typeSymbol is not INamedTypeSymbol type)
                return false;

            typeName = type.ToDisplayString();

            // Read‑only collection interfaces: no Add method at all.
            if (type.Name == "IReadOnlyList" || type.Name == "IReadOnlyCollection")
                return true;

            // If it doesn't look like a collection, it's not problematic.
            if (!IsCollectionType(type))
                return false;

            // If it has a usable Add method, it's fine.
            if (HasUsableAddMethod(type))
                return false;

            // Otherwise, it's problematic.
            return true;
        }

        private static bool IsCollectionType(INamedTypeSymbol type)
        {
            // Check if the type implements ICollection or ICollection<T>
            foreach (var @interface in type.AllInterfaces)
            {
                if (@interface.Name == "ICollection" || @interface.Name == "ICollection`1")
                    return true;
            }
            return false;
        }

        private static bool HasUsableAddMethod(INamedTypeSymbol type)
        {
            // Walk up the inheritance chain to find an Add method.
            var current = type;
            while (current != null)
            {
                // Look for an instance method named "Add" with at least one parameter and returning void.
                foreach (var member in current.GetMembers("Add"))
                {
                    if (member is IMethodSymbol method && !method.IsStatic && method.DeclaredAccessibility == Accessibility.Public)
                    {
                        if (method.Parameters.Length > 0 && method.ReturnsVoid)
                            return true;
                    }
                }
                current = current.BaseType;
            }
            return false;
        }

        private static bool HasIgnoreAttribute(ISymbol member, INamedTypeSymbol ignoreAttribute)
        {
            return member.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttribute));
        }
    }
}
