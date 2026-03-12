// <copyright file="FluentValidateRange.Attribute.cs.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a numeric property or field is within a specified inclusive range.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidateRangeAttribute : FluentValidateAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateRangeAttribute"/> class for integer range.
        /// </summary>
        /// <param name="min">Minimum allowed value (inclusive).</param>
        /// <param name="max">Maximum allowed value (inclusive).</param>
        public FluentValidateRangeAttribute(int min, int max)
        {
            MinValue = min;
            MaxValue = max;
            CustomMessage = $"Value must be between {min} and {max}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateRangeAttribute"/> class for double range.
        /// </summary>
        /// <param name="min">Minimum allowed value (inclusive).</param>
        /// <param name="max">Maximum allowed value (inclusive).</param>
        public FluentValidateRangeAttribute(double min, double max)
        {
            MinValue = min;
            MaxValue = max;
            CustomMessage = $"Value must be between {min} and {max}";
        }
    }
}