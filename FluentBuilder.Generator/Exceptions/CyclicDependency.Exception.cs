// <copyright file="CyclicDependency.Exception.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;

namespace FluentBuilder.Generator.Exceptions
{
    /// <summary>
    /// Exception thrown when a cyclic dependency is detected during the generation of a builder.
    /// This typically occurs when types reference each other in a loop, making it impossible
    /// to generate a deterministic builder without causing infinite recursion.
    /// </summary>
    [Serializable]
    public class CyclicDependencyException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class.
        /// </summary>
        public CyclicDependencyException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CyclicDependencyException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CyclicDependencyException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected CyclicDependencyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}