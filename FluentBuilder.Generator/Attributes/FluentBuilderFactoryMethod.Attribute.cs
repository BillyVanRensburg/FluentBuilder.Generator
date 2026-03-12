// <copyright file="FluentBuilderFactoryMethod.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies a static factory method to be used instead of a constructor when creating instances
    /// in the generated builder.
    /// </summary>
    /// <remarks>
    /// The factory method must be public, static, parameterless, and return the target type.
    /// This attribute is ignored for records or classes with init‑only properties.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentBuilder]
    /// [FluentBuilderFactoryMethod("Create")]
    /// public class MyClass
    /// {
    ///     public static MyClass Create() => new MyClass();
    /// }
    /// </code>
    /// The builder will call <c>MyClass.Create()</c> instead of <c>new MyClass()</c>.
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FluentBuilderFactoryMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the factory method. Defaults to "Create" if not specified.
        /// </summary>
        public string? MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderFactoryMethodAttribute"/> class.
        /// </summary>
        public FluentBuilderFactoryMethodAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderFactoryMethodAttribute"/> class
        /// with the specified factory method name.
        /// </summary>
        /// <param name="methodName">The name of the static factory method.</param>
        public FluentBuilderFactoryMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}