// <copyright file="Member.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Constants;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// Cache for member-related operations to avoid repeated symbol lookups.
    /// </summary>
    internal static class MemberCache
    {
        private class MemberInfo
        {
            // This dictionary uses a cache key (string) to store the list of buildable members.
            // The key includes the type name and member count to detect changes (if any).
            public ConcurrentDictionary<string, List<ISymbol>> BuildableMembers { get; } = new();
            public List<IMethodSymbol>? IncludableMethods { get; set; }
            public List<ISymbol>? RequiredMembers { get; set; }
            public ConcurrentDictionary<string, ITypeSymbol?> MemberTypeCache { get; } = new();
        }

        private static readonly WeakCache<INamedTypeSymbol, MemberInfo> _cache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ISymbol> GetBuildableMembers(INamedTypeSymbol typeSymbol)
        {
            var info = _cache.GetOrCreateValue(typeSymbol);
            var key = GetCacheKey(typeSymbol);

            return info.BuildableMembers.GetOrAdd(key, _ =>
            {
                var result = new List<ISymbol>();
                var members = typeSymbol.GetMembers();

                for (var i = 0; i < members.Length; i++)
                {
                    var member = members[i];
                    if (member is IFieldSymbol field && ShouldIncludeMember(field))
                    {
                        result.Add(field);
                    }
                    else if (member is IPropertySymbol property && ShouldIncludeMember(property))
                    {
                        result.Add(property);
                    }
                }

                return result;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<IMethodSymbol> GetIncludableMethods(INamedTypeSymbol typeSymbol)
        {
            var info = _cache.GetOrCreateValue(typeSymbol);
            return info.IncludableMethods ??= BuildIncludableMethods(typeSymbol);
        }

        private static List<IMethodSymbol> BuildIncludableMethods(INamedTypeSymbol typeSymbol)
        {
            var result = new List<IMethodSymbol>();
            var members = typeSymbol.GetMembers();

            for (var i = 0; i < members.Length; i++)
            {
                if (members[i] is IMethodSymbol method &&
                    method.MethodKind == MethodKind.Ordinary &&
                    ShouldIncludeMethod(method))
                {
                    result.Add(method);
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ISymbol> GetRequiredMembers(INamedTypeSymbol typeSymbol)
        {
            var info = _cache.GetOrCreateValue(typeSymbol);
            return info.RequiredMembers ??= BuildRequiredMembers(typeSymbol);
        }

        private static List<ISymbol> BuildRequiredMembers(INamedTypeSymbol typeSymbol)
        {
            var result = new List<ISymbol>();
            var members = typeSymbol.GetMembers();

            for (var i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if ((member is IFieldSymbol f && f.IsRequired) ||
                    (member is IPropertySymbol p && p.IsRequired))
                {
                    if (ShouldIncludeMember(member))
                    {
                        result.Add(member);
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITypeSymbol? GetMemberType(ISymbol member)
        {
            if (member.ContainingType == null)
                return null;

            var info = _cache.GetOrCreateValue(member.ContainingType);
            return info.MemberTypeCache.GetOrAdd(member.Name, _ =>
            {
                if (member is IFieldSymbol field)
                    return field.Type;
                if (member is IPropertySymbol property)
                    return property.Type;
                return null;
            });
        }

        private static bool ShouldIncludeMember(ISymbol member)
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

        private static bool ShouldIncludeMethod(IMethodSymbol method)
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

        private static string GetCacheKey(INamedTypeSymbol typeSymbol)
        {
            return $"{typeSymbol.ToDisplayString()}_{typeSymbol.GetMembers().Length}";
        }

        public static void Clear() => _cache.Clear();
    }
}