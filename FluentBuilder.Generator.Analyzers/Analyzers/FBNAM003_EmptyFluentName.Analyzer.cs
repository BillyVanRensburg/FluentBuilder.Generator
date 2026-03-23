using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EmptyFluentNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBNAM003";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "FluentName cannot be empty",
            messageFormat: "FluentName attribute on '{0}' cannot have an empty or whitespace value. Value was: '{1}'.",
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
                var builderAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentBuilderAttribute");
                var fluentNameAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentNameAttribute");

                if (builderAttribute == null || fluentNameAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, fluentNameAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(
            SymbolAnalysisContext context,
            INamedTypeSymbol builderAttribute,
            INamedTypeSymbol fluentNameAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only types with [FluentBuilder] matter because FluentName is only meaningful there.
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            // Check the type itself for [FluentName] (renames the builder class)
            foreach (var attr in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, fluentNameAttribute))
                {
                    var arg = attr.ConstructorArguments.FirstOrDefault();
                    if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string name && IsEmptyOrWhitespace(name))
                    {
                        var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                       ?? type.Locations.FirstOrDefault();
                        if (location != null)
                            context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name, name));
                    }
                }
            }

            // Check members
            foreach (var member in type.GetMembers())
            {
                foreach (var attr in member.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, fluentNameAttribute))
                    {
                        var arg = attr.ConstructorArguments.FirstOrDefault();
                        if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string name && IsEmptyOrWhitespace(name))
                        {
                            var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                           ?? member.Locations.FirstOrDefault()
                                           ?? type.Locations.FirstOrDefault();
                            if (location != null)
                                context.ReportDiagnostic(Diagnostic.Create(Rule, location, member.Name, name));
                        }
                    }
                }
            }
        }

        private static bool IsEmptyOrWhitespace(string s) => string.IsNullOrWhiteSpace(s);
    }
}
