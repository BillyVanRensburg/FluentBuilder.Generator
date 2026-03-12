// <copyright file="Cache.Manager.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Managers
{
    /// <summary>
    /// Central manager for all caches to ensure consistent clearing between generation runs.
    /// </summary>
    /// <remarks>
    /// This class provides a single point to clear all static caches used throughout the generator.
    /// Clearing caches at the start of each generation prevents cross-compilation contamination
    /// and ensures that each run starts with a clean state.
    /// </remarks>
    internal static class CacheManager
    {
        /// <summary>
        /// Clears all static caches used by the generator.
        /// </summary>
        /// <remarks>
        /// The following caches are cleared:
        /// <list type="bullet">
        ///   <item><see cref="AttributeCache"/> – cached attribute data.</item>
        ///   <item><see cref="TypeSymbolCache"/> – cached type symbols.</item>
        ///   <item><see cref="MemberCache"/> – cached member symbols.</item>
        ///   <item><see cref="MethodCache"/> – cached method symbols.</item>
        ///   <item><see cref="StringCache"/> – cached string representations.</item>
        ///   <item><see cref="IdentifierCache"/> – cached identifier names.</item>
        ///   <item><see cref="DefaultValueCache"/> – cached default values.</item>
        /// </list>
        /// This method is aggressively inlined for potential performance gains.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearAllCaches()
        {
            AttributeCache.Clear();
            TypeSymbolCache.Clear();
            MemberCache.Clear();
            MethodCache.Clear();
            StringCache.Clear();
            IdentifierCache.Clear();
            DefaultValueCache.Clear();
        }

        /// <summary>
        /// Prepares for a new generation pass by clearing all caches.
        /// </summary>
        /// <remarks>
        /// This method should be called at the beginning of each generation run
        /// to ensure that no stale data from previous compilations remains.
        /// It simply delegates to <see cref="ClearAllCaches"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitializeGeneration()
        {
            ClearAllCaches();
        }
    }
}