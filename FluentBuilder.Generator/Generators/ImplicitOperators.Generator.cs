// <copyright file="ImplicitOperators.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using Microsoft.CodeAnalysis;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class ImplicitOperatorsGenerator
    {
        public static void GenerateImplicitOperator(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            string builderName,
            string buildMethodName)
        {
            sb.AppendLine($"{indent}    public static implicit operator {typeSymbol.Name}({builderName} builder)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if (builder is null) throw new System.ArgumentNullException(nameof(builder));");
            sb.AppendLine($"{indent}        return builder.{buildMethodName}();");
            sb.AppendLine($"{indent}    }}");
        }

        public static void GenerateAsyncImplicitOperator(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            string builderName,
            string asyncBuildMethodName)
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}    public static implicit operator System.Threading.Tasks.Task<{typeSymbol.Name}>({builderName} builder)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if (builder is null) throw new System.ArgumentNullException(nameof(builder));");
            sb.AppendLine($"{indent}        return builder.{asyncBuildMethodName}();");
            sb.AppendLine($"{indent}    }}");
        }
    }
}