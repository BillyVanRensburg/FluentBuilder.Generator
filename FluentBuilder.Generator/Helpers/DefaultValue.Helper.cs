// <copyright file="DefaultValue.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Helpers
{
    /// <summary>
    /// Provides helper methods for processing default values from attributes.
    /// </summary>
    internal static class DefaultValueHelper
    {
        /// <summary>
        /// Processes a default value attribute and returns a cached result.
        /// </summary>
        /// <param name="defaultValueAttr">The attribute data containing the default value.</param>
        /// <param name="member">The member symbol (property/field) being processed.</param>
        /// <param name="memberType">The type of the member.</param>
        /// <returns>A <see cref="DefaultValueCache.DefaultValueResult"/> containing the formatted value and its kind.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DefaultValueCache.DefaultValueResult ProcessDefaultValue(
            AttributeData defaultValueAttr,
            ISymbol member,
            ITypeSymbol memberType)
            => DefaultValueCache.GetDefaultValue(member, defaultValueAttr, memberType);

        /// <summary>
        /// Formats a value for embedding in source code using the string cache.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value suitable for source code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormatDefaultValueForCode(object value)
            => StringCache.FormatValue(value);
    }
}