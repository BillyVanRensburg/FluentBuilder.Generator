// <copyright file="Namespace.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class NamespaceGenerator
    {
        public static void GenerateNamespaceStart(
            StringBuilder sb,
            INamedTypeSymbol typeSymbol,
            BuilderConfiguration config,
            out string indent)
        {
            // Use custom namespace if provided, otherwise fall back to target's namespace
            string ns = !string.IsNullOrEmpty(config.BuilderNamespace)
                ? config.BuilderNamespace!
                : typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;

            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                indent = "    ";
            }
            else
            {
                indent = "";
            }
        }

        public static void GenerateNamespaceEnd(
            StringBuilder sb,
            INamedTypeSymbol typeSymbol,
            BuilderConfiguration config)
        {
            string ns = !string.IsNullOrEmpty(config.BuilderNamespace)
                ? config.BuilderNamespace!
                : typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;

            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine("}");
            }
        }
    }
}