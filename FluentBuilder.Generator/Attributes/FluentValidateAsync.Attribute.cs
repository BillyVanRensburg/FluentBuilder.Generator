// <copyright file="FluentValidateAsync.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies an asynchronous validator for a property or field.
    /// </summary>
    /// <remarks>
    /// The validator type must have a public parameterless constructor and a method
    /// with the specified name that takes the member's type and returns <see cref="System.Threading.Tasks.Task{TResult}"/> where TResult is <see cref="bool"/>.
    /// The generated builder will include an async validation method (e.g., <c>ValidateEmailAsync()</c>)
    /// that invokes the validator.
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentValidateAsync(typeof(EmailValidator), nameof(EmailValidator.IsValidAsync))]
    /// public string Email { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidateAsyncAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the validator.
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        /// Gets the name of the validation method on the validator.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateAsyncAttribute"/> class.
        /// </summary>
        /// <param name="validatorType">The type of the validator. Must have a public parameterless constructor.</param>
        /// <param name="methodName">The name of the validation method that accepts the member's value and returns <c>Task&lt;bool&gt;</c>.</param>
        public FluentValidateAsyncAttribute(Type validatorType, string methodName)
        {
            ValidatorType = validatorType;
            MethodName = methodName;
        }
    }
}