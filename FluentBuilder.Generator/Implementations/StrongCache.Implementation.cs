// <copyright file="StrongCache.Implementation.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Implementations
{
    /// <summary>
    /// A thread-safe cache that holds strong references to its entries.
    /// Uses <see cref="ConcurrentDictionary{TKey, TValue}"/> internally.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key. Must not be null.</typeparam>
    /// <typeparam name="TValue">The type of the cached value.</typeparam>
    internal sealed class StrongCache<TKey, TValue> where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TValue> _cache = new();

        /// <summary>
        /// Gets the value associated with the specified key, or adds a new value
        /// using the provided factory function.
        /// </summary>
        /// <param name="key">The key of the element to get or add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value for the key.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) =>
            _cache.GetOrAdd(key, valueFactory);

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with
        /// the specified key, if found; otherwise, the default value.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue? value) =>
            _cache.TryGetValue(key, out value);

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        public void Clear() => _cache.Clear();
    }
}