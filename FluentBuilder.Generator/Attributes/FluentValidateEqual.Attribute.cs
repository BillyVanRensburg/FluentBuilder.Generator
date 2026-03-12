// <copyright file="FluentValidateEqual.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a property or field equals a specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidateEqualAttribute : Attribute
    {
        /// <summary>
        /// Gets the value that the member must equal.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateEqualAttribute"/> class.
        /// </summary>
        /// <param name="value">The required value.</param>
        public FluentValidateEqualAttribute(object value)
        {
            Value = value;
        }
    }
}