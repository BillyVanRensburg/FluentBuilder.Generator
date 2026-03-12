// <copyright file="WeakCache.Implementation.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// A cache that holds weak references to its keys, allowing them to be garbage collected.
    /// Uses <see cref="ConditionalWeakTable{TKey, TEntry}"/> internally.
    /// The entry type must be a reference type with a parameterless constructor.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key. Must be a reference type.</typeparam>
    /// <typeparam name="TEntry">The type of the cached entry. Must be a reference type with a parameterless constructor.</typeparam>
    internal sealed class WeakCache<TKey, TEntry>
        where TKey : class
        where TEntry : class, new()
    {
        private ConditionalWeakTable<TKey, TEntry> _cache = new();

        /// <summary>
        /// Gets the entry associated with the specified key, or creates a new one
        /// using the default constructor if it does not already exist.
        /// </summary>
        /// <param name="key">The key of the entry to get or create.</param>
        /// <returns>The entry associated with the key.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntry GetOrCreateValue(TKey key) => _cache.GetOrCreateValue(key);

        /// <summary>
        /// Attempts to get the entry associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the entry to get.</param>
        /// <param name="entry">When this method returns, contains the entry associated with
        /// the specified key, if found; otherwise, the default value.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TEntry? entry) =>
            _cache.TryGetValue(key, out entry);

        /// <summary>
        /// Clears all entries from the cache by creating a new underlying table.
        /// </summary>
        public void Clear() => _cache = new ConditionalWeakTable<TKey, TEntry>();
    }
}