// <copyright file="NestedBuilder.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Provides methods to generate code for configuring nested builders.
    /// </summary>
    internal static class NestedBuilderGenerator
    {
        /// <summary>
        /// Emits a method that configures a nested builder for a complex property.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="indent">Current indentation string.</param>
        /// <param name="builderName">Name of the builder class.</param>
        /// <param name="methodName">Name of the method (e.g., "WithAddress").</param>
        /// <param name="nestedBuilderType">Fully qualified name of the nested builder type.</param>
        /// <param name="targetExpression">Expression that accesses the target field (e.g., "_address").</param>
        /// <param name="buildMethodName">Name of the Build method on the nested builder (typically "Build").</param>
        /// <param name="memberName">Name of the member for tracking overridden state.</param>
        public static void EmitNestedBuilderMethod(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string nestedBuilderType,
            string targetExpression,
            string buildMethodName,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} {methodName}(System.Action<{nestedBuilderType}> configure)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var nestedBuilder = new {nestedBuilderType}();");
            sb.AppendLine($"{indent}        configure(nestedBuilder);");
            sb.AppendLine($"{indent}        {targetExpression} = nestedBuilder.{buildMethodName}();");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits a method that configures and adds an element to a collection using its own builder.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="indent">Current indentation string.</param>
        /// <param name="builderName">Name of the builder class.</param>
        /// <param name="methodName">Name of the method (e.g., "AddItem").</param>
        /// <param name="elementBuilderType">Fully qualified name of the builder for the element type.</param>
        /// <param name="collectionTypeName">Fully qualified name of the collection type.</param>
        /// <param name="targetExpression">Expression that accesses the collection field (e.g., "_items").</param>
        /// <param name="buildMethodName">Name of the Build method on the element builder.</param>
        /// <param name="memberName">Name of the collection member for tracking overridden state.</param>
        public static void EmitCollectionElementBuilderMethod(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string elementBuilderType,
            string collectionTypeName,
            string targetExpression,
            string buildMethodName,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} {methodName}(System.Action<{elementBuilderType}> configure)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if (configure == null) throw new System.ArgumentNullException(nameof(configure));");
            sb.AppendLine($"{indent}        var elementBuilder = new {elementBuilderType}();");
            sb.AppendLine($"{indent}        configure(elementBuilder);");
            sb.AppendLine($"{indent}        var element = elementBuilder.{buildMethodName}();");
            sb.AppendLine($"{indent}        if ({targetExpression} == null)");
            sb.AppendLine($"{indent}            {targetExpression} = new {collectionTypeName}();");
            sb.AppendLine($"{indent}        {targetExpression}.Add(element);");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }
    }
}