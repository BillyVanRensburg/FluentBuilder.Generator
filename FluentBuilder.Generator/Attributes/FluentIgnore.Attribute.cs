// <copyright file="FluentIgnore.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Instructs the generator to ignore a specific member, constructor, or method.
    /// No fluent method will be created for it.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to any member that you do not want to be part of the fluent builder.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentIgnore]
    /// public string InternalId { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method |
                    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class FluentIgnoreAttribute : Attribute { }
}