using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BuilderAccessibilityVsContainerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD0005";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Nested builder accessibility cannot exceed containing type accessibility",
            messageFormat: "Builder accessibility '{0}' is more permissive than containing type '{1}' accessibility '{2}'. The builder must be at most as accessible as its container.",
            category: "FluentBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "For nested builders, the builder's accessibility must be at most the accessibility of its containing type.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

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

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, attributeSymbol),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol attributeSymbol)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;

            if (namedType.TypeKind != TypeKind.Class)
                return;

            // Only check if the type is nested (has containing types)
            if (namedType.ContainingType == null)
                return;

            var attribute = namedType.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

            if (attribute == null)
                return;

            var accessibilityArg = attribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "BuilderAccessibility");

            if (accessibilityArg.Key == null)
                return;

            var typedConstant = accessibilityArg.Value;

            if (typedConstant.Kind != TypedConstantKind.Enum ||
                typedConstant.Type is not INamedTypeSymbol enumType)
            {
                return;
            }

            var rawValue = typedConstant.Value;
            if (rawValue == null)
                return;

            int intValue = rawValue is int i
                ? i
                : Convert.ToInt32(rawValue);

            var enumMember = enumType.GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f =>
                    f.HasConstantValue &&
                    Equals(f.ConstantValue, intValue));

            if (enumMember == null)
                return;

            var builderName = enumMember.Name;
            var builderAccessibility = MapBuilderAccessibility(builderName);

            // -----------------------------------------------------------------
            // Special rule: A file‑scoped builder cannot be nested at all.
            // Since the target is nested, any builder set to File is invalid.
            // -----------------------------------------------------------------
            if (builderName == "File")
            {
                ReportDiagnostic(context, attribute, builderName, namedType.ContainingType.Name, "file (cannot be nested)");
                return;
            }

            // Walk up all containing types for normal accessibility comparison
            var current = namedType.ContainingType;
            while (current != null)
            {
                // If the container is file‑scoped, the builder cannot reference it from another file.
                if (IsFileLocalType(current))
                {
                    ReportDiagnostic(context, attribute, builderName, current.Name, "file");
                    return;
                }

                // Normal accessibility rank comparison
                if (!IsBuilderCompatibleWithContainer(builderAccessibility, current.DeclaredAccessibility))
                {
                    ReportDiagnostic(context, attribute, builderName, current.Name, current.DeclaredAccessibility.ToString());
                    return;
                }

                current = current.ContainingType;
            }
        }

        private static bool IsFileLocalType(INamedTypeSymbol type)
        {
            // Try the new IsFileLocal property (Roslyn 4.0+)
            var isFileLocalProp = type.GetType().GetProperty("IsFileLocal", BindingFlags.Public | BindingFlags.Instance);
            if (isFileLocalProp != null)
            {
                return (bool)isFileLocalProp.GetValue(type);
            }

            // Fallback: check syntax for 'file' keyword
            foreach (var syntaxRef in type.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax() is BaseTypeDeclarationSyntax typeDecl)
                {
                    if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.FileKeyword)))
                        return true;
                }
            }
            return false;
        }

        private static bool IsBuilderCompatibleWithContainer(Accessibility builder, Accessibility container)
        {
            return GetAccessibilityRank(builder) <= GetAccessibilityRank(container);
        }

        private static Accessibility MapBuilderAccessibility(string name)
        {
            return name switch
            {
                "Public" => Accessibility.Public,
                "Internal" => Accessibility.Internal,
                "Protected" => Accessibility.Protected,
                "Private" => Accessibility.Private,
                "ProtectedInternal" => Accessibility.ProtectedOrInternal,
                "PrivateProtected" => Accessibility.ProtectedAndInternal,
                "File" => Accessibility.NotApplicable, // handled separately
                _ => Accessibility.Public
            };
        }

        private static int GetAccessibilityRank(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => 6,
                Accessibility.ProtectedOrInternal => 5,
                Accessibility.Internal => 4,
                Accessibility.Protected => 3,
                Accessibility.ProtectedAndInternal => 2,
                Accessibility.Private => 1,
                Accessibility.NotApplicable => 0, // File (should not appear here for file builder check)
                _ => 0
            };
        }

        private static void ReportDiagnostic(
            SymbolAnalysisContext context,
            AttributeData attribute,
            string builderAcc,
            string containerName,
            string containerAcc)
        {
            var location =
                attribute.ApplicationSyntaxReference?
                .GetSyntax(context.CancellationToken)
                .GetLocation()
                ?? context.Symbol.Locations.FirstOrDefault();

            if (location != null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, location, builderAcc, containerName, containerAcc));
            }
        }
    }
}
