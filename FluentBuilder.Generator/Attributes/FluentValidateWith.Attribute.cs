// <copyright file="FluentValidateWith.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Specifies a custom validator for a property or field.
    /// </summary>
    /// <remarks>
    /// The validator type must have a public parameterless constructor and a method
    /// with the specified name that takes the member's value and returns <see cref="bool"/>.
    /// The generated builder will include a validation method (e.g., <c>ValidateAge()</c>)
    /// that instantiates the validator and calls its method, throwing an exception if it returns false.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class AgeValidator
    /// {
    ///     public bool IsValid(int age) => age >= 18;
    /// }
    /// 
    /// [FluentValidateWith(typeof(AgeValidator), MethodName = "IsValid")]
    /// public int Age { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class FluentValidateWithAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the validator.
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        /// Gets or sets the name of the validation method on the validator.
        /// Default is "Validate".
        /// </summary>
        public string MethodName { get; set; } = "Validate";

        /// <summary>
        /// Gets or sets a custom error message.
        /// </summary>
        public string? CustomMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidateWithAttribute"/> class.
        /// </summary>
        /// <param name="validatorType">The type of the validator. Must have a public parameterless constructor.</param>
        public FluentValidateWithAttribute(Type validatorType)
        {
            ValidatorType = validatorType;
        }
    }
}