// <copyright file="FastStringBuilder.Pool.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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

namespace FluentBuilder.Generator.Pools
{
    /// <summary>
    /// Provides a fast, stack-allocated string builder for small, frequent concatenations
    /// with zero heap allocations in the common case.
    /// </summary>
    /// <remarks>
    /// This builder writes directly into a caller-provided <see cref="Span{T}"/> and does not
    /// allocate managed memory. It is ideal for building short strings in performance-critical paths.
    /// If the buffer capacity is exceeded, additional characters are silently discarded.
    /// </remarks>
    public ref struct FastStringBuilder
    {
        private Span<char> _buffer;
        private int _pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastStringBuilder"/> struct.
        /// </summary>
        /// <param name="buffer">The span of characters to write into.</param>
        public FastStringBuilder(Span<char> buffer)
        {
            _buffer = buffer;
            _pos = 0;
        }

        /// <summary>
        /// Appends a single character to the builder.
        /// </summary>
        /// <param name="c">The character to append.</param>
        /// <remarks>
        /// If the internal buffer is full, the character is silently discarded.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            if (_pos < _buffer.Length)
                _buffer[_pos++] = c;
        }

        /// <summary>
        /// Appends a string to the builder.
        /// </summary>
        /// <param name="s">The string to append. If null or empty, nothing is appended.</param>
        /// <remarks>
        /// If the remaining buffer space is insufficient, the string is truncated or ignored.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string s)
        {
            if (string.IsNullOrEmpty(s)) return;

            if (_pos + s.Length <= _buffer.Length)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    _buffer[_pos++] = s[i];
                }
            }
        }

        /// <summary>
        /// Appends the string representation of an integer to the builder.
        /// </summary>
        /// <param name="value">The integer value to append.</param>
        /// <remarks>
        /// Single-digit values (0–9) are appended without allocation. Larger values
        /// use <see cref="int.ToString()"/> and may allocate temporarily.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(int value)
        {
            // Fast path for single-digit numbers
            if (value >= 0 && value < 10 && _pos < _buffer.Length)
            {
                _buffer[_pos++] = (char)('0' + value);
                return;
            }

            // Fallback for multi-digit numbers
            string str = value.ToString();
            Append(str);
        }

        /// <summary>
        /// Returns the built string.
        /// </summary>
        /// <returns>A new <see cref="string"/> containing the characters written so far.</returns>
        /// <remarks>
        /// This method allocates a new string on the heap. For zero‑allocation scenarios,
        /// consider accessing the buffer directly via <see cref="Span{T}"/> slicing.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return new string(_buffer.Slice(0, _pos).ToArray());
        }

        /// <summary>
        /// Clears the builder, resetting the write position to zero.
        /// </summary>
        /// <remarks>
        /// This does not clear the underlying buffer; it simply allows the buffer to be reused.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _pos = 0;
        }
    }
}