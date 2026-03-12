// <copyright file="FluentValidateNotEqual.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a property or field is not equal to a specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidateNotEqualAttribute : Attribute
    {
        /// <summary>
        /// Gets the forbidden value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateNotEqualAttribute"/> class.
        /// </summary>
        /// <param name="value">The value that the member must not equal.</param>
        public FluentValidateNotEqualAttribute(object value)
        {
            Value = value;
        }
    }
}