// <copyright file="StringBuilder.Pool.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;
using System.Runtime.CompilerServices;
using System.Text;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Implementations;

namespace FluentBuilder.Generator.Pools
{
    /// <summary>
    /// Provides a pool of reusable <see cref="StringBuilder"/> instances to reduce allocations.
    /// </summary>
    /// <remarks>
    /// The pool combines a thread-static fast path with a shared LIFO stack for overflow.
    /// Instances are cleared before being returned to the pool.
    /// Pool size and initial capacity are configured via <see cref="Constant.StringBuilderPool"/>.
    /// </remarks>
    internal static class StringBuilderPool
    {
        private static int _lastSize = Constant.StringBuilderPool.InitialCapacity;
        private static readonly ObjectPool<StringBuilder> _pool =
            new(
                () => new StringBuilder(Constant.StringBuilderPool.InitialCapacity),
                sb => sb.Clear(),
                maxSize: Constant.StringBuilderPool.MaxPoolSize
            );

        /// <summary>
        /// Retrieves a <see cref="StringBuilder"/> instance from the pool.
        /// </summary>
        /// <returns>A cleared <see cref="StringBuilder"/> ready for use.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Get() => _pool.Get();

        /// <summary>
        /// Returns a <see cref="StringBuilder"/> to the pool for reuse.
        /// </summary>
        /// <param name="sb">The builder to return. It will be cleared before being made available again.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(StringBuilder sb) => _pool.Return(sb);

        /// <summary>
        /// Retrieves a <see cref="StringBuilder"/> from the pool with a guaranteed minimum capacity.
        /// </summary>
        /// <param name="estimatedSize">The estimated minimum capacity required.</param>
        /// <returns>A cleared <see cref="StringBuilder"/> with capacity at least <paramref name="estimatedSize"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder GetSized(int estimatedSize)
        {
            var sb = Get();
            if (sb.Capacity < estimatedSize)
                sb.Capacity = estimatedSize;
            _lastSize = estimatedSize;
            return sb;
        }

        /// <summary>
        /// Converts the builder to a string and returns it to the pool.
        /// </summary>
        /// <param name="sb">The builder to convert and return. May be null.</param>
        /// <returns>The string built by <paramref name="sb"/>, or <see cref="string.Empty"/> if <paramref name="sb"/> is null.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringAndReturn(this StringBuilder? sb)
        {
            if (sb == null) return string.Empty;
            var result = sb.ToString();
            Return(sb);
            return result;
        }

        /// <summary>
        /// Provides a disposable wrapper for a pooled <see cref="StringBuilder"/>.
        /// Use with <c>using</c> to ensure the builder is returned to the pool.
        /// </summary>
        public readonly ref struct PooledBuilder
        {
            /// <summary>
            /// Gets the underlying <see cref="StringBuilder"/>.
            /// </summary>
            public StringBuilder Builder { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PooledBuilder"/> struct.
            /// </summary>
            /// <param name="builder">The pooled builder instance.</param>
            public PooledBuilder(StringBuilder builder) => Builder = builder;

            /// <summary>
            /// Returns the underlying builder to the pool.
            /// </summary>
            public void Dispose() => Return(Builder);
        }

        /// <summary>
        /// Rents a <see cref="StringBuilder"/> from the pool wrapped in a disposable <see cref="PooledBuilder"/>.
        /// </summary>
        /// <returns>A <see cref="PooledBuilder"/> that will return the builder when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledBuilder Rent() => new(Get());
    }
}