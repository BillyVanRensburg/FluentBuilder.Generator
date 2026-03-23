using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidDefaultValueAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBDEF001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Invalid default value type",
            messageFormat: "Default value '{0}' for member '{1}' cannot be converted to type '{2}'",
            category: "FluentDefaultValue",
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
                var defaultValueAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentDefaultValueAttribute");

                if (builderAttribute == null || defaultValueAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, defaultValueAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(
            SymbolAnalysisContext context,
            INamedTypeSymbol builderAttribute,
            INamedTypeSymbol defaultValueAttribute)
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
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, defaultValueAttribute));
                if (attr == null)
                    continue;

                // Get the constructor argument (the default value)
                var args = attr.ConstructorArguments;
                if (args.Length == 0)
                    continue;

                var constantValue = args[0];
                var memberType = member is IPropertySymbol prop ? prop.Type : (member as IFieldSymbol)?.Type;
                if (memberType == null) continue;

                // Check if the constant value can be assigned to the member type
                if (!IsValueAssignable(constantValue, memberType, context.Compilation))
                {
                    var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                   ?? member.Locations.FirstOrDefault()
                                   ?? type.Locations.FirstOrDefault();
                    if (location != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Rule, location,
                            constantValue.Value?.ToString() ?? "null",
                            member.Name,
                            memberType.ToDisplayString()));
                    }
                }
            }
        }

        private static bool IsValueAssignable(TypedConstant value, ITypeSymbol targetType, Compilation compilation)
        {
            // If value is null, it's assignable only if targetType is nullable (reference type or Nullable<T>)
            if (value.Value == null)
            {
                // Check for reference type
                if (targetType.IsReferenceType)
                    return true;

                // Check for Nullable<T>
                if (targetType is INamedTypeSymbol namedTarget && namedTarget.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                    return true;

                return false;
            }

            // For non-null values, check implicit conversion
            var conversion = compilation.ClassifyConversion(value.Type, targetType);
            return conversion.IsImplicit || conversion.IsIdentity;
        }
    }
}
