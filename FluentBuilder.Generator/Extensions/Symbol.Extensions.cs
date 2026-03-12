// <copyright file="Symbol.Extensions.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Extensions
{
    /// <summary>
    /// Extension methods for Roslyn symbols to simplify common operations.
    /// </summary>
    internal static class SymbolExtensions
    {
        /// <summary>
        /// Determines whether the symbol has the specified attribute by type.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <param name="attributeType">The attribute type to look for.</param>
        /// <returns>True if the symbol has the attribute; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol? attributeType)
        {
            if (attributeType is null) return false;

            var attributes = symbol.GetAttributes();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(attributes[i].AttributeClass, attributeType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the symbol has the specified attribute by name.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <param name="attributeName">The name of the attribute to look for.</param>
        /// <returns>True if the symbol has the attribute; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute(this ISymbol symbol, string attributeName)
        {
            return AttributeCache.HasAttribute(symbol, attributeName);
        }

        /// <summary>
        /// Tries to get the first attribute of the specified type.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <param name="attributeType">The attribute type to look for.</param>
        /// <param name="attributeData">The attribute data if found.</param>
        /// <returns>True if the attribute was found; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetAttribute(this ISymbol symbol, INamedTypeSymbol? attributeType, out AttributeData? attributeData)
        {
            attributeData = null;

            if (attributeType is null) return false;

            var attributes = symbol.GetAttributes();
            for (var i = 0; i < attributes.Length; i++)
            {
                var attr = attributes[i];
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType))
                {
                    attributeData = attr;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to get the first attribute of the specified name.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <param name="attributeName">The name of the attribute to look for.</param>
        /// <param name="attributeData">The attribute data if found.</param>
        /// <returns>True if the attribute was found; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetAttribute(this ISymbol symbol, string attributeName, out AttributeData? attributeData)
        {
            attributeData = AttributeCache.GetAttribute(symbol, attributeName);
            return attributeData != null;
        }

        /// <summary>
        /// Gets all attributes of the symbol as an array for efficient iteration.
        /// </summary>
        /// <param name="symbol">The symbol to get attributes from.</param>
        /// <returns>An array of attribute data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeData[] GetAttributeArray(this ISymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            var result = new AttributeData[attributes.Length];
            for (var i = 0; i < attributes.Length; i++)
            {
                result[i] = attributes[i];
            }
            return result;
        }

        /// <summary>
        /// Gets a string value from an attribute's constructor or named argument.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetAttributeStringValue(this ISymbol symbol, string attributeName, string? defaultValue = null, int constructorArgIndex = 0, string? namedArgKey = null)
        {
            var attr = AttributeCache.GetAttribute(symbol, attributeName);
            if (attr == null)
                return defaultValue;

            if (attr.ConstructorArguments.Length > constructorArgIndex)
            {
                var value = attr.ConstructorArguments[constructorArgIndex].Value as string;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            if (!string.IsNullOrEmpty(namedArgKey))
            {
                var value = AttributeCache.GetNamedArgument<string>(attr, namedArgKey!);
                if (value != null)
                    return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a boolean value from an attribute's named argument.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetAttributeBoolValue(this ISymbol symbol, string attributeName, string namedArgKey, bool defaultValue = false)
        {
            var attr = AttributeCache.GetAttribute(symbol, attributeName);
            if (attr == null)
                return defaultValue;

            var value = AttributeCache.GetNamedArgument<bool?>(attr, namedArgKey);
            return value ?? defaultValue;
        }

        /// <summary>
        /// Gets an attribute by name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeData? GetAttribute(this ISymbol symbol, string attributeName)
            => AttributeCache.GetAttribute(symbol, attributeName);

        /// <summary>
        /// Gets a string value from a named argument of an attribute.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetAttributeNamedString(this ISymbol symbol, string attributeName, string argumentName, string? defaultValue = null)
        {
            var attr = AttributeCache.GetAttribute(symbol, attributeName);
            if (attr == null) return defaultValue;
            var value = AttributeCache.GetNamedArgument<string>(attr, argumentName);
            return value ?? defaultValue;
        }

        /// <summary>
        /// Gets the value of a constructor argument at the specified index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetConstructorArgumentValue(this AttributeData attribute, int index)
        {
            if (attribute == null) return null;
            return attribute.ConstructorArguments.Length > index
                ? attribute.ConstructorArguments[index].Value
                : null;
        }

        /// <summary>
        /// Gets the values of a constructor argument that is an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] GetConstructorArgumentArray(this AttributeData attribute, int index)
        {
            if (attribute == null) return Array.Empty<object>();

            if (attribute.ConstructorArguments.Length > index)
            {
                var typedConstant = attribute.ConstructorArguments[index];
                if (typedConstant.Values != null && typedConstant.Values.Length > 0)
                {
                    var result = new object[typedConstant.Values.Length];
                    for (var i = 0; i < typedConstant.Values.Length; i++)
                    {
                        result[i] = typedConstant.Values[i].Value!;
                    }
                    return result;
                }
            }
            return Array.Empty<object>();
        }

        /// <summary>
        /// Gets a named argument value of a specified type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetNamedArgumentValue<T>(this AttributeData attribute, string argumentName)
            => AttributeCache.GetNamedArgument<T>(attribute, argumentName)!;

        /// <summary>
        /// Gets collection options for a member.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CollectionOptions GetCollectionOptions(this ISymbol member)
            => AttributeCache.GetCollectionOptions(member);
    }
}