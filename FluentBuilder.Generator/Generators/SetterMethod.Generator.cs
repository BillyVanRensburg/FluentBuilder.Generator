// <copyright file="SetterMethod.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// <summary>
    /// Provides methods to generate direct and action-based setter code for builder properties.
    /// </summary>
    internal static class SetterMethodGenerator
    {
        /// <summary>
        /// Emits a direct setter method that assigns a value to a field or property.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="indent">Current indentation string.</param>
        /// <param name="builderName">Name of the builder class.</param>
        /// <param name="methodName">Name of the setter method (e.g., "WithName").</param>
        /// <param name="typeName">Fully qualified type name of the parameter.</param>
        /// <param name="targetExpression">Expression that accesses the target field (e.g., "_name" or "_instance.Name").</param>
        /// <param name="memberName">Name of the member for tracking overridden state.</param>
        public static void EmitDirectSetter(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string typeName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} {methodName}({typeName} value)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        {targetExpression} = value;");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits an action-based setter that allows configuring a complex member via an action delegate.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="indent">Current indentation string.</param>
        /// <param name="builderName">Name of the builder class.</param>
        /// <param name="methodName">Name of the setter method (e.g., "WithName").</param>
        /// <param name="typeName">Fully qualified type name of the member.</param>
        /// <param name="targetExpression">Expression that accesses the target field (e.g., "_name" or "_instance.Name").</param>
        /// <param name="memberName">Name of the member for tracking overridden state.</param>
        /// <param name="initializeIfNull">If <c>true</c>, adds a null-check to initialize the target field.</param>
        public static void EmitActionSetter(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string typeName,
            string targetExpression,
            string memberName,
            bool initializeIfNull = false)
        {
            sb.AppendLine($"{indent}    public {builderName} {methodName}(System.Action<{typeName}> action)");
            sb.AppendLine($"{indent}    {{");
            if (initializeIfNull)
            {
                sb.AppendLine($"{indent}        if ({targetExpression} == null)");
                sb.AppendLine($"{indent}            {targetExpression} = new {typeName}();");
            }
            sb.AppendLine($"{indent}        action({targetExpression});");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }
    }
}