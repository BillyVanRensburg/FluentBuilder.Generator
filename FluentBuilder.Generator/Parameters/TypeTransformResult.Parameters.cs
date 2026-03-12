// <copyright file="TypeTransformResult.Parameters.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Parameters
{
    /// <summary>
    /// Represents the result of syntax transformation with proper null safety.
    /// </summary>
    internal readonly struct TypeTransformResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeTransformResult"/> struct.
        /// </summary>
        /// <param name="typeSymbol">The named type symbol of the target.</param>
        /// <param name="compilation">The current compilation.</param>
        /// <param name="hasFluentBuilderAttribute">
        /// Indicates whether the target type has the <see cref="FluentBuilderAttribute"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TypeTransformResult(INamedTypeSymbol typeSymbol, Compilation compilation, bool hasFluentBuilderAttribute)
        {
            TypeSymbol = typeSymbol;
            Compilation = compilation;
            HasFluentBuilderAttribute = hasFluentBuilderAttribute;
        }

        /// <summary>
        /// Gets the named type symbol of the target.
        /// </summary>
        internal INamedTypeSymbol TypeSymbol { get; }

        /// <summary>
        /// Gets the current compilation.
        /// </summary>
        internal Compilation Compilation { get; }

        /// <summary>
        /// Gets a value indicating whether the target type has the <see cref="FluentBuilderAttribute"/>.
        /// </summary>
        internal bool HasFluentBuilderAttribute { get; }
    }
}