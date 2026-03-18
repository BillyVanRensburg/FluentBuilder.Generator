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
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            // Find [FluentBuilder] attribute by full name
            var fluentBuilderAttr = type.GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "FluentBuilder.FluentBuilderAttribute");

            if (fluentBuilderAttr == null)
                return;

            var builderName = GetBuilderName(type);
            var builderNamespace = GetBuilderNamespace(type, fluentBuilderAttr);

            // Nested type scenario
            if (type.ContainingType != null)
            {
                var container = type.ContainingType;

                var conflicts = container.GetTypeMembers()
                    .Where(t =>
                        t.Name == builderName &&
                        !SymbolEqualityComparer.Default.Equals(t, type));

                foreach (var conflict in conflicts)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            type.Locations.FirstOrDefault(),
                            builderName,
                            conflict.Name,
                            $"container '{container.Name}'"));
                }
            }
            else
            {
                // Top-level type
                INamespaceSymbol? targetNamespace;
                string locationDescription;

                if (!string.IsNullOrEmpty(builderNamespace))
                {
                    targetNamespace = GetNamespaceByName(
                        context.Compilation.GlobalNamespace,
                        builderNamespace);

                    if (targetNamespace == null)
                        return;

                    locationDescription = $"namespace '{builderNamespace}'";
                }
                else
                {
                    targetNamespace = type.ContainingNamespace;
                    locationDescription = $"namespace '{targetNamespace.ToDisplayString()}'";
                }

                var conflicts = targetNamespace.GetTypeMembers(builderName)
                    .Where(t =>
                        !SymbolEqualityComparer.Default.Equals(t, type));

                foreach (var conflict in conflicts)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            type.Locations.FirstOrDefault(),
                            builderName,
                            conflict.Name,
                            locationDescription));
                }
            }
        }

        private static string GetBuilderName(INamedTypeSymbol type)
        {
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

        private static string? GetBuilderNamespace(
            INamedTypeSymbol type,
            AttributeData fluentBuilderAttr)
        {
            foreach (var arg in fluentBuilderAttr.NamedArguments)
            {
                if (arg.Key == "BuilderNamespace")
                    return arg.Value.Value as string;
            }

            return null;
        }

        private static INamespaceSymbol? GetNamespaceByName(
            INamespaceSymbol root,
            string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return root;

            var parts = namespaceName.Split('.');
            var current = root;

            foreach (var part in parts)
            {
                var next = current.GetMembers()
                    .OfType<INamespaceSymbol>()
                    .FirstOrDefault(n => n.Name == part);

                if (next == null)
                    return null;

                current = next;
            }

            return current;
        }
    }
}
