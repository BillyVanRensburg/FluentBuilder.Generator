// <copyright file="FluentBuilder.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Marks a class or record for fluent builder generation.
    /// </summary>
    /// <remarks>
    /// The generator will create a nested or separate builder class with fluent methods
    /// for all buildable members (public instance properties and fields). You can customize
    /// the builder's name, accessibility, namespace, and method naming conventions.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentBuilder]
    /// public class Person { ... }
    /// </code>
    /// Generates a <c>PersonBuilder</c> class.
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FluentBuilderAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the generated builder class should be partial.
        /// Default is <see langword="false"/>.
        /// </summary>
        public bool GeneratePartial { get; set; }

        /// <summary>
        /// Gets or sets the accessibility modifier for the generated builder class.
        /// Default is <see cref="BuilderAccessibility.Public"/>.
        /// </summary>
        /// <remarks>
        /// Not all values are valid in every context:
        /// <list type="bullet">
        ///   <item><description>For top‑level types, only <see cref="BuilderAccessibility.Public"/> and <see cref="BuilderAccessibility.Internal"/> are allowed.</description></item>
        ///   <item><description>For nested types, any value is permitted, subject to C# accessibility rules.</description></item>
        ///   <item><description>The <see cref="BuilderAccessibility.File"/> value is only valid for top‑level types (C# 11+).</description></item>
        /// </list>
        /// </remarks>
        public BuilderAccessibility BuilderAccessibility { get; set; } = BuilderAccessibility.Public;

        /// <summary>
        /// Gets or sets the namespace in which to place the generated builder class.
        /// If not specified, the builder will be placed in the same namespace as the target type.
        /// </summary>
        public string? BuilderNamespace { get; set; }

        /// <summary>
        /// Gets or sets an optional prefix to apply to all fluent method names (e.g., "With").
        /// Must be a valid C# identifier or empty.
        /// </summary>
        public string MethodPrefix { get; set; } = "";

        /// <summary>
        /// Gets or sets an optional suffix to apply to all fluent method names (e.g., "Async").
        /// Must be a valid C# identifier or empty.
        /// </summary>
        public string MethodSuffix { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderAttribute"/> class.
        /// </summary>
        public FluentBuilderAttribute() { }
    }
}