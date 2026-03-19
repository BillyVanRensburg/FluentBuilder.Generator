using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidFluentNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBNAM001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Invalid fluent method name",
            messageFormat: "Invalid fluent method name '{0}' for member '{1}'. Method names must be valid C# identifiers.",
            category: "FluentName",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var fluentNameAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentNameAttribute");
                if (fluentNameAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, fluentNameAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol fluentNameAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // We only care about types that have the FluentBuilder attribute,
            // because that's where FluentName is meaningful.
            var builderAttribute = context.Compilation.GetTypeByMetadataName("FluentBuilder.FluentBuilderAttribute");
            if (builderAttribute == null)
                return;

            bool hasBuilderAttr = type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute));
            if (!hasBuilderAttr)
                return;

            foreach (var member in type.GetMembers())
            {
                var fluentNameAttr = member.GetAttributes()
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, fluentNameAttribute));
                if (fluentNameAttr == null)
                    continue;

                // Get the name argument from the attribute constructor (first argument).
                var nameArg = fluentNameAttr.ConstructorArguments.FirstOrDefault();
                if (nameArg.Kind != TypedConstantKind.Primitive || !(nameArg.Value is string name))
                    continue;

                // Check if it's a valid C# identifier.
                if (!SyntaxFacts.IsValidIdentifier(name))
                {
                    var location = member.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    if (location != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, name, member.Name));
                    }
                }
            }
        }
    }
}
