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