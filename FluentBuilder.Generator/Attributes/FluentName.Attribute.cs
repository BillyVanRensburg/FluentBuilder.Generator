// <copyright file="FluentName.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies a custom name for the generated fluent method corresponding to a member,
    /// or for the builder class itself when applied to a class.
    /// </summary>
    /// <remarks>
    /// <para>When applied to a class, it renames the generated builder class.</para>
    /// <para>When applied to a property, field, or method, it renames the corresponding fluent method.</para>
    /// The provided name must be a valid C# identifier.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentName("PersonBuilder")]
    /// public class Person { ... }
    /// 
    /// [FluentName("SetAge")]
    /// public int Age { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface |
                    AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method |
                    AttributeTargets.Constructor,
                    Inherited = false, AllowMultiple = false)]
    public sealed class FluentNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the custom name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The custom name. Must not be null, empty, or whitespace.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null, empty, or whitespace.</exception>
        public FluentNameAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("FluentName cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("FluentName cannot be whitespace only", nameof(name));

            Name = name;
        }
    }
}