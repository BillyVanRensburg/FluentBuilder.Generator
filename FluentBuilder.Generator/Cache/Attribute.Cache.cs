// <copyright file="Attribute.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Implementations;
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// High-performance cache for attribute operations.
    /// </summary>
    internal static class AttributeCache
    {
        private static readonly StrongCache<CacheKey, object?> _cache = new();
        private static readonly object True = true;
        private static readonly object False = false;

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly ISymbol _symbol;
            private readonly string _key;
            private readonly int _hashCode;

            public CacheKey(ISymbol symbol, string key)
            {
                _symbol = symbol;
                _key = key;
                unchecked
                {
                    _hashCode = (SymbolEqualityComparer.Default.GetHashCode(symbol) * 397) ^
                                (key.GetHashCode() * 397);
                }
            }

            public bool Equals(CacheKey other) =>
                _key == other._key &&
                SymbolEqualityComparer.Default.Equals(_symbol, other._symbol);

            public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);
            public override int GetHashCode() => _hashCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute(ISymbol symbol, string attributeName)
        {
            var key = new CacheKey(symbol, $"Has:{attributeName}");

            if (_cache.TryGetValue(key, out var cached))
                return cached == True;

            var result = CheckHasAttribute(symbol, attributeName);
            _cache.GetOrAdd(key, _ => result ? True : False);
            return result;
        }

        private static bool CheckHasAttribute(ISymbol symbol, string attributeName)
        {
            foreach (var attr in symbol.GetAttributes())
                if (attr.AttributeClass?.Name == attributeName)
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeData? GetAttribute(ISymbol symbol, string attributeName)
        {
            var key = new CacheKey(symbol, $"Attr:{attributeName}");

            if (_cache.TryGetValue(key, out var cached))
                return cached as AttributeData;

            var attr = FindAttribute(symbol, attributeName);
            _cache.GetOrAdd(key, _ => attr);
            return attr;
        }

        private static AttributeData? FindAttribute(ISymbol symbol, string attributeName)
        {
            foreach (var attr in symbol.GetAttributes())
                if (attr.AttributeClass?.Name == attributeName)
                    return attr;
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeData[] GetCachedAttributes(ISymbol symbol)
        {
            var key = new CacheKey(symbol, "AllAttributes");

            if (_cache.TryGetValue(key, out var cached) && cached is AttributeData[] attrs)
                return attrs;

            var allAttrs = symbol.GetAttributes();
            var result = new AttributeData[allAttrs.Length];
            for (var i = 0; i < allAttrs.Length; i++)
                result[i] = allAttrs[i];

            _cache.GetOrAdd(key, _ => result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasValidateAttribute(ISymbol symbol)
        {
            var key = new CacheKey(symbol, "HasValidate");

            if (_cache.TryGetValue(key, out var cached))
                return cached == True;

            var result = CheckHasValidateAttribute(symbol);
            _cache.GetOrAdd(key, _ => result ? True : False);
            return result;
        }

        private static bool CheckHasValidateAttribute(ISymbol symbol)
        {
            foreach (var attr in symbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.Name;
                if (attrName == null) continue;

                if (attrName == Constant.AttributeName.FluentValidate ||
                    attrName == Constant.AttributeName.FluentValidateEmail ||
                    attrName == Constant.AttributeName.FluentValidatePhone ||
                    attrName == Constant.AttributeName.FluentValidateUrl ||
                    attrName == Constant.AttributeName.FluentValidateEqual ||
                    attrName == Constant.AttributeName.FluentValidateNotEqual ||
                    attrName == Constant.AttributeName.FluentValidateGreaterThan ||
                    attrName == Constant.AttributeName.FluentValidateGreaterThanOrEqual ||
                    attrName == Constant.AttributeName.FluentValidateLessThan ||
                    attrName == Constant.AttributeName.FluentValidateLessThanOrEqual ||
                    attrName == Constant.AttributeName.FluentValidateOneOf ||
                    attrName == Constant.AttributeName.FluentValidateWith)
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CollectionOptions GetCollectionOptions(ISymbol symbol)
        {
            var key = new CacheKey(symbol, "CollectionOptions");

            if (_cache.TryGetValue(key, out var cached) && cached is CollectionOptions options)
                return options;

            options = BuildCollectionOptions(symbol);
            _cache.GetOrAdd(key, _ => options);
            return options;
        }

        private static CollectionOptions BuildCollectionOptions(ISymbol symbol)
        {
            var options = new CollectionOptions();
            var attr = GetAttribute(symbol, Constant.AttributeName.FluentCollectionOptions);

            if (attr == null)
                return options;

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "GenerateDirectSetter" when arg.Value.Value is bool value:
                        options.GenerateDirectSetter = value;
                        break;
                    case "GenerateActionSetter" when arg.Value.Value is bool value:
                        options.GenerateActionSetter = value;
                        break;
                    case "GenerateAdd" when arg.Value.Value is bool value:
                        options.GenerateAdd = value;
                        break;
                    case "GenerateRemove" when arg.Value.Value is bool value:
                        options.GenerateRemove = value;
                        break;
                    case "GenerateClear" when arg.Value.Value is bool value:
                        options.GenerateClear = value;
                        break;
                    case "GenerateAddRange" when arg.Value.Value is bool value:
                        options.GenerateAddRange = value;
                        break;
                    case "GenerateCount" when arg.Value.Value is bool value:
                        options.GenerateCount = value;
                        break;
                    case "MinCount" when arg.Value.Value is int value:
                        options.MinCount = value;
                        break;
                    case "MaxCount" when arg.Value.Value is int value:
                        options.MaxCount = value;
                        break;
                    case "ExactCount" when arg.Value.Value is int value:
                        options.ExactCount = value;
                        break;
                    case "CountValidationMessage" when arg.Value.Value is string value:
                        options.CountValidationMessage = value;
                        break;
                }
            }
            return options;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? GetConstructorArgument<T>(ISymbol symbol, string attributeName, int index = 0)
        {
            var key = new CacheKey(symbol, $"Ctor:{attributeName}:{index}");

            if (_cache.TryGetValue(key, out var cached))
                return (T?)cached;

            var attr = GetAttribute(symbol, attributeName);
            var result = GetConstructorArgumentSlow<T>(attr, index);
            _cache.GetOrAdd(key, _ => result);
            return result;
        }

        private static T? GetConstructorArgumentSlow<T>(AttributeData? attribute, int index)
        {
            if (attribute == null || attribute.ConstructorArguments.Length <= index)
                return default;

            var value = attribute.ConstructorArguments[index].Value;
            if (value is T tValue)
                return tValue;

            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? GetNamedArgument<T>(ISymbol symbol, string attributeName, string argumentName)
        {
            var key = new CacheKey(symbol, $"Named:{attributeName}:{argumentName}");

            if (_cache.TryGetValue(key, out var cached))
                return (T?)cached;

            var attr = GetAttribute(symbol, attributeName);
            var result = GetNamedArgumentSlow<T>(attr, argumentName);
            _cache.GetOrAdd(key, _ => result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? GetNamedArgument<T>(AttributeData? attribute, string argumentName)
        {
            if (attribute == null) return default;
            return GetNamedArgumentSlow<T>(attribute, argumentName);
        }

        private static T? GetNamedArgumentSlow<T>(AttributeData? attribute, string argumentName)
        {
            if (attribute == null) return default;

            foreach (var arg in attribute.NamedArguments)
            {
                if (arg.Key == argumentName)
                {
                    var value = arg.Value.Value;

                    // NEW: Convert enum to its name when T is string
                    if (value != null && value.GetType().IsEnum && typeof(T) == typeof(string))
                    {
                        return (T)(object)value.ToString();
                    }

                    if (value is T tValue)
                        return tValue;

                    try
                    {
                        return (T?)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return default;
                    }
                }
            }
            return default;
        }

        public static void Clear() => _cache.Clear();
    }
}