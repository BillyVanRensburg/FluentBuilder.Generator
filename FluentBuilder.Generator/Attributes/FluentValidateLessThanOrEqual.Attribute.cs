// <copyright file="FluentValidateLessThanOrEqual.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a property or field is less than or equal to a specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidateLessThanOrEqualAttribute : Attribute
    {
        /// <summary>
        /// Gets the threshold value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateLessThanOrEqualAttribute"/> class.
        /// </summary>
        /// <param name="value">The value that the member must be less than or equal to.</param>
        public FluentValidateLessThanOrEqualAttribute(object value)
        {
            Value = value;
        }
    }
}