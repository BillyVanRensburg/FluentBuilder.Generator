using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BuilderAccessibilityVsTargetTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD0004";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Builder accessibility cannot exceed target type accessibility",
            messageFormat: "Builder accessibility '{0}' is more permissive than target type accessibility '{1}'. The builder must be at most as accessible as the target type.",
            category: "FluentBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The generated builder class must not be more accessible than the type it builds.");

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

            // -----------------------------------------------------------------
            // 1. If the target type is file-scoped, the builder cannot reference it
            //    because the builder is always in a separate generated file.
            // -----------------------------------------------------------------
            if (IsFileLocalTarget(namedType))
            {
                ReportDiagnostic(context, attribute, builderName, "File");
                return;
            }

            // -----------------------------------------------------------------
            // 2. Handle builder = File
            // -----------------------------------------------------------------
            if (builderName == "File")
            {
                // File-scoped builder cannot be nested (C# language rule)
                if (namedType.ContainingType != null)
                {
                    // File builder on a nested target is invalid
                    ReportDiagnostic(context, attribute, builderName, GetTargetAccessibilityString(namedType));
                    return;
                }

                // For top-level non-file targets, File builder is always allowed
                // because it's the most restrictive.
                return;
            }

            // -----------------------------------------------------------------
            // 3. Normal accessibility comparison for other builder values
            // -----------------------------------------------------------------
            var builderAccessibility = MapBuilderAccessibility(builderName);
            var targetAccessibility = namedType.DeclaredAccessibility;

            if (!IsBuilderAccessibilityCompatible(builderAccessibility, targetAccessibility))
            {
                ReportDiagnostic(context, attribute, builderName, GetTargetAccessibilityString(namedType));
            }
        }

        private static bool IsFileLocalTarget(INamedTypeSymbol namedType)
        {
            foreach (var syntaxRef in namedType.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax() is BaseTypeDeclarationSyntax typeDecl)
                {
                    if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.FileKeyword)))
                        return true;
                }
            }

            return false;
        }

        private static string GetTargetAccessibilityString(INamedTypeSymbol namedType)
        {
            if (IsFileLocalTarget(namedType))
                return "File";

            return namedType.DeclaredAccessibility.ToString();
        }

        private static void ReportDiagnostic(
            SymbolAnalysisContext context,
            AttributeData attribute,
            string builderAcc,
            string targetAcc)
        {
            var location =
                attribute.ApplicationSyntaxReference?
                .GetSyntax(context.CancellationToken)
                .GetLocation()
                ?? context.Symbol.Locations.FirstOrDefault();

            if (location != null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, location, builderAcc, targetAcc));
            }
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

        private static bool IsBuilderAccessibilityCompatible(
            Accessibility builder,
            Accessibility target)
        {
            // File builder is handled earlier, so we never get here with builder == NotApplicable.
            // But if we do, treat it as always compatible (it's the most restrictive).
            if (builder == Accessibility.NotApplicable)
                return true;

            return (target, builder) switch
            {
                // Public target: allows all except File (File already handled)
                (Accessibility.Public, _) => true,

                // Internal target: allows only Private, PrivateProtected, Internal
                (Accessibility.Internal, Accessibility.Private) => true,
                (Accessibility.Internal, Accessibility.ProtectedAndInternal) => true,
                (Accessibility.Internal, Accessibility.Internal) => true,

                // Protected target: allows only Private, PrivateProtected, Protected
                (Accessibility.Protected, Accessibility.Private) => true,
                (Accessibility.Protected, Accessibility.ProtectedAndInternal) => true,
                (Accessibility.Protected, Accessibility.Protected) => true,

                // ProtectedInternal target: allows Private, PrivateProtected, Protected, Internal, ProtectedInternal
                (Accessibility.ProtectedOrInternal, Accessibility.Private) => true,
                (Accessibility.ProtectedOrInternal, Accessibility.ProtectedAndInternal) => true,
                (Accessibility.ProtectedOrInternal, Accessibility.Protected) => true,
                (Accessibility.ProtectedOrInternal, Accessibility.Internal) => true,
                (Accessibility.ProtectedOrInternal, Accessibility.ProtectedOrInternal) => true,

                // PrivateProtected target: allows only Private, PrivateProtected
                (Accessibility.ProtectedAndInternal, Accessibility.Private) => true,
                (Accessibility.ProtectedAndInternal, Accessibility.ProtectedAndInternal) => true,

                // Private target: allows only Private
                (Accessibility.Private, Accessibility.Private) => true,

                // Everything else is not allowed
                _ => false
            };
        }
    }
}
