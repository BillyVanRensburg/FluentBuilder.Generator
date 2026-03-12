// <copyright file="FluentTruthOperator.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Enables truth operators (`true`, `false`, `!`) on the generated builder.
    /// </summary>
    /// <remarks>
    /// When applied, the builder gains a private `_built` field and the operators
    /// return true if `Build()` has been called.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentBuilder]
    /// [FluentTruthOperator]
    /// public class MyClass { ... }
    /// 
    /// // Usage:
    /// var builder = new MyClassBuilder();
    /// if (builder) { /* Build not called yet – false */ }
    /// builder.Build();
    /// if (builder) { /* Now true */ }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FluentTruthOperatorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentTruthOperatorAttribute"/> class.
        /// </summary>
        public FluentTruthOperatorAttribute() { }
    }
}