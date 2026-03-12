// <copyright file="CountValidation.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Parameters;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    /// <summary>
    /// Provides methods to generate count validation logic for collection members
    /// within the Build method of a fluent builder.
    /// </summary>
    internal static class CountValidationGenerator
    {
        /// <summary>
        /// Emits count validation code for a collection in the Build method.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="target">The expression accessing the collection field (e.g., "_items").</param>
        /// <param name="name">The name of the collection member (used in error messages).</param>
        /// <param name="options">Collection options containing validation rules (<see cref="CollectionOptions.HasCountValidation"/>).</param>
        /// <remarks>
        /// This method generates if-blocks that throw <see cref="System.InvalidOperationException"/>
        /// when the collection count does not satisfy the specified constraints (exact count, min, max).
        /// The validation is only emitted if <see cref="CollectionOptions.HasCountValidation"/> is <c>true</c>.
        /// </remarks>
        public static void EmitCountValidation(
            StringBuilder sb,
            string indent,
            string target,
            string name,
            CollectionOptions options)
        {
            if (sb == null || string.IsNullOrEmpty(target) || !options.HasCountValidation)
                return;

            string message = options.CountValidationMessage ??
                (options.ExactCount >= 0
                    ? $"Collection '{name}' must contain exactly {options.ExactCount} item(s)"
                    : $"Collection '{name}' must contain between {options.MinCount} and {options.MaxCount} item(s)");

            sb.AppendLine($"{indent}        // Count validation for {name}");

            if (options.ExactCount >= 0)
            {
                sb.AppendLine($"{indent}        if ({target}?.Count != {options.ExactCount})");
                sb.AppendLine($"{indent}            throw new System.InvalidOperationException(\"{EscapeMessage(message)}\");");
            }
            else
            {
                if (options.MinCount >= 0)
                {
                    sb.AppendLine($"{indent}        if ({target}?.Count < {options.MinCount})");
                    sb.AppendLine($"{indent}            throw new System.InvalidOperationException(\"{EscapeMessage(message)}\");");
                }

                if (options.MaxCount >= 0)
                {
                    sb.AppendLine($"{indent}        if ({target}?.Count > {options.MaxCount})");
                    sb.AppendLine($"{indent}            throw new System.InvalidOperationException(\"{EscapeMessage(message)}\");");
                }
            }
        }

        /// <summary>
        /// Escapes a message string for safe inclusion in a generated C# string literal.
        /// </summary>
        /// <param name="message">The original message.</param>
        /// <returns>The message with double quotes escaped.</returns>
        private static string EscapeMessage(string message)
        {
            return message?.Replace("\"", "\\\"") ?? string.Empty;
        }
    }
}