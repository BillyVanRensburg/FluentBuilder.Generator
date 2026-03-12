// <copyright file="Method.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Constants;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Helpers
{
    /// <summary>
    /// Provides helper methods for method-related operations.
    /// </summary>
    internal static class MethodHelper
    {
        /// <summary>
        /// Determines whether the specified class has the <see cref="Constant.AttributeName.FluentImplicit"/> attribute.
        /// </summary>
        /// <param name="classSymbol">The class symbol to inspect.</param>
        /// <returns><c>true</c> if the attribute exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFluentImplicitAttribute(INamedTypeSymbol classSymbol)
            => AttributeCache.HasAttribute(classSymbol, Constant.AttributeName.FluentImplicit);

        /// <summary>
        /// Gets the async method name for a given method symbol (e.g., adds "Async" suffix if missing).
        /// </summary>
        /// <param name="method">The method symbol.</param>
        /// <returns>The method name with "Async" suffix if the method is async; otherwise the original name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAsyncMethodName(IMethodSymbol method)
            => MethodCache.GetAsyncMethodName(method);
    }
}