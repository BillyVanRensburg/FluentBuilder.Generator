// <copyright file="CollectionOptions.Parameters.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

namespace FluentBuilder.Generator.Parameters
{
    /// <summary>
    /// Configuration options for collection properties in fluent builders.
    /// </summary>
    public class CollectionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to generate the direct setter method (e.g., Items(value)).
        /// Default is true.
        /// </summary>
        public bool GenerateDirectSetter { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate the action setter method (e.g., Items(action)).
        /// Default is false.
        /// </summary>
        public bool GenerateActionSetter { get; set; }

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
        /// Gets or sets the custom validation message for count validation.
        /// </summary>
        public string? CountValidationMessage { get; set; }

        /// <summary>
        /// Gets a value indicating whether count validation is enabled.
        /// </summary>
        public bool HasCountValidation =>
            (MinCount >= 0 || MaxCount >= 0 || ExactCount >= 0) && GenerateCount;
    }
}