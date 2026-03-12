// <copyright file="FluentBuilderAsyncSupport.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Enables and configures asynchronous support for the generated builder.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a class to generate async build methods, async fluent methods,
    /// and async validation methods. You can customize the method names and which features are generated.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentBuilder]
    /// [FluentBuilderAsyncSupport(AsyncBuildMethodName = "CreateAsync", GenerateCancellationTokens = true)]
    /// public class MyClass { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FluentBuilderAsyncSupportAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderAsyncSupportAttribute"/> class.
        /// </summary>
        public FluentBuilderAsyncSupportAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderAsyncSupportAttribute"/> class
        /// with a specified async build method name.
        /// </summary>
        /// <param name="asyncBuildMethodName">The name of the async build method (e.g., "BuildAsync").</param>
        public FluentBuilderAsyncSupportAttribute(string asyncBuildMethodName)
        {
            AsyncBuildMethodName = asyncBuildMethodName;
        }

        /// <summary>
        /// Gets or sets the name of the asynchronous build method. Default is "BuildAsync".
        /// </summary>
        public string AsyncBuildMethodName { get; set; } = "BuildAsync";

        /// <summary>
        /// Gets or sets the prefix used for generated async validation methods. Default is "Validate".
        /// </summary>
        public string AsyncValidationPrefix { get; set; } = "Validate";

        /// <summary>
        /// Gets or sets a value indicating whether to generate the async build method. Default is true.
        /// </summary>
        public bool GenerateAsyncBuild { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate async validation methods. Default is true.
        /// </summary>
        public bool GenerateAsyncValidation { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether async methods should include a
        /// <see cref="System.Threading.CancellationToken"/> parameter. Default is true.
        /// </summary>
        public bool GenerateCancellationTokens { get; set; } = true;
    }
}