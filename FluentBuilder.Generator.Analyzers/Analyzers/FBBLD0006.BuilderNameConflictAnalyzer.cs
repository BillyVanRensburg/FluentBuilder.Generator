using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BuilderNameConflictAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FBBLD0006";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                "Builder name conflicts with existing type",
                "The generated builder name '{0}' conflicts with an existing type '{1}' in {2}.",
                "FluentBuilder",
                DiagnosticSeverity.Error,
                true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var compilation = startContext.Compilation;

                var fluentBuilderAttr =
                    compilation.GetTypeByMetadataName("FluentBuilder.FluentBuilderAttribute");

                var fluentNameAttr =
                    compilation.GetTypeByMetadataName("FluentBuilder.FluentNameAttribute");

                if (fluentBuilderAttr == null)
                    return;

                startContext.RegisterCompilationEndAction(ctx =>
                    AnalyzeCompilation(ctx, compilation, fluentBuilderAttr, fluentNameAttr));
            });
        }


        private static void AnalyzeCompilation(
            CompilationAnalysisContext context,
            Compilation compilation,
            INamedTypeSymbol fluentBuilderAttr,
            INamedTypeSymbol? fluentNameAttr)
        {
            var allTypes = GetAllTypes(compilation.Assembly.GlobalNamespace);
            var fluentBuilderTypes = allTypes
                .Where(t => t.GetAttributes()
                    .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, fluentBuilderAttr)))
                .ToList();

            foreach (var sourceType in fluentBuilderTypes)
            {
            // Find the FluentBuilder attribute by name to be resilient across compilation contexts
            var attr = sourceType.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "FluentBuilderAttribute");

                if (attr == null)
                    continue;

            var builderName = GetBuilderName(sourceType);
                var builderNamespace = GetBuilderNamespace(sourceType, attr);

                if (sourceType.ContainingType != null)
                {
                    // Nested type - builder is placed in the container
                    var container = sourceType.ContainingType;

                // Search members of the container for conflicting nested types
                var conflictingTypes = container.GetTypeMembers()
                    .Where(t => t.Name == builderName && !SymbolEqualityComparer.Default.Equals(t, sourceType))
                    .ToList();

                    var sourceTree = sourceType.Locations.FirstOrDefault()?.SourceTree;
                    foreach (var conflict in conflictingTypes)
                    {
                        // Only consider conflicts declared in the same source tree as the target type
                        if (sourceTree == null || !conflict.Locations.Any(l => l.SourceTree == sourceTree))
                            continue;

                        var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? sourceType.Locations.FirstOrDefault();
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, builderName, conflict.Name, $"container '{container.Name}'"));
                    }
                }
                else
                {
                    // Top-level type
                    INamespaceSymbol? targetNamespace;
                    string locationDescription;

                    if (!string.IsNullOrEmpty(builderNamespace))
                    {
                        // Custom namespace specified
                        targetNamespace = GetNamespaceByName(compilation.GlobalNamespace, builderNamespace);
                        if (targetNamespace == null)
                        {
                            // Namespace doesn't exist, no conflict possible
                            continue;
                        }
                        locationDescription = $"namespace '{builderNamespace}'";
                    }
                    else
                    {
                        // Default namespace (same as target type)
                        targetNamespace = sourceType.ContainingNamespace;
                        locationDescription = $"namespace '{targetNamespace.ToDisplayString()}'";
                    }

                    var conflictingTypes = targetNamespace.GetTypeMembers(builderName)
                        .Where(t => !SymbolEqualityComparer.Default.Equals(t, sourceType))
                        .ToList();

                    var sourceTree = sourceType.Locations.FirstOrDefault()?.SourceTree;
                    foreach (var conflict in conflictingTypes)
                    {
                        // Only consider conflicts declared in the same source tree as the target type
                        if (sourceTree == null || !conflict.Locations.Any(l => l.SourceTree == sourceTree))
                            continue;

                        var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? sourceType.Locations.FirstOrDefault();
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, builderName, conflict.Name, locationDescription));
                    }
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                {
                    foreach (var t in GetAllTypes(nestedNs))
                        yield return t;
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;

                    foreach (var nested in GetNestedTypes(type))
                        yield return nested;
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
        {
            foreach (var nested in type.GetTypeMembers())
            {
                yield return nested;

                foreach (var deeper in GetNestedTypes(nested))
                    yield return deeper;
            }
        }

        private static string GetBuilderName(INamedTypeSymbol type)
        {
            // Try to find a FluentName attribute on the type and return its constructor argument
            var attr = type.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "FluentNameAttribute");

            if (attr?.ConstructorArguments.Length > 0)
            {
                var name = attr.ConstructorArguments[0].Value as string;
                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            return type.Name + "Builder";
        }

        private static string? GetBuilderNamespace(INamedTypeSymbol type, AttributeData fluentBuilderAttr)
        {
            // Look for BuilderNamespace named argument
            foreach (var arg in fluentBuilderAttr.NamedArguments)
            {
                if (arg.Key == "BuilderNamespace")
                {
                    return arg.Value.Value as string;
                }
            }

            return null;
        }

        private static INamespaceSymbol? GetNamespaceByName(INamespaceSymbol root, string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return root;

            var parts = namespaceName.Split('.');
            var current = root;

            foreach (var part in parts)
            {
                var found = false;
                foreach (var member in current.GetMembers())
                {
                    if (member is INamespaceSymbol ns && ns.Name == part)
                    {
                        current = ns;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return null;
            }

            return current;
        }
    }
}
