// <copyright file="FluentValidatePhone.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Validates that a string property or field is a valid phone number.
    /// </summary>
    /// <remarks>
    /// Uses a simple regex pattern for basic phone number validation.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidatePhoneAttribute : FluentValidateAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidatePhoneAttribute"/> class.
        /// </summary>
        public FluentValidatePhoneAttribute()
        {
            RegexPattern = @"^[\+]?[1-9][\d]{0,15}$";
            CustomMessage = "Invalid phone number";
        }
    }
}