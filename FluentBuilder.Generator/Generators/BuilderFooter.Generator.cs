// <copyright file="BuilderFooter.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class BuilderFooterGenerator
    {
        public static void GenerateClassFooter(StringBuilder sb, string indent)
        {
            sb.AppendLine($"{indent}}}");
        }
    }
}