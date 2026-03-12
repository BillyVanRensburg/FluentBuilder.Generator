// <copyright file="FluentValidateUrl.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a string property or field is a valid URL.
    /// </summary>
    /// <remarks>
    /// Uses a regex pattern for basic URL validation.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidateUrlAttribute : FluentValidateAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateUrlAttribute"/> class.
        /// </summary>
        public FluentValidateUrlAttribute()
        {
            RegexPattern = @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$";
            CustomMessage = "Invalid URL";
        }
    }
}