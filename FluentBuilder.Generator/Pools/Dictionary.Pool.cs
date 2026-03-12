// <copyright file="Dictionary.Pool.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Pools
{
    /// <summary>
    /// Provides a pool of reusable <see cref="Dictionary{TKey, TValue}"/> instances to reduce allocations.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    internal static class DictionaryPool<TKey, TValue> where TKey : notnull
    {
        private static readonly ObjectPool<Dictionary<TKey, TValue>> _pool =
            new(() => new Dictionary<TKey, TValue>(), dict => dict.Clear(), maxSize: Constant.StringBuilderPool.MaxPoolSize);

        /// <summary>
        /// Retrieves a <see cref="Dictionary{TKey, TValue}"/> instance from the pool.
        /// </summary>
        /// <returns>A cleared <see cref="Dictionary{TKey, TValue}"/> ready for use.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKey, TValue> Get() => _pool.Get();

        /// <summary>
        /// Returns a <see cref="Dictionary{TKey, TValue}"/> to the pool for reuse.
        /// </summary>
        /// <param name="dict">The dictionary to return. It will be cleared before being made available again.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(Dictionary<TKey, TValue> dict) => _pool.Return(dict);
    }

    /// <summary>
    /// Provides a disposable wrapper for a pooled <see cref="Dictionary{TKey, TValue}"/>.
    /// Use with <c>using</c> to ensure the dictionary is returned to the pool.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public ref struct PooledDictionary<TKey, TValue> where TKey : notnull
    {
        private Dictionary<TKey, TValue>? _dict;

        /// <summary>
        /// Gets the underlying <see cref="Dictionary{TKey, TValue}"/>.
        /// If not yet obtained, it is retrieved from the pool.
        /// </summary>
        public Dictionary<TKey, TValue> Dict => _dict ??= DictionaryPool<TKey, TValue>.Get();

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledDictionary{TKey, TValue}"/> struct.
        /// </summary>
        /// <param name="initialize">
        /// If <c>true</c>, the underlying dictionary is obtained immediately;
        /// otherwise it is obtained on first access.
        /// </param>
        public PooledDictionary(bool initialize = false) : this()
        {
            if (initialize) _ = Dict;
        }

        /// <summary>
        /// Returns the underlying dictionary to the pool, making it available for reuse.
        /// </summary>
        public void Dispose()
        {
            if (_dict != null)
            {
                DictionaryPool<TKey, TValue>.Return(_dict);
                _dict = null;
            }
        }
    }
}