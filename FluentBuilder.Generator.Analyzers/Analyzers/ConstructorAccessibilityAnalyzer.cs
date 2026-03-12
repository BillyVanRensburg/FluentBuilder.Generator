using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConstructorAccessibilityAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FB050";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Builder accessibility incompatible with constructor accessibility",
            messageFormat: "Builder accessibility '{0}' is more permissive than any accessible constructor of type '{1}'. The builder cannot instantiate the type.",
            category: "FluentBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The builder must have access to at least one constructor of the target type. If the builder is more accessible than all constructors, instantiation will fail.");

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
            var namedType = (INamedTypeSymbol)context.Symbol;

            if (namedType.TypeKind != TypeKind.Class && !namedType.IsRecord)
                return;

            var attribute = namedType.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
            if (attribute == null)
                return;

            string builderAccessibilityName = GetBuilderAccessibilityFromAttribute(attribute) ?? "Public";

            // Find the most accessible constructor (excluding static constructors)
            var constructors = namedType.Constructors.Where(c => !c.IsStatic).ToList();
            if (constructors.Count == 0)
            {
                // No constructor? Possibly a record with primary constructor. For records, we assume they have an accessible constructor.
                if (namedType.IsRecord)
                    return;

                // Should not happen, but if no constructor, report error.
                var location = attribute.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? namedType.Locations.FirstOrDefault();
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, builderAccessibilityName, namedType.Name));
                return;
            }

            int builderLevel = GetAccessibilityLevel(builderAccessibilityName);
            bool hasCompatibleCtor = constructors.Any(ctor =>
            {
                int ctorLevel = GetAccessibilityLevel(ctor.DeclaredAccessibility);
                return ctorLevel >= builderLevel;
            });

            if (!hasCompatibleCtor)
            {
                var location = attribute.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? namedType.Locations.FirstOrDefault();
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, builderAccessibilityName, namedType.Name));
            }
        }

        private static string? GetBuilderAccessibilityFromAttribute(AttributeData attribute)
        {
            var accessibilityArg = attribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "BuilderAccessibility");
            if (accessibilityArg.Key == null)
                return null; // default Public

            var typedConstant = accessibilityArg.Value;
            if (typedConstant.Kind != TypedConstantKind.Enum || !(typedConstant.Type is INamedTypeSymbol enumType))
                return null;

            var intValue = typedConstant.Value is int intVal ? intVal : Convert.ToInt32(typedConstant.Value);
            var member = enumType.GetMembers().OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue?.Equals(intValue) == true);
            return member?.Name;
        }

        private static int GetAccessibilityLevel(string name) => name switch
        {
            "File" => 0,
            "Private" => 1,
            "Protected" => 2,
            "Internal" => 3,
            "PrivateProtected" => 3,
            "ProtectedInternal" => 4,
            "Public" => 5,
            _ => 5
        };

        private static int GetAccessibilityLevel(Accessibility accessibility) => accessibility switch
        {
            Accessibility.NotApplicable => 5,
            Accessibility.Private => 1,
            Accessibility.Protected => 2,
            Accessibility.Internal => 3,
            Accessibility.ProtectedAndInternal => 3, // private protected
            Accessibility.ProtectedOrInternal => 4,
            Accessibility.Public => 5,
            _ => 5
        };
    }
}