// <copyright file="CollectionHelperGenerator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Generates collection helper methods (Add, Remove, Clear, AddRange) for fluent builders.</summary>

using System.Text;

namespace FluentBuilder.Generator.Generator
{
    /// <summary>
    /// Provides methods to generate collection-specific helper methods for builder properties.
    /// </summary>
    internal static class CollectionHelperGenerator
    {
        /// <summary>
        /// Emits an Add method for a single-item collection (e.g., List&lt;T&gt;).
        /// </summary>
        public static void EmitAddForList(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string itemTypeName,
            string collectionTypeName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} Add{methodName}({itemTypeName} item)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if ({targetExpression} == null)");
            sb.AppendLine($"{indent}            {targetExpression} = new {collectionTypeName}();");
            sb.AppendLine($"{indent}        {targetExpression}.Add(item);");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits a Remove method for a single-item collection.
        /// </summary>
        public static void EmitRemoveForList(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string itemTypeName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} Remove{methodName}({itemTypeName} item)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if ({targetExpression} != null)");
            sb.AppendLine($"{indent}            {targetExpression}.Remove(item);");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits a Clear method for a collection.
        /// </summary>
        public static void EmitClear(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} Clear{methodName}()");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if ({targetExpression} != null)");
            sb.AppendLine($"{indent}            {targetExpression}.Clear();");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits an AddRange method for a collection that supports AddRange (e.g., List&lt;T&gt;).
        /// </summary>
        public static void EmitAddRangeForList(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string itemTypeName,
            string collectionTypeName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} Add{methodName}Range(System.Collections.Generic.IEnumerable<{itemTypeName}> items)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if ({targetExpression} == null)");
            sb.AppendLine($"{indent}            {targetExpression} = new {collectionTypeName}();");
            sb.AppendLine($"{indent}        {targetExpression}.AddRange(items);");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits an Add method for a dictionary (key-value pair).
        /// </summary>
        public static void EmitAddForDictionary(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string keyTypeName,
            string valueTypeName,
            string dictionaryTypeName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} Add{methodName}({keyTypeName} key, {valueTypeName} value)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if ({targetExpression} == null)");
            sb.AppendLine($"{indent}            {targetExpression} = new {dictionaryTypeName}();");
            sb.AppendLine($"{indent}        {targetExpression}.Add(key, value);");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Emits a Remove method for a dictionary (by key).
        /// </summary>
        public static void EmitRemoveForDictionary(
            StringBuilder sb,
            string indent,
            string builderName,
            string methodName,
            string keyTypeName,
            string targetExpression,
            string memberName)
        {
            sb.AppendLine($"{indent}    public {builderName} Remove{methodName}({keyTypeName} key)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if ({targetExpression} != null)");
            sb.AppendLine($"{indent}            {targetExpression}.Remove(key);");
            sb.AppendLine($"{indent}        _overridden.Add(\"{memberName}\");");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }
    }
}