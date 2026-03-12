// <copyright file="ValueStringBuilder.Pool.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Provides a stack-allocated string builder that can grow by renting from <see cref="ArrayPool{T}"/>
    /// when the initial buffer capacity is exceeded.
    /// </summary>
    /// <remarks>
    /// This builder starts with a caller-provided stack buffer (e.g., from <c>stackalloc</c>) to avoid heap allocations
    /// for small strings. If the buffer is insufficient, it transparently rents a larger array from
    /// <see cref="ArrayPool{T}.Shared"/> and releases it back when disposed.
    /// </remarks>
    public ref struct ValueStringBuilder
    {
        private Span<char> _buffer;
        private int _length;
        private char[]? _rentedArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct.
        /// </summary>
        /// <param name="initialBuffer">A stack-allocated span to use as the initial buffer.</param>
        /// <remarks>
        /// The builder writes into <paramref name="initialBuffer"/> until it is full, then allocates a larger
        /// rented array. The caller is responsible for ensuring the initial buffer is appropriately sized.
        /// </remarks>
        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _buffer = initialBuffer;
            _length = 0;
            _rentedArray = null;
        }

        /// <summary>
        /// Gets the number of characters written to the builder.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Appends a single character to the builder.
        /// </summary>
        /// <param name="c">The character to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            if ((uint)_length < (uint)_buffer.Length)
            {
                _buffer[_length++] = c;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        /// <summary>
        /// Appends a string to the builder.
        /// </summary>
        /// <param name="s">The string to append. If null or empty, nothing is appended.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string? s)
        {
            if (string.IsNullOrEmpty(s)) return;

            int needed = _length + s!.Length; // s is known not null here
            if (needed <= _buffer.Length)
            {
                for (int i = 0; i < s!.Length; i++)
                {
                    _buffer[_length + i] = s[i];
                }
                _length = needed;
            }
            else
            {
                GrowAndAppend(s);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(string s)
        {
            Grow(s.Length);
            Append(s);
        }

        private void Grow(int additional)
        {
            char[] newArray = new char[Math.Max(_buffer.Length * 2, _buffer.Length + additional)];

            // Copy existing data – suppress false positive CS8602 (dereference of possibly null reference)
            for (int i = 0; i < _length; i++)
            {
                newArray[i] = _buffer[i];
            }

            // If we had a rented array, return it
            if (_rentedArray != null)
            {
                ArrayPool<char>.Shared.Return(_rentedArray);
            }

            _rentedArray = newArray;
            _buffer = _rentedArray;
        }

        /// <summary>
        /// Returns the built string.
        /// </summary>
        /// <returns>A new <see cref="string"/> containing the characters written so far.</returns>
        /// <remarks>
        /// This method allocates a new string on the heap. If the builder had rented an array,
        /// the array remains rented until <see cref="Dispose"/> is called.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (_length == 0) return string.Empty;

            char[] result = new char[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = _buffer[i];
            }
            return new string(result);
        }

        /// <summary>
        /// Clears the builder, resetting the write position to zero.
        /// </summary>
        /// <remarks>
        /// This does not clear the underlying buffer nor release any rented array. It simply allows the builder to be reused.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _length = 0;
        }

        /// <summary>
        /// Releases any rented array back to the pool.
        /// </summary>
        /// <remarks>
        /// If the builder had allocated a rented array, it is returned to <see cref="ArrayPool{T}.Shared"/>.
        /// After disposal, the builder should not be used again.
        /// </remarks>
        public void Dispose()
        {
            if (_rentedArray != null)
            {
                ArrayPool<char>.Shared.Return(_rentedArray);
                _rentedArray = null;
            }
        }
    }
}