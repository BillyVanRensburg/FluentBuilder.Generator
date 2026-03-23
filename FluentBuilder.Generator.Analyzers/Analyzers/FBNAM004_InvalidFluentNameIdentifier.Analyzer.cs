using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidFluentNameIdentifierAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBNAM004";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "FluentName is not a valid identifier",
            messageFormat: "FluentName attribute on '{0}' has invalid value '{1}'. Name must be a valid C# identifier (letters, digits, underscore, cannot start with digit, not a keyword).",
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

            // Check the type itself
            foreach (var attr in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, fluentNameAttribute))
                    CheckFluentName(attr, type, context);
            }

            // Check members
            foreach (var member in type.GetMembers())
            {
                foreach (var attr in member.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, fluentNameAttribute))
                        CheckFluentName(attr, member, context);
                }
            }
        }

        private static void CheckFluentName(AttributeData attr, ISymbol symbol, SymbolAnalysisContext context)
        {
            var arg = attr.ConstructorArguments.FirstOrDefault();
            if (arg.Kind != TypedConstantKind.Primitive || !(arg.Value is string name))
                return;

            // Skip empty/whitespace – those are handled by FBNAM003.
            if (string.IsNullOrWhiteSpace(name))
                return;

            // Check if it's a valid C# identifier (including @-prefixed keywords).
            if (!IsValidIdentifier(name))
            {
                var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                               ?? symbol.Locations.FirstOrDefault();
                if (location != null)
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, name));
            }
        }

        private static bool IsValidIdentifier(string name)
        {
            bool escaped = name.Length > 1 && name[0] == '@';
            if (escaped)
                name = name.Substring(1);

            // First, check basic identifier syntax (letters, digits, underscore, etc.)
            if (!SyntaxFacts.IsValidIdentifier(name))
                return false;

            // If it's a keyword and it was not escaped, it's invalid.
            if (!escaped && SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
                return false;

            return true;
        }
    }
}
