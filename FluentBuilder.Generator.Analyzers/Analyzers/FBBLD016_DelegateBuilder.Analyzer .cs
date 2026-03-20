using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DelegateBuilderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD016";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cannot generate builder for delegate",
            messageFormat: "Cannot generate fluent builder for delegate '{0}'",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

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

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, attributeSymbol),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol attributeSymbol)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Must be delegate
            if (type.TypeKind != TypeKind.Delegate)
                return;

            // Must have the FluentBuilder attribute
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol)))
                return;

            var location = type.Locations.FirstOrDefault();
            if (location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name));
            }
        }
    }
}
