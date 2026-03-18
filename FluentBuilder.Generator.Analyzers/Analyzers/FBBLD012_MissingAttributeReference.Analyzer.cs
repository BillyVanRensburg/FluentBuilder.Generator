using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MissingAttributeReferenceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD012";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Missing required assembly reference",
            messageFormat: "Cannot find FluentBuilder attributes. Please add a reference to the FluentBuilder assembly.",
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
                // Try to get the attribute symbol. If it exists, no diagnostic needed.
                var builderAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentBuilderAttribute");
                if (builderAttribute != null)
                    return; // Attribute found, nothing to report.

                // Attribute not found – register a syntax action to find its usage.
                compilationContext.RegisterSyntaxNodeAction(
                    AnalyzeAttribute,
                    SyntaxKind.Attribute);
            });
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attributeSyntax = (AttributeSyntax)context.Node;

            // Check if the attribute name is one of the FluentBuilder attributes.
            // We look for simple names like "FluentBuilder", "FluentName", etc.
            var name = attributeSyntax.Name.ToString();
            if (!IsFluentBuilderAttributeName(name))
                return;

            // If we are here, the attribute symbol is missing (already confirmed in compilation start).
            // Report diagnostic on the attribute name.
            var location = attributeSyntax.Name.GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }

        private static bool IsFluentBuilderAttributeName(string name)
        {
            // List of known FluentBuilder attribute names (without "Attribute" suffix).
            return name == "FluentBuilder" ||
                   name == "FluentName" ||
                   name == "FluentIgnore" ||
                   name == "FluentInclude" ||
                   name == "FluentDefaultValue" ||
                   name == "FluentBuilderBuildMethod" ||
                   name == "FluentBuilderFactoryMethod" ||
                   name == "FluentImplicit" ||
                   name == "FluentTruthOperator" ||
                   name == "FluentCollectionOptions" ||
                   name == "FluentValidate" ||
                   name == "FluentValidateEmail" ||
                   name == "FluentValidatePhone" ||
                   name == "FluentValidateUrl" ||
                   name == "FluentValidateRange" ||
                   name == "FluentValidateEqual" ||
                   name == "FluentValidateNotEqual" ||
                   name == "FluentValidateGreaterThan" ||
                   name == "FluentValidateGreaterThanOrEqual" ||
                   name == "FluentValidateLessThan" ||
                   name == "FluentValidateLessThanOrEqual" ||
                   name == "FluentValidateOneOf" ||
                   name == "FluentValidateWith" ||
                   name == "FluentValidateAsync" ||
                   name == "FluentValidationMethod" ||
                   name == "FluentBuilderAsyncSupport" ||
                   name == "FluentAsyncMethod";
        }
    }
}
