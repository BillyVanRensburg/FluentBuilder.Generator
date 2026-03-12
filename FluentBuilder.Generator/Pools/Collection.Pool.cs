// <copyright file="Collection.Pool.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Provides a pool of reusable <see cref="HashSet{T}"/> instances to reduce allocations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the hash set.</typeparam>
    internal static class HashSetPool<T>
    {
        private static readonly ObjectPool<HashSet<T>> _pool =
            new(() => new HashSet<T>(), set => set.Clear(), maxSize: Constant.StringBuilderPool.MaxPoolSize);

        /// <summary>
        /// Retrieves a <see cref="HashSet{T}"/> instance from the pool.
        /// </summary>
        /// <returns>A cleared <see cref="HashSet{T}"/> ready for use.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<T> Get() => _pool.Get();

        /// <summary>
        /// Returns a <see cref="HashSet{T}"/> to the pool for reuse.
        /// </summary>
        /// <param name="set">The hash set to return. It will be cleared before being made available again.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(HashSet<T> set) => _pool.Return(set);
    }

    /// <summary>
    /// Provides a pool of reusable <see cref="List{T}"/> instances to reduce allocations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    internal static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> _pool =
            new(() => new List<T>(), list => list.Clear(), maxSize: Constant.StringBuilderPool.MaxPoolSize);

        /// <summary>
        /// Retrieves a <see cref="List{T}"/> instance from the pool.
        /// </summary>
        /// <returns>A cleared <see cref="List{T}"/> ready for use.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Get() => _pool.Get();

        /// <summary>
        /// Returns a <see cref="List{T}"/> to the pool for reuse.
        /// </summary>
        /// <param name="list">The list to return. It will be cleared before being made available again.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(List<T> list) => _pool.Return(list);
    }

    /// <summary>
    /// Provides a disposable wrapper for a pooled <see cref="HashSet{T}"/>.
    /// Use with <c>using</c> to ensure the set is returned to the pool.
    /// </summary>
    /// <typeparam name="T">The type of elements in the hash set.</typeparam>
    public ref struct PooledHashSet<T>
    {
        private HashSet<T>? _set;

        /// <summary>
        /// Gets the underlying <see cref="HashSet{T}"/>. If not yet obtained, it is retrieved from the pool.
        /// </summary>
        public HashSet<T> Set => _set ??= HashSetPool<T>.Get();

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledHashSet{T}"/> struct.
        /// </summary>
        /// <param name="initialize">
        /// If <c>true</c>, the underlying set is obtained immediately; otherwise it is obtained on first access.
        /// </param>
        public PooledHashSet(bool initialize = false) : this()
        {
            if (initialize) _ = Set;
        }

        /// <summary>
        /// Returns the underlying set to the pool, making it available for reuse.
        /// </summary>
        public void Dispose()
        {
            if (_set != null)
            {
                HashSetPool<T>.Return(_set);
                _set = null;
            }
        }
    }

    /// <summary>
    /// Provides a disposable wrapper for a pooled <see cref="List{T}"/>.
    /// Use with <c>using</c> to ensure the list is returned to the pool.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public ref struct PooledList<T>
    {
        private List<T>? _list;

        /// <summary>
        /// Gets the underlying <see cref="List{T}"/>. If not yet obtained, it is retrieved from the pool.
        /// </summary>
        public List<T> List => _list ??= ListPool<T>.Get();

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledList{T}"/> struct.
        /// </summary>
        /// <param name="initialize">
        /// If <c>true</c>, the underlying list is obtained immediately; otherwise it is obtained on first access.
        /// </param>
        public PooledList(bool initialize = false) : this()
        {
            if (initialize) _ = List;
        }

        /// <summary>
        /// Returns the underlying list to the pool, making it available for reuse.
        /// </summary>
        public void Dispose()
        {
            if (_list != null)
            {
                ListPool<T>.Return(_list);
                _list = null;
            }
        }
    }
}