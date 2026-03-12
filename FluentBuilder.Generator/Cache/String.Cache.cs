// <copyright file="String.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Implementations;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// Cache for string operations to avoid repeated formatting.
    /// </summary>
    internal static class StringCache
    {
        private static readonly StrongCache<string, string> _typeNameCache = new();
        private static readonly StrongCache<object, string> _valueFormatCache = new();
        private static readonly StrongCache<(ITypeSymbol, string), bool> _methodNameCache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeDisplayName(ITypeSymbol type)
        {
            if (type == null) return "object";
            var key = type.ToDisplayString();
            return _typeNameCache.GetOrAdd(key, _ => key);
        }

        /// <summary>
        /// Gets the type name with nullable annotation if applicable (e.g., "string?").
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetNullableTypeDisplayName(ITypeSymbol type)
        {
            if (type == null) return "object";

            var baseName = GetTypeDisplayName(type);
            if (type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return baseName + "?";
            }
            return baseName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSimpleTypeName(INamedTypeSymbol type)
        {
            if (!type.IsGenericType)
                return type.Name;

            return type.Name.Split('`')[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatValue(object? value)
        {
            if (value == null) return "null";

            if (_valueFormatCache.TryGetValue(value, out var cached))
                return cached!;

            string result = FormatValueCore(value);

            // Cache only small value types or short strings to avoid holding large objects
            if (value.GetType().IsValueType || (value is string str && str.Length <= 50))
                _valueFormatCache.GetOrAdd(value, _ => result);

            return result;
        }

        private static string FormatValueCore(object value)
        {
            return value switch
            {
                string str => $"\"{str.Replace("\"", "\\\"")}\"",
                bool b => b.ToString().ToLower(),
                char c => $"'{c}'",
                int i => i.ToString(),
                long l => $"{l}L",
                decimal d => $"{d.ToString(System.Globalization.CultureInfo.InvariantCulture)}m",
                double db => FormatDouble(db),
                float f => FormatFloat(f),
                byte b8 => b8.ToString(),
                sbyte sb8 => sb8.ToString(),
                short s16 => s16.ToString(),
                ushort us16 => us16.ToString(),
                uint ui32 => $"{ui32}u",
                ulong ul64 => $"{ul64}ul",
                _ => FormatComplexValue(value)
            };
        }

        private static string FormatDouble(double value)
        {
            if (double.IsNaN(value)) return "double.NaN";
            if (double.IsPositiveInfinity(value)) return "double.PositiveInfinity";
            if (double.IsNegativeInfinity(value)) return "double.NegativeInfinity";
            return $"{value.ToString(System.Globalization.CultureInfo.InvariantCulture)}d";
        }

        private static string FormatFloat(float value)
        {
            if (float.IsNaN(value)) return "float.NaN";
            if (float.IsPositiveInfinity(value)) return "float.PositiveInfinity";
            if (float.IsNegativeInfinity(value)) return "float.NegativeInfinity";
            return $"{value.ToString(System.Globalization.CultureInfo.InvariantCulture)}f";
        }

        private static string FormatComplexValue(object value)
        {
            var valueType = value.GetType();
            if (valueType.IsEnum)
            {
                var enumTypeName = valueType.FullName ?? valueType.Name;
                var enumValueName = Enum.GetName(valueType, value);
                return !string.IsNullOrEmpty(enumValueName)
                    ? $"{enumTypeName}.{enumValueName}"
                    : Convert.ToInt32(value).ToString();
            }

            return $"\"{value.ToString()?.Replace("\"", "\\\"")}\"";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasMethodName(ITypeSymbol type, string methodName)
        {
            var key = (type, methodName);
            return _methodNameCache.GetOrAdd(key, static k =>
            {
                var members = k.Item1.GetMembers(k.Item2);
                for (var i = 0; i < members.Length; i++)
                {
                    if (members[i] is IMethodSymbol)
                        return true;
                }
                return false;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetMethodSignature(IMethodSymbol method, string fluentName)
        {
            var sb = new StringBuilder();
            sb.Append(fluentName);
            sb.Append('(');
            bool first = true;
            foreach (var param in method.Parameters)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append(param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static void Clear()
        {
            _typeNameCache.Clear();
            _valueFormatCache.Clear();
            _methodNameCache.Clear();
        }
    }
}