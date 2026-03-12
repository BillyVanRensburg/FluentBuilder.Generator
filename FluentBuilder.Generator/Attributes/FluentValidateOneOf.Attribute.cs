// <copyright file="FluentValidateOneOf.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a property or field is one of a specified set of values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class FluentValidateOneOfAttribute : Attribute
    {
        /// <summary>
        /// Gets the allowed values.
        /// </summary>
        public object[] Values { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateOneOfAttribute"/> class.
        /// </summary>
        /// <param name="values">The array of allowed values.</param>
        public FluentValidateOneOfAttribute(params object[] values)
        {
            Values = values;
        }
    }
}