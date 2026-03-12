// <copyright file="FluentValidate.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;

namespace FluentBuilder
{
    /// <summary>
    /// Base attribute for synchronous validation. Can be used directly or derived from.
    /// </summary>
    /// <remarks>
    /// When applied to a property or field, the generated builder will include a validation method
    /// (e.g., <c>ValidateAge()</c>) that checks the configured constraints and throws on failure.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentValidate(MinLength = 3, MaxLength = 50, Required = true)]
    /// public string Name { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class FluentValidateAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the minimum length for string members. Default is -1 (no minimum).
        /// </summary>
        public int MinLength { get; set; } = -1;

        /// <summary>
        /// Gets or sets the maximum length for string members. Default is -1 (no maximum).
        /// </summary>
        public int MaxLength { get; set; } = -1;

        /// <summary>
        /// Gets or sets a regular expression pattern for string members. Default is null.
        /// </summary>
        public string? RegexPattern { get; set; }

        /// <summary>
        /// Gets or sets the minimum value for numeric members. Default is <see cref="double.MinValue"/>.
        /// </summary>
        public double MinValue { get; set; } = double.MinValue;

        /// <summary>
        /// Gets or sets the maximum value for numeric members. Default is <see cref="double.MaxValue"/>.
        /// </summary>
        public double MaxValue { get; set; } = double.MaxValue;

        /// <summary>
        /// Gets or sets a value indicating whether the member is required (non‑null). Default is false.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a custom error message. If not provided, a default message is used.
        /// </summary>
        public string? CustomMessage { get; set; }
    }
}