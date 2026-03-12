// <copyright file="Generator.Configuration.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

namespace FluentBuilder.Generator.Configuration
{
    /// <summary>
    /// Global configuration for the FluentBuilder generator, read from MSBuild properties.
    /// Implemented as a record to provide value equality, ensuring that identical
    /// configurations (same property values) are considered equal even if they are
    /// different object instances.
    /// </summary>
    public record class GeneratorConfiguration
    {
        /// <summary>
        /// Default suffix for builder class names (e.g., "Builder"). Defaults to "Builder".
        /// </summary>
        public string DefaultBuilderSuffix { get; set; } = "Builder";

        /// <summary>
        /// Default namespace for generated builder classes. If null or empty, builders are placed
        /// in the same namespace as the target type. Defaults to null.
        /// </summary>
        public string? DefaultBuilderNamespace { get; set; }

        /// <summary>
        /// Default accessibility for generated builder classes (e.g., "public", "internal").
        /// Defaults to "public".
        /// </summary>
        public string DefaultBuilderAccessibility { get; set; } = "public";

        /// <summary>
        /// Default prefix for all fluent methods. Defaults to "With".
        /// </summary>
        public string DefaultMethodPrefix { get; set; } = "With";

        /// <summary>
        /// Default suffix for all fluent methods. Defaults to empty string.
        /// </summary>
        public string DefaultMethodSuffix { get; set; } = "";

        /// <summary>
        /// Default value for GeneratePartial on the builder class. Defaults to false.
        /// </summary>
        public bool DefaultGeneratePartial { get; set; } = false;

        /// <summary>
        /// Enables verbose logging from the generator. Defaults to false.
        /// </summary>
        public bool EnableLogging { get; set; } = false;
    }
}