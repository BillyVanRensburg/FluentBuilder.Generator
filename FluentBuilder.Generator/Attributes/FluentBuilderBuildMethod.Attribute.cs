// <copyright file="FluentBuilderBuildMethod.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies a custom name for the build method in the generated builder.
    /// </summary>
    /// <remarks>
    /// By default, the build method is named "Build". Apply this attribute to the target class
    /// to change the method name.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentBuilder]
    /// [FluentBuilderBuildMethod("Create")]
    /// public class Order { ... }
    /// </code>
    /// Generates a builder with a <c>Create()</c> method instead of <c>Build()</c>.
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FluentBuilderBuildMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets the custom build method name.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderBuildMethodAttribute"/> class.
        /// </summary>
        /// <param name="methodName">The desired name of the build method. Defaults to "Build".</param>
        public FluentBuilderBuildMethodAttribute(string methodName = "Build")
        {
            MethodName = methodName;
        }
    }
}