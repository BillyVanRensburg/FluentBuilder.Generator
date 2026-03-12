// <copyright file="FluentImplicit.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Enables implicit conversion operators from the builder to the target type
    /// and optionally to <see cref="System.Threading.Tasks.Task{TResult}"/>.
    /// </summary>
    /// <remarks>
    /// When applied, the generated builder will include:
    /// <list type="bullet">
    /// <item><description><c>public static implicit operator TargetType(Builder builder)</c></description></item>
    /// <item><description>If async support is enabled, <c>public static implicit operator Task&lt;TargetType&gt;(Builder builder)</c></description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentBuilder]
    /// [FluentImplicit]
    /// public class Person { ... }
    /// </code>
    /// Allows writing <c>Person p = new PersonBuilder();</c>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FluentImplicitAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentImplicitAttribute"/> class.
        /// </summary>
        public FluentImplicitAttribute() { }
    }
}