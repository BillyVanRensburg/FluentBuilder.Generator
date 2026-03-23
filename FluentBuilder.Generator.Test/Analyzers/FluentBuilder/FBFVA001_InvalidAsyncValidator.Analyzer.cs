using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidAsyncValidatorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBFVA001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Invalid async validator",
            messageFormat: "Async validator for '{0}' has invalid configuration: {1}",
            category: "FluentValidateAsync",
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
                var validateAsyncAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentValidateAsyncAttribute");
                var taskType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var taskGenericType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
                var cancellationTokenType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

                if (builderAttribute == null || validateAsyncAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, validateAsyncAttribute, taskGenericType, cancellationTokenType),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(
            SymbolAnalysisContext context,
            INamedTypeSymbol builderAttribute,
            INamedTypeSymbol validateAsyncAttribute,
            INamedTypeSymbol? taskGenericType,
            INamedTypeSymbol? cancellationTokenType)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only types with [FluentBuilder]
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            foreach (var member in type.GetMembers())
            {
                // Only properties and fields
                if (member is not IPropertySymbol property && member is not IFieldSymbol field)
                    continue;

                if (member.DeclaredAccessibility != Accessibility.Public || member.IsStatic)
                    continue;

                var attr = member.GetAttributes()
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, validateAsyncAttribute));
                if (attr == null)
                    continue;

                // Get the constructor arguments (validatorType, methodName)
                var args = attr.ConstructorArguments;
                if (args.Length < 2)
                {
                    ReportDiagnostic(context, attr, member.Name, "Missing validator type or method name");
                    continue;
                }

                var validatorTypeSymbol = args[0].Value as INamedTypeSymbol;
                var methodName = args[1].Value as string;

                if (validatorTypeSymbol == null)
                {
                    ReportDiagnostic(context, attr, member.Name, "Validator type not found");
                    continue;
                }

                if (string.IsNullOrEmpty(methodName))
                {
                    ReportDiagnostic(context, attr, member.Name, "Method name is empty");
                    continue;
                }

                // 1. Check for public parameterless constructor
                var ctor = validatorTypeSymbol.Constructors
                    .FirstOrDefault(c => c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0);
                if (ctor == null)
                {
                    ReportDiagnostic(context, attr, member.Name, $"Validator type '{validatorTypeSymbol.Name}' must have a public parameterless constructor");
                    continue;
                }

                // 2. Find the validation method
                var methods = validatorTypeSymbol.GetMembers(methodName).OfType<IMethodSymbol>().ToList();
                if (methods.Count == 0)
                {
                    ReportDiagnostic(context, attr, member.Name, $"Method '{methodName}' not found on type '{validatorTypeSymbol.Name}'");
                    continue;
                }

                // Get the member's type (the value to validate)
                var valueType = member is IPropertySymbol p ? p.Type : (member as IFieldSymbol)?.Type;
                if (valueType == null) continue;

                bool foundValidMethod = false;
                foreach (var method in methods)
                {
                    if (method.DeclaredAccessibility != Accessibility.Public || method.IsStatic)
                        continue;

                    if (method.ReturnType is not INamedTypeSymbol returnType)
                        continue;

                    // Must return Task<bool>
                    bool isTaskBool = false;
                    if (taskGenericType != null &&
                        returnType.IsGenericType &&
                        SymbolEqualityComparer.Default.Equals(returnType.ConstructedFrom, taskGenericType) &&
                        returnType.TypeArguments.Length == 1 &&
                        returnType.TypeArguments[0].SpecialType == SpecialType.System_Boolean)
                    {
                        isTaskBool = true;
                    }

                    if (!isTaskBool)
                        continue;

                    // Method parameters
                    var parameters = method.Parameters;
                    if (parameters.Length == 1)
                    {
                        // Single parameter of the value type
                        if (SymbolEqualityComparer.Default.Equals(parameters[0].Type, valueType))
                        {
                            foundValidMethod = true;
                            break;
                        }
                    }
                    else if (parameters.Length == 2)
                    {
                        // Two parameters: value type and CancellationToken
                        if (SymbolEqualityComparer.Default.Equals(parameters[0].Type, valueType) &&
                            cancellationTokenType != null &&
                            SymbolEqualityComparer.Default.Equals(parameters[1].Type, cancellationTokenType))
                        {
                            foundValidMethod = true;
                            break;
                        }
                    }
                }

                if (!foundValidMethod)
                {
                    ReportDiagnostic(context, attr, member.Name,
                        $"Method '{methodName}' on '{validatorTypeSymbol.Name}' does not have the expected signature: public {methodName}({valueType.Name} value) : Task<bool> (or with CancellationToken)");
                }
            }
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, AttributeData attr, string memberName, string message)
        {
            var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                           ?? attr.AttributeClass?.Locations.FirstOrDefault();
            if (location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, memberName, message));
            }
        }
    }
}
