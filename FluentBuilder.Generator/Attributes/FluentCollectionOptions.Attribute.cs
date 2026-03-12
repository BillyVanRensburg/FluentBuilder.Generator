// <copyright file="FluentCollectionOptions.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Configures how a collection property or field is handled in the generated builder.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a collection member to enable additional fluent methods
    /// such as Add, Remove, Clear, AddRange, and count validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentCollectionOptions(GenerateAdd = true, GenerateRemove = true, GenerateCount = true)]
    /// public List&lt;string&gt; Tags { get; set; }
    /// </code>
    /// Generates <c>AddTag</c>, <c>RemoveTag</c>, and a <c>TagsCount</c> property.
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentCollectionOptionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether to generate the direct setter method (e.g., Items(value)).
        /// Default is true.
        /// </summary>
        public bool GenerateDirectSetter { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate the action setter method (e.g., Items(action)).
        /// Default is true.
        /// </summary>
        public bool GenerateActionSetter { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate Add methods for the collection.
        /// Default is false.
        /// </summary>
        public bool GenerateAdd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate Remove methods for the collection.
        /// Default is false.
        /// </summary>
        public bool GenerateRemove { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate Clear methods for the collection.
        /// Default is false.
        /// </summary>
        public bool GenerateClear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate AddRange methods for List collections.
        /// Default is false.
        /// </summary>
        public bool GenerateAddRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate a Count property and validation.
        /// Default is false.
        /// </summary>
        public bool GenerateCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of items required (only used if <see cref="GenerateCount"/> is true).
        /// Default is -1 (no minimum).
        /// </summary>
        public int MinCount { get; set; } = -1;

        /// <summary>
        /// Gets or sets the maximum number of items allowed (only used if <see cref="GenerateCount"/> is true).
        /// Default is -1 (no maximum).
        /// </summary>
        public int MaxCount { get; set; } = -1;

        /// <summary>
        /// Gets or sets the exact number of items required (only used if <see cref="GenerateCount"/> is true).
        /// If set, this overrides <see cref="MinCount"/> and <see cref="MaxCount"/>.
        /// Default is -1 (no exact count).
        /// </summary>
        public int ExactCount { get; set; } = -1;

        /// <summary>
        /// Gets or sets a custom validation message for count validation.
        /// </summary>
        public string? CountValidationMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentCollectionOptionsAttribute"/> class.
        /// </summary>
        public FluentCollectionOptionsAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentCollectionOptionsAttribute"/> class
        /// with common options.
        /// </summary>
        /// <param name="generateDirectSetter">Whether to generate the direct setter.</param>
        /// <param name="generateActionSetter">Whether to generate the action setter.</param>
        /// <param name="generateAdd">Whether to generate Add methods.</param>
        /// <param name="generateRemove">Whether to generate Remove methods.</param>
        /// <param name="generateClear">Whether to generate Clear methods.</param>
        /// <param name="generateAddRange">Whether to generate AddRange methods.</param>
        /// <param name="generateCount">Whether to generate Count property and validation.</param>
        public FluentCollectionOptionsAttribute(
            bool generateDirectSetter = true,
            bool generateActionSetter = true,
            bool generateAdd = false,
            bool generateRemove = false,
            bool generateClear = false,
            bool generateAddRange = false,
            bool generateCount = false)
        {
            GenerateDirectSetter = generateDirectSetter;
            GenerateActionSetter = generateActionSetter;
            GenerateAdd = generateAdd;
            GenerateRemove = generateRemove;
            GenerateClear = generateClear;
            GenerateAddRange = generateAddRange;
            GenerateCount = generateCount;
        }
    }
}