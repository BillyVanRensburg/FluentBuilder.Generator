// <copyright file="BuildPropertyNames.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Contains constants for MSBuild property names used by the FluentBuilder generator.</summary>

namespace FluentBuilder.Generator.Constants
{
    /// <summary>
    /// Defines the MSBuild property names that can be used to configure the FluentBuilder source generator.
    /// </summary>
    public static class BuildPropertyNames
    {
        /// <summary>
        /// MSBuild property to set the default suffix for builder class names (e.g., "Builder").
        /// </summary>
        public const string DefaultBuilderSuffix = "build_property.FluentBuilder_DefaultBuilderSuffix";

        /// <summary>
        /// MSBuild property to set the default namespace for generated builder classes.
        /// </summary>
        public const string DefaultBuilderNamespace = "build_property.FluentBuilder_DefaultBuilderNamespace";

        /// <summary>
        /// MSBuild property to set the default accessibility for generated builder classes (e.g., "public", "internal").
        /// </summary>
        public const string DefaultBuilderAccessibility = "build_property.FluentBuilder_DefaultBuilderAccessibility";

        /// <summary>
        /// MSBuild property to set the default prefix for fluent setter methods.
        /// </summary>
        public const string DefaultMethodPrefix = "build_property.FluentBuilder_DefaultMethodPrefix";

        /// <summary>
        /// MSBuild property to set the default suffix for fluent setter methods.
        /// </summary>
        public const string DefaultMethodSuffix = "build_property.FluentBuilder_DefaultMethodSuffix";

        /// <summary>
        /// MSBuild property to control whether generated builder classes should be partial by default.
        /// </summary>
        public const string DefaultGeneratePartial = "build_property.FluentBuilder_DefaultGeneratePartial";

        /// <summary>
        /// MSBuild property to enable or disable diagnostic logging within the generator.
        /// </summary>
        public const string EnableLogging = "build_property.FluentBuilder_EnableLogging";
    }
}