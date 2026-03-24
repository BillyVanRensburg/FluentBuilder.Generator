using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OrchestratorNotInitializedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBGEN002";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Generator orchestrator not initialized",
            messageFormat: "Generator orchestrator was not properly initialized for type '{0}'. This is an internal error in the FluentBuilder generator.",
            category: "General",
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
                if (builderAttribute == null)
                    return;

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeSyntax(ctx, builderAttribute),
                    SyntaxKind.ClassDeclaration,
                    SyntaxKind.RecordDeclaration);
            });
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context, INamedTypeSymbol builderAttribute)
        {
            var typeDecl = (TypeDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
            if (typeSymbol == null)
                return;

            // Check if the type has the [FluentBuilder] attribute.
            if (!typeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            // If the type is not nested, no problem.
            if (typeDecl.Parent is not TypeDeclarationSyntax containerDecl)
                return;

            // Walk up the syntax tree to check all containing types.
            var current = containerDecl;
            while (current != null)
            {
                if (!current.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    var location = typeDecl.Identifier.GetLocation();
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, typeSymbol.Name));
                    return;
                }
                current = current.Parent as TypeDeclarationSyntax;
            }
        }
    }
}
