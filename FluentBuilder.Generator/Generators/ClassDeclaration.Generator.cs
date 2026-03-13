// <copyright file="ClassDeclaration.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Parameters;
using System;
using System.Linq;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class ClassDeclarationGenerator
    {
        public static void GenerateClassDeclaration(
            StringBuilder sb,
            string indent,
            BuilderConfiguration config,
            string actualBuilderName)
        {
            string accessibility = config.BuilderAccessibility;

            // File-local types must be top-level in C#. If the builder is being generated as a nested
            // type (there are containing types), do not emit the 'file' accessibility because that
            // produces a file-local nested type which is invalid (CS9054). Fall back to 'private'
            // for nested builders to keep generated code compilable; analyzer handles reporting
            // unsupported file-scoped usage where appropriate.
            if (string.Equals(accessibility, "file", StringComparison.OrdinalIgnoreCase)
                && config.ContainingTypes != null && config.ContainingTypes.Count > 0)
            {
                accessibility = "private";
            }
            string modifier = config.IsPartial ? "partial" : "sealed";

            // If the target type is generic, the builder should also be generic
            if (config.TypeParameters.Count > 0)
            {
                var typeParams = string.Join(", ", config.TypeParameters.Select(t => StringCache.GetTypeDisplayName(t)));
                sb.AppendLine($"{indent}{accessibility} {modifier} class {actualBuilderName}<{typeParams}>");
            }
            else
            {
                sb.AppendLine($"{indent}{accessibility} {modifier} class {actualBuilderName}");
            }
            sb.AppendLine($"{indent}{{");
        }
    }
}
