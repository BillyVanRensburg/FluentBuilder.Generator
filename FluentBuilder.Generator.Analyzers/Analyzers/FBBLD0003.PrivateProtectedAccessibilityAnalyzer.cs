using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FluentBuilderPrivateProtectedAccessibilityAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD0003";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "BuilderAccessibility.PrivateProtected requires C# 7.2 or higher",
                messageFormat:
                    "BuilderAccessibility.PrivateProtected is not supported in C# language version '{0}'. C# 7.2 or higher is required.",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description:
                    "The 'private protected' accessibility modifier is only available starting from C# 7.2.");

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

            if (namedType.TypeKind != TypeKind.Class && !namedType.IsRecord)
                return;

            if (namedType.IsImplicitlyDeclared)
                return;

            var attribute = namedType.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

            if (attribute == null)
                return;

            var builderArg = attribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "BuilderAccessibility");

            if (builderArg.Key == null || builderArg.Value.Value == null)
                return;

            var rawValue = builderArg.Value.Value;

            if (builderArg.Value.Type is not INamedTypeSymbol enumType ||
                enumType.TypeKind != TypeKind.Enum)
                return;

            var memberName = enumType.GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, rawValue))
                ?.Name;

            if (memberName != "PrivateProtected")
                return;

            // ✅ Check C# language version
            if (context.Compilation is CSharpCompilation csharpCompilation)
            {
                var languageVersion = csharpCompilation.LanguageVersion;

                if (languageVersion < LanguageVersion.CSharp7_2)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            namedType.Locations[0],
                            languageVersion.ToDisplayString()));
                }
            }
        }
    }
}