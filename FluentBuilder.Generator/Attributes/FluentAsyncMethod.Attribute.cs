// <copyright file="FluentAsyncMethod.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies the synchronous counterpart of an async method in the target class.
    /// This allows the generator to create a fluent method with the original name
    /// that calls the async method internally.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to async methods in the target class to control the name
    /// of the generated fluent method. The generator will produce a synchronous-looking
    /// method (without the "Async" suffix) that internally awaits the async method.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentAsyncMethod(SyncMethodName = "Load")]
    /// public async Task LoadAsync() { ... }
    /// </code>
    /// This generates a fluent method <c>Load()</c> that calls <c>LoadAsync()</c>.
    /// </example>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FluentAsyncMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the synchronous fluent method to generate.
        /// If not specified, the generator removes the "Async" suffix from the method name.
        /// </summary>
        public string SyncMethodName { get; set; } = "";
    }
}