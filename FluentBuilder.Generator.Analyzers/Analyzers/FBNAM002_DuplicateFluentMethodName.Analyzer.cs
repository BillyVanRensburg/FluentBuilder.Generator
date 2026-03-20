using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DuplicateFluentMethodNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBNAM002";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Duplicate fluent method name",
            messageFormat: "Duplicate fluent method name '{0}' in class '{1}'. All fluent method names must be unique.",
            category: "FluentName",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        // Built‑in method names that could conflict.
        private static readonly HashSet<string> ReservedNames = new HashSet<string>
        {
            "Build",
            "BuildAsync",
            "Validate",
            "ValidateAsync",
            "Equals",
            "GetHashCode",
            "ToString"
        };

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
                var ignoreAttribute = compilationContext.Compilation
                    .GetTypeByMetadataName("FluentBuilder.FluentIgnoreAttribute");

                if (builderAttribute == null)
                    return;

                compilationContext.RegisterSymbolAction(
                    ctx => Analyze(ctx, builderAttribute, fluentNameAttribute, ignoreAttribute),
                    SymbolKind.NamedType);
            });
        }

        private static void Analyze(
            SymbolAnalysisContext context,
            INamedTypeSymbol builderAttribute,
            INamedTypeSymbol? fluentNameAttribute,
            INamedTypeSymbol? ignoreAttribute)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Only classes/records with [FluentBuilder]
            if (!type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute)))
                return;

            // Extract MethodPrefix and MethodSuffix from the attribute (if present)
            string prefix = "With";
            string suffix = "";
            var attr = type.GetAttributes().First(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, builderAttribute));
            var prefixArg = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "MethodPrefix");
            if (prefixArg.Key != null && prefixArg.Value.Value is string p)
                prefix = p;
            var suffixArg = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "MethodSuffix");
            if (suffixArg.Key != null && suffixArg.Value.Value is string s)
                suffix = s;

            // Collect fluent names for eligible members
            var fluentNames = new Dictionary<string, ISymbol>(); // FIXED: removed invalid comparer

            // Process all eligible members: public instance properties, fields, and methods
            foreach (var member in type.GetMembers())
            {
                // Skip static, non‑public, or ignored members
                if (member.IsStatic)
                    continue;
                if (member.DeclaredAccessibility != Accessibility.Public)
                    continue;
                if (ignoreAttribute != null && HasIgnoreAttribute(member, ignoreAttribute))
                    continue;

                // Skip constructors and other non‑applicable members
                if (member is IMethodSymbol method)
                {
                    if (method.MethodKind != MethodKind.Ordinary)
                        continue;
                }
                else if (!(member is IPropertySymbol || member is IFieldSymbol))
                    continue;

                // Determine fluent name
                string fluentName = null;

                // Check for [FluentName] attribute
                if (fluentNameAttribute != null)
                {
                    var fluentNameAttr = member.GetAttributes()
                        .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, fluentNameAttribute));
                    if (fluentNameAttr != null)
                    {
                        var nameArg = fluentNameAttr.ConstructorArguments.FirstOrDefault();
                        if (nameArg.Kind == TypedConstantKind.Primitive && nameArg.Value is string name)
                            fluentName = name;
                    }
                }

                // If no custom name, generate default based on member kind
                if (fluentName == null)
                {
                    if (member is IPropertySymbol || member is IFieldSymbol)
                    {
                        fluentName = prefix + member.Name + suffix;
                    }
                    else if (member is IMethodSymbol)
                    {
                        fluentName = member.Name;
                    }
                }

                if (fluentName == null)
                    continue;

                // Check for reserved name conflict
                if (ReservedNames.Contains(fluentName))
                {
                    var location = member.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, fluentName, type.Name));
                    continue;
                }

                // Check for duplicate
                if (fluentNames.TryGetValue(fluentName, out var existingMember))
                {
                    var location = member.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault();
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, fluentName, type.Name));
                    // Also optionally report on the existing member? We'll just report the duplicate.
                }
                else
                {
                    fluentNames[fluentName] = member;
                }
            }
        }

        private static bool HasIgnoreAttribute(ISymbol member, INamedTypeSymbol ignoreAttribute)
        {
            return member.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttribute));
        }
    }
}
