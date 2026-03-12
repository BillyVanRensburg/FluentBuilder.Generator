// <copyright file="AttributeValue.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Provides helper methods for retrieving attribute values from symbols.
    /// </summary>
    public static class AttributeValueHelper
    {
        /// <summary>
        /// Gets a string value from an attribute on the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to inspect.</param>
        /// <param name="attributeName">The full name of the attribute.</param>
        /// <param name="defaultValue">The default value to return if the attribute or value is missing.</param>
        /// <param name="constructorArgIndex">The index of the constructor argument to read.</param>
        /// <param name="namedArgKey">Optional named argument key to try if the constructor argument is missing or empty.</param>
        /// <returns>The attribute string value, or <paramref name="defaultValue"/> if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAttributeStringValue(
            ISymbol symbol,
            string attributeName,
            string defaultValue,
            int constructorArgIndex = 0,
            string? namedArgKey = null)
        {
            var attr = AttributeCache.GetAttribute(symbol, attributeName);
            if (attr == null)
                return defaultValue;

            // Try constructor argument
            if (attr.ConstructorArguments.Length > constructorArgIndex)
            {
                var value = attr.ConstructorArguments[constructorArgIndex].Value as string;
                if (!string.IsNullOrEmpty(value))
                    return value!;
            }

            // Try named argument if provided
            if (!string.IsNullOrEmpty(namedArgKey))
            {
                var value = AttributeCache.GetNamedArgument<string>(attr, namedArgKey!);
                if (value != null)
                    return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a boolean value from a named argument of an attribute on the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to inspect.</param>
        /// <param name="attributeName">The full name of the attribute.</param>
        /// <param name="namedArgKey">The name of the named argument.</param>
        /// <param name="defaultValue">The default value to return if the attribute or argument is missing.</param>
        /// <returns>The attribute boolean value, or <paramref name="defaultValue"/> if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetAttributeBoolValue(
            ISymbol symbol,
            string attributeName,
            string namedArgKey,
            bool defaultValue = true)
        {
            var attr = AttributeCache.GetAttribute(symbol, attributeName);
            if (attr == null)
                return defaultValue;

            var value = AttributeCache.GetNamedArgument<bool?>(attr, namedArgKey);
            return value ?? defaultValue;
        }

        /// <summary>
        /// Gets a nullable struct value from a named argument of an attribute.
        /// </summary>
        /// <typeparam name="T">The type of the struct to retrieve.</typeparam>
        /// <param name="attr">The attribute data.</param>
        /// <param name="namedArgKey">The name of the named argument.</param>
        /// <param name="defaultValue">The default value to return if the argument is missing.</param>
        /// <returns>The attribute value, or <paramref name="defaultValue"/> if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? GetAttributeValue<T>(
            AttributeData attr,
            string namedArgKey,
            T? defaultValue = default) where T : struct
        {
            if (attr == null)
                return defaultValue;

            var value = AttributeCache.GetNamedArgument<T?>(attr, namedArgKey);
            return value ?? defaultValue;
        }

        /// <summary>
        /// Determines whether the specified symbol has an attribute with the given name.
        /// </summary>
        /// <param name="symbol">The symbol to inspect.</param>
        /// <param name="attributeName">The full name of the attribute.</param>
        /// <returns><c>true</c> if the attribute exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute(ISymbol symbol, string attributeName)
            => AttributeCache.HasAttribute(symbol, attributeName);
    }
}