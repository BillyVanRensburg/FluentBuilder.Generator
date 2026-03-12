// <copyright file="TypeSymbol.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// Cache for frequently accessed type symbol properties.
    /// </summary>
    internal static class TypeSymbolCache
    {
        private class TypeInfo
        {
            public string? DisplayName { get; set; }
            public bool? IsCollection { get; set; }
            public ConcurrentDictionary<string, bool> MethodCache { get; } = new();
        }

        private static readonly WeakCache<ITypeSymbol, TypeInfo> _cache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDisplayName(ITypeSymbol type)
        {
            var info = _cache.GetOrCreateValue(type);
            return info.DisplayName ??= type.ToDisplayString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollectionType(ITypeSymbol type)
        {
            var info = _cache.GetOrCreateValue(type);
            if (info.IsCollection.HasValue)
                return info.IsCollection.Value;

            var typeName = GetDisplayName(type);
            var result = typeName.Contains("List<") ||
                        typeName.Contains("Dictionary<") ||
                        typeName.Contains("HashSet<") ||
                        typeName.Contains("Collection<") ||
                        typeName.Contains("IEnumerable<");

            info.IsCollection = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasMethod(ITypeSymbol type, string methodName)
        {
            var info = _cache.GetOrCreateValue(type);
            return info.MethodCache.GetOrAdd(methodName, name =>
            {
                var members = type.GetMembers(name);
                for (var i = 0; i < members.Length; i++)
                {
                    if (members[i] is IMethodSymbol)
                        return true;
                }

                if (type is INamedTypeSymbol namedType)
                {
                    var interfaces = namedType.AllInterfaces;
                    for (var i = 0; i < interfaces.Length; i++)
                    {
                        var ifaceMembers = interfaces[i].GetMembers(name);
                        for (var j = 0; j < ifaceMembers.Length; j++)
                        {
                            if (ifaceMembers[j] is IMethodSymbol)
                                return true;
                        }
                    }
                }
                return false;
            });
        }

        /// <summary>
        /// Clears the cache (used at start of generation) by creating a new table.
        /// </summary>
        public static void Clear() => _cache.Clear();
    }
}