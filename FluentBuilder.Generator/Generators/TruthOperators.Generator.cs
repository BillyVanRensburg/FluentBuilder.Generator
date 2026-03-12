// <copyright file="TruthOperators.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    internal static class TruthOperatorsGenerator
    {
        public static void GenerateTruthOperatorField(StringBuilder sb, string indent)
        {
            sb.AppendLine($"{indent}    private bool _built;");
        }

        public static void GenerateTruthOperators(StringBuilder sb, string indent, string builderName)
        {
            sb.AppendLine($"{indent}    public static bool operator true({builderName} b) => b?._built == true;");
            sb.AppendLine($"{indent}    public static bool operator false({builderName} b) => b?._built != true;");
            sb.AppendLine($"{indent}    public static bool operator !({builderName} b) => b?._built != true;");
            sb.AppendLine();
        }
    }
}