using FluentBuilder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FluentBuilderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD0001";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "Invalid BuilderAccessibility for top-level type",
                messageFormat:
                    "Top-level {0} '{1}' can only use BuilderAccessibility.Public or BuilderAccessibility.Internal",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description:
                    "Top-level types using FluentBuilder must specify Public or Internal accessibility if explicitly set.");

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

            // Only analyze top-level classes and records
            if (namedType.TypeKind != TypeKind.Class && !namedType.IsRecord)
                return;

            if (namedType.IsImplicitlyDeclared)
                return;

            if (namedType.ContainingType != null) // only top-level
                return;

            var attribute = namedType.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

            if (attribute == null)
                return;

            // Check BuilderAccessibility argument
            var builderArg = attribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "BuilderAccessibility");

            bool reportDiagnostic = false;

            if (builderArg.Key != null && builderArg.Value.Value != null)
            {
                // Argument is explicitly provided, check its value
                var rawValue = builderArg.Value.Value;

                if (builderArg.Value.Type is INamedTypeSymbol enumType && enumType.TypeKind == TypeKind.Enum)
                {
                    var memberName = enumType.GetMembers()
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue.Equals(rawValue))
                        ?.Name;

                    // Only Public and Internal are allowed
                    if (memberName != "Public" && memberName != "Internal")
                    {
                        reportDiagnostic = true;
                    }
                }
            }

            if (reportDiagnostic)
            {
                var typeKind = namedType.IsRecord ? "record" : "class";
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    namedType.Locations[0],
                    typeKind,
                    namedType.Name));
            }
        }
    }
}