using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidAsyncMethodSignatureAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBAMD001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Invalid async method signature",
            messageFormat: "Async method '{0}' must return Task or Task<T>",
            category: "FluentAsyncMethod",
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
                var asyncMethodAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentAsyncMethodAttribute");
                var ignoreAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentIgnoreAttribute");
                if (builderAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, asyncMethodAttribute, ignoreAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(
            SymbolAnalysisContext context,
            INamedTypeSymbol builderAttribute,
            INamedTypeSymbol? asyncMethodAttribute,
            INamedTypeSymbol? ignoreAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only types with [FluentBuilder]
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            var taskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var taskGenericType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            foreach (var member in type.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

                // Skip static, non‑public, or ignored members
                if (method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
                    continue;
                if (ignoreAttribute != null && HasIgnoreAttribute(method, ignoreAttribute))
                    continue;

                // Check if method is async or has [FluentAsyncMethod]
                bool isAsync = method.IsAsync;
                bool hasAsyncAttr = asyncMethodAttribute != null &&
                                    method.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, asyncMethodAttribute));

                if (!isAsync && !hasAsyncAttr)
                    continue;

                // Check return type
                var returnType = method.ReturnType;
                bool isValid = false;

                if (returnType is INamedTypeSymbol namedReturn)
                {
                    if (taskType != null && SymbolEqualityComparer.Default.Equals(returnType, taskType))
                        isValid = true;
                    else if (taskGenericType != null && namedReturn.IsGenericType &&
                             SymbolEqualityComparer.Default.Equals(namedReturn.ConstructedFrom, taskGenericType))
                        isValid = true;
                }

                if (!isValid)
                {
                    var location = method.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    if (location != null)
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name));
                }
            }
        }

        private static bool HasIgnoreAttribute(ISymbol member, INamedTypeSymbol ignoreAttribute)
        {
            return member.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttribute));
        }
    }
}
