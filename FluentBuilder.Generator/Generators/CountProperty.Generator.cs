// <copyright file="CountProperty.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Provides methods to generate a Count property for collection members.
    /// </summary>
    internal static class CountPropertyGenerator
    {
        /// <summary>
        /// Emits a read-only Count property for a collection member.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="indent">Current indentation string.</param>
        /// <param name="propertyName">Name of the property (e.g., "ItemsCount").</param>
        /// <param name="targetExpression">Expression that accesses the collection field (e.g., "_items").</param>
        public static void EmitCountProperty(
            StringBuilder sb,
            string indent,
            string propertyName,
            string targetExpression)
        {
            sb.AppendLine($"{indent}    public int {propertyName} => {targetExpression}?.Count ?? 0;");
            sb.AppendLine();
        }
    }
}