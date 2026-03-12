// <copyright file="Memory.Pool.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Pools
{
    /// <summary>
    /// Provides a simplified interface to the shared <see cref="ArrayPool{T}"/> for renting and returning arrays.
    /// </summary>
    /// <remarks>
    /// This static class delegates all operations to <see cref="ArrayPool{T}.Shared"/>, which is a thread-safe,
    /// highly optimized pool for temporary array allocations.
    /// </remarks>
    internal static class MemoryPool
    {
        /// <summary>
        /// Rents a character array of at least the specified length from the shared <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array to rent.</param>
        /// <returns>A rented character array that may be larger than <paramref name="minimumLength"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char[] RentChar(int minimumLength)
        {
            return ArrayPool<char>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// Returns a character array to the shared <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <param name="array">The array to return. If <c>null</c>, the method does nothing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnChar(char[] array)
        {
            if (array != null)
            {
                ArrayPool<char>.Shared.Return(array);
            }
        }

        /// <summary>
        /// Rents an array of type <typeparamref name="T"/> of at least the specified length from the shared <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="minimumLength">The minimum length of the array to rent.</param>
        /// <returns>A rented array that may be larger than <paramref name="minimumLength"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Rent<T>(int minimumLength)
        {
            return ArrayPool<T>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// Returns an array of type <typeparamref name="T"/> to the shared <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to return. If <c>null</c>, the method does nothing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(T[] array)
        {
            if (array != null)
            {
                ArrayPool<T>.Shared.Return(array);
            }
        }
    }

    /// <summary>
    /// Provides a disposable wrapper for a rented array, ensuring it is returned to the pool when disposed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <remarks>
    /// Use this struct with a <c>using</c> statement to guarantee that the rented array is returned to the pool,
    /// even if an exception occurs.
    /// </remarks>
    public ref struct PooledArray<T>
    {
        private T[]? _array;
        private int _length;

        /// <summary>
        /// Gets the underlying rented array.
        /// </summary>
        /// <returns>The rented array.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the array has not been initialized (e.g., if the default constructor was used).</exception>
        public T[] Array
        {
            get
            {
                if (_array == null)
                {
                    throw new InvalidOperationException("Array not initialized. Use constructor with length.");
                }
                return _array;
            }
        }

        /// <summary>
        /// Gets the requested minimum length of the array.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct, renting an array of at least the specified length.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array to rent.</param>
        public PooledArray(int minimumLength)
        {
            _array = MemoryPool.Rent<T>(minimumLength);
            _length = minimumLength;
        }

        /// <summary>
        /// Returns the rented array to the pool, making it available for reuse.
        /// </summary>
        public void Dispose()
        {
            if (_array != null)
            {
                MemoryPool.Return(_array);
                _array = null;
            }
        }
    }
}