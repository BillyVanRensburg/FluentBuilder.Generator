// <copyright file="DefaultValue.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// Cache for default value processing.
    /// </summary>
    internal static class DefaultValueCache
    {
        // Use a tuple of (ISymbol, int) as key; int is attribute hash code to differentiate attributes.
        private static readonly StrongCache<(ISymbol, int), DefaultValueResult> _defaultValueCache = new();

        internal class DefaultValueResult
        {
            public string FormattedValue { get; set; } = string.Empty;
            public bool IsValid { get; set; } = true;
            public Diagnostic? Error { get; set; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DefaultValueResult GetDefaultValue(ISymbol member, AttributeData defaultValueAttr, ITypeSymbol memberType)
        {
            var key = (member, defaultValueAttr.GetHashCode());
            return _defaultValueCache.GetOrAdd(key, _ => ProcessDefaultValue(defaultValueAttr, member, memberType));
        }

        private static DefaultValueResult ProcessDefaultValue(AttributeData defaultValueAttr, ISymbol member, ITypeSymbol memberType)
        {
            var result = new DefaultValueResult();

            if (defaultValueAttr.ConstructorArguments.Length == 0)
                return result;

            var typedConstant = defaultValueAttr.ConstructorArguments[0];

            // Handle enum types
            if (memberType.TypeKind == TypeKind.Enum)
            {
                return ProcessEnumDefaultValue(typedConstant, memberType, member);
            }

            // Handle primitive types
            var value = typedConstant.Value;
            if (value == null)
            {
                result.FormattedValue = "null";
                return result;
            }

            if (!IsValueCompatibleWithType(value, memberType))
            {
                result.IsValid = false;
                // Error would be set by caller
                return result;
            }

            result.FormattedValue = StringCache.FormatValue(value);
            return result;
        }

        private static DefaultValueResult ProcessEnumDefaultValue(TypedConstant typedConstant, ITypeSymbol memberType, ISymbol member)
        {
            var result = new DefaultValueResult();
            string? enumValue = null;
            var enumTypeName = memberType.ToDisplayString();

            // Handle enum constant
            if (typedConstant.Kind == TypedConstantKind.Enum)
            {
                enumValue = ExtractEnumValueFromConstant(typedConstant, memberType);
            }
            // Handle string representation
            else if (typedConstant.Kind == TypedConstantKind.Primitive &&
                     typedConstant.Value is string enumString)
            {
                enumValue = enumString;
            }
            // Handle integer value
            else if (typedConstant.Value is int intVal)
            {
                enumValue = ExtractEnumValueFromInt(intVal, memberType);
            }

            if (!string.IsNullOrEmpty(enumValue))
            {
                result.FormattedValue = $"{enumTypeName}.{enumValue}";
            }
            else
            {
                result.IsValid = false;
            }

            return result;
        }

        private static string? ExtractEnumValueFromConstant(TypedConstant typedConstant, ITypeSymbol enumType)
        {
            var enumIntValue = typedConstant.Value?.ToString();
            if (enumIntValue != null && int.TryParse(enumIntValue, out int intValue))
            {
                return GetEnumFieldName(enumType, intValue);
            }
            return null;
        }

        private static string? ExtractEnumValueFromInt(int intValue, ITypeSymbol enumType)
        {
            return GetEnumFieldName(enumType, intValue);
        }

        private static string? GetEnumFieldName(ITypeSymbol enumType, int intValue)
        {
            foreach (var member in enumType.GetMembers())
            {
                if (member is IFieldSymbol field &&
                    field.HasConstantValue &&
                    field.ConstantValue != null &&
                    Convert.ToInt32(field.ConstantValue) == intValue)
                {
                    return field.Name;
                }
            }
            return null;
        }

        private static bool IsValueCompatibleWithType(object value, ITypeSymbol targetType)
        {
            if (value == null)
            {
                return targetType.IsValueType
                    ? targetType.OriginalDefinition?.SpecialType == SpecialType.System_Nullable_T
                    : true;
            }

            if (targetType.TypeKind == TypeKind.Enum)
            {
                return IsValueCompatibleWithEnum(value, targetType);
            }

            return value.GetType().Name switch
            {
                "Int32" => IsNumericTypeCompatible(targetType),
                "String" => targetType.SpecialType == SpecialType.System_String,
                "Boolean" => targetType.SpecialType == SpecialType.System_Boolean,
                "Double" => targetType.SpecialType == SpecialType.System_Double || targetType.SpecialType == SpecialType.System_Decimal,
                "Decimal" => targetType.SpecialType == SpecialType.System_Decimal,
                _ => true
            };
        }

        private static bool IsNumericTypeCompatible(ITypeSymbol targetType)
        {
            return targetType.SpecialType == SpecialType.System_Int32 ||
                   targetType.SpecialType == SpecialType.System_Int64 ||
                   targetType.SpecialType == SpecialType.System_Double ||
                   targetType.SpecialType == SpecialType.System_Decimal;
        }

        private static bool IsValueCompatibleWithEnum(object value, ITypeSymbol enumType)
        {
            if (value is string enumString)
            {
                foreach (var member in enumType.GetMembers())
                {
                    if (member.Kind == SymbolKind.Field && member.Name == enumString)
                        return true;
                }
                return false;
            }

            if (value is int || value is short || value is long || value is byte)
            {
                int numericValue = Convert.ToInt32(value);
                foreach (var member in enumType.GetMembers())
                {
                    if (member is IFieldSymbol field &&
                        field.HasConstantValue &&
                        field.ConstantValue != null &&
                        Convert.ToInt32(field.ConstantValue) == numericValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void Clear() => _defaultValueCache.Clear();
    }
}