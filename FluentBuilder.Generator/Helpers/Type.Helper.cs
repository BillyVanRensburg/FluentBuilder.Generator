// <copyright file="Type.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Helpers
{
    /// <summary>
    /// Provides helper methods for type-related operations in the FluentBuilder generator.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Gets the type of a member (property or field) using the cache.
        /// </summary>
        /// <param name="member">The member symbol.</param>
        /// <returns>The type symbol of the member, or <c>null</c> if not applicable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITypeSymbol? GetMemberType(ISymbol member)
            => MemberCache.GetMemberType(member);

        /// <summary>
        /// Determines whether the specified type is a collection (implements IEnumerable or is an array).
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if the type is a collection; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollectionType(ITypeSymbol? type)
            => type != null && TypeSymbolCache.IsCollectionType(type);

        /// <summary>
        /// Determines whether the specified type is a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if the type is List&lt;T&gt;; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsListType(ITypeSymbol? type)
        {
            if (type == null) return false;

            if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition != null)
            {
                return namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.List<T>";
            }

            var typeName = GetTypeDisplayName(type);
            return typeName.Contains("List<");
        }

        /// <summary>
        /// Determines whether a type should be configured using an Action&lt;T&gt; setter.
        /// Complex reference types (except strings) return true.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if an Action setter should be generated; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldUseAction(ITypeSymbol? type)
        {
            if (type == null) return false;

            // Value types and simple primitives are better set directly
            if (type.IsValueType) return false;
            if (type.SpecialType == SpecialType.System_String) return false;
            if (type.SpecialType >= SpecialType.System_Boolean && type.SpecialType <= SpecialType.System_UIntPtr)
                return false;
            if (type.SpecialType == SpecialType.System_Object) return false;

            return true;
        }

        /// <summary>
        /// Checks if the type has an Add method.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if an Add method exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAddMethod(ITypeSymbol? type)
            => HasMethod(type, "Add");

        /// <summary>
        /// Checks if the type has a Remove method.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if a Remove method exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasRemoveMethod(ITypeSymbol? type)
            => HasMethod(type, "Remove");

        /// <summary>
        /// Checks if the type has a Clear method.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if a Clear method exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasClearMethod(ITypeSymbol? type)
            => HasMethod(type, "Clear");

        /// <summary>
        /// Checks if the type has a method with the specified name.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <param name="methodName">The method name to look for.</param>
        /// <returns><c>true</c> if the method exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasMethod(ITypeSymbol? type, string methodName)
            => type != null && StringCache.HasMethodName(type, methodName);

        /// <summary>
        /// Determines whether the type is a numeric primitive (int, long, double, decimal, float).
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns><c>true</c> if numeric; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumericType(ITypeSymbol type)
        {
            var specialType = type.SpecialType;
            return specialType == SpecialType.System_Int32 ||
                   specialType == SpecialType.System_Int64 ||
                   specialType == SpecialType.System_Double ||
                   specialType == SpecialType.System_Decimal ||
                   specialType == SpecialType.System_Single;
        }

        /// <summary>
        /// Determines whether a builder exists for the specified type.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <param name="compilation">The current compilation.</param>
        /// <returns><c>true</c> if a builder is found; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasBuilder(ITypeSymbol type, Compilation compilation)
        {
            if (type is INamedTypeSymbol nts)
            {
                var attributes = nts.GetAttributes();
                foreach (var attr in attributes)
                {
                    if (attr.AttributeClass?.Name == Constant.AttributeName.FluentBuilder)
                        return true;
                }

                var builderTypeName = $"{nts.Name}{Constant.DefaultNames.BuilderSuffix}";
                var fullName = $"{nts.ContainingNamespace?.ToDisplayString()}.{builderTypeName}";
                return compilation.GetTypeByMetadataName(fullName) != null;
            }
            return false;
        }

        /// <summary>
        /// Gets a list of buildable members (properties and fields) from a type.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns>A list of member symbols that should be included in the builder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ISymbol> GetBuildableMembers(INamedTypeSymbol typeSymbol)
            => MemberCache.GetBuildableMembers(typeSymbol);

        /// <summary>
        /// Gets a list of includable methods from a type.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns>A list of method symbols that should be included in the builder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<IMethodSymbol> GetIncludableMethods(INamedTypeSymbol typeSymbol)
            => MemberCache.GetIncludableMethods(typeSymbol);

        /// <summary>
        /// Gets a list of required members (properties/fields) from a type.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns>A list of member symbols that are required (e.g., via constructor parameters).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ISymbol> GetRequiredMembers(INamedTypeSymbol typeSymbol)
            => MemberCache.GetRequiredMembers(typeSymbol);

        /// <summary>
        /// Determines whether a member should be included in the builder based on accessibility and attributes.
        /// </summary>
        /// <param name="member">The member symbol.</param>
        /// <returns><c>true</c> if the member should be included; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldIncludeMember(ISymbol member)
        {
            if (member.IsStatic)
                return false;

            var accessibility = member.DeclaredAccessibility;

            if (accessibility == Accessibility.Public)
                return !AttributeCache.HasAttribute(member, Constant.AttributeName.FluentIgnore);

            if (accessibility == Accessibility.Internal)
                return AttributeCache.HasAttribute(member, Constant.AttributeName.FluentInclude);

            return false;
        }

        /// <summary>
        /// Determines whether a method should be included in the builder.
        /// </summary>
        /// <param name="method">The method symbol.</param>
        /// <returns><c>true</c> if the method should be included; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldIncludeMethod(IMethodSymbol method)
        {
            if (method.MethodKind != MethodKind.Ordinary || method.IsStatic)
                return false;

            var accessibility = method.DeclaredAccessibility;

            if (accessibility == Accessibility.Public)
                return !AttributeCache.HasAttribute(method, Constant.AttributeName.FluentIgnore);

            if (accessibility == Accessibility.Internal)
                return AttributeCache.HasAttribute(method, Constant.AttributeName.FluentInclude);

            return false;
        }

        /// <summary>
        /// Determines whether a constructor should be included in the builder.
        /// </summary>
        /// <param name="constructor">The method symbol representing a constructor.</param>
        /// <returns><c>true</c> if the constructor should be included; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldIncludeConstructor(IMethodSymbol constructor)
        {
            if (constructor.MethodKind != MethodKind.Constructor)
                return false;

            var accessibility = constructor.DeclaredAccessibility;

            if (accessibility == Accessibility.Public)
                return !AttributeCache.HasAttribute(constructor, Constant.AttributeName.FluentIgnore);

            if (accessibility == Accessibility.Internal)
                return AttributeCache.HasAttribute(constructor, Constant.AttributeName.FluentInclude);

            return false;
        }

        /// <summary>
        /// Checks if a string is a valid C# identifier.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns><c>true</c> if valid; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIdentifier(string name)
            => IdentifierCache.IsValidIdentifier(name);

        /// <summary>
        /// Determines whether a property has a public setter.
        /// For fields, always returns true (fields can always be set).
        /// </summary>
        /// <param name="member">The member symbol.</param>
        /// <returns><c>true</c> if the member is settable; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPublicSetter(ISymbol member)
        {
            if (member is IPropertySymbol property)
            {
                return property.SetMethod?.DeclaredAccessibility == Accessibility.Public;
            }
            return true; // Fields are always settable
        }

        /// <summary>
        /// Determines whether a property has an init-only setter.
        /// </summary>
        /// <param name="member">The member symbol.</param>
        /// <returns><c>true</c> if the property has an init accessor; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInitOnly(ISymbol member)
        {
            if (member is IPropertySymbol property)
            {
                return property.SetMethod?.IsInitOnly == true;
            }
            return false;
        }

        /// <summary>
        /// Gets the default value expression for a given type (e.g., "null", "0", "default(T)").
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns>A string representing the default value for the type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDefaultValueForType(ITypeSymbol type)
        {
            if (type.IsReferenceType || type.NullableAnnotation == NullableAnnotation.Annotated)
                return "null";

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean: return "false";
                case SpecialType.System_Char: return "'\\0'";
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    return "0";
                case SpecialType.System_Single: return "0f";
                case SpecialType.System_Double: return "0d";
                case SpecialType.System_Decimal: return "0m";
                default:
                    return $"default({GetTypeDisplayName(type)})";
            }
        }

        /// <summary>
        /// Determines whether a method is async (returns Task or Task&lt;T&gt;).
        /// </summary>
        /// <param name="method">The method symbol.</param>
        /// <returns><c>true</c> if async; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsyncMethod(IMethodSymbol method)
            => MethodCache.IsAsyncMethod(method);

        /// <summary>
        /// Gets the display name of a type suitable for code generation.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns>A string representation of the type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeDisplayName(ITypeSymbol type)
            => StringCache.GetTypeDisplayName(type);

        /// <summary>
        /// Attempts to get the element type of a collection (e.g., T from List&lt;T&gt; or array).
        /// </summary>
        /// <param name="type">The collection type.</param>
        /// <param name="elementType">When this method returns, contains the element type if found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if an element type was found; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetElementType(ITypeSymbol type, out ITypeSymbol? elementType)
        {
            elementType = null;
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                if (namedType.TypeArguments.Length == 1)
                {
                    elementType = namedType.TypeArguments[0];
                    return true;
                }
            }
            else if (type is IArrayTypeSymbol arrayType)
            {
                elementType = arrayType.ElementType;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether a type has any init-only properties.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns><c>true</c> if at least one init-only property exists; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasInitOnlyProperties(INamedTypeSymbol typeSymbol)
        {
            foreach (var member in GetBuildableMembers(typeSymbol))
            {
                if (member is IPropertySymbol property && property.SetMethod?.IsInitOnly == true)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets custom validation methods (decorated with FluentValidationMethod) from a type.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns>A list of method symbols that are custom validation methods.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<IMethodSymbol> GetCustomValidationMethods(INamedTypeSymbol typeSymbol)
        {
            var result = new List<IMethodSymbol>();
            var members = typeSymbol.GetMembers();
            foreach (var member in members)
            {
                if (member is IMethodSymbol method &&
                    method.MethodKind == MethodKind.Ordinary &&
                    !method.IsStatic &&
                    method.DeclaredAccessibility == Accessibility.Public &&
                    AttributeCache.HasAttribute(method, Constant.AttributeName.FluentValidationMethod))
                {
                    result.Add(method);
                }
            }
            return result;
        }

        /// <summary>
        /// Determines if a type is file-scoped (accessible only within the same file).
        /// Uses reflection to check for the 'IsFileLocal' property (Roslyn 4.0+).
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns><c>true</c> if the type is file-scoped; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFileScoped(INamedTypeSymbol typeSymbol)
        {
            // Use reflection to check for IsFileLocal property (Roslyn 4.0+)
            var prop = typeSymbol.GetType().GetProperty("IsFileLocal");
            if (prop != null && prop.GetValue(typeSymbol) is bool isFile && isFile)
                return true;

            return false;
        }
    }
}