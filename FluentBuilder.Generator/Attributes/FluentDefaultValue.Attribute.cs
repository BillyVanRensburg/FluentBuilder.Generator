// <copyright file="FluentDefaultValue.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies a default value for a property or field in the generated builder.
    /// </summary>
    /// <remarks>
    /// The builder will initialize the member with this value unless it is explicitly set
    /// via a fluent method or constructor parameter.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentDefaultValue(42)]
    /// public int Age { get; set; }
    /// </code>
    /// The builder will set <c>Age</c> to 42 by default.
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentDefaultValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the default value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The default value for the member.</param>
        public FluentDefaultValueAttribute(object value)
        {
            Value = value;
        }
    }
}