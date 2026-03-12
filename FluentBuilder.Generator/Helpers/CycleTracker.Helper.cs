// <copyright file="CycleTracker.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Helper for tracking builder cycles during generation.</summary>

using FluentBuilder.Generator.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Helpers
{
    /// <summary>
    /// Tracks the path of types being processed to detect circular references in builder chains.
    /// This struct is intended to be used with a <see cref="PooledHashSet{T}"/> for the processed set.
    /// </summary>
    internal readonly struct CycleTracker : IDisposable
    {
        private readonly HashSet<string> _processed;
        private readonly Stack<string> _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="CycleTracker"/> struct.
        /// </summary>
        /// <param name="processed">The HashSet tracking all processed types (usually pooled).</param>
        /// <param name="currentType">The fully qualified name of the current type.</param>
        /// <param name="currentTypeName">The simple name of the current type for path tracking.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processed"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CycleTracker(HashSet<string> processed, string currentType, string currentTypeName)
        {
            _processed = processed ?? throw new ArgumentNullException(nameof(processed));
            _path = new Stack<string>();

            _processed.Add(currentType);
            _path.Push(currentTypeName);
        }

        /// <summary>
        /// Gets the current cycle path as a formatted string (e.g., "TypeA -> TypeB -> TypeC").
        /// </summary>
        public string Path => string.Join(" -> ", _path.Reverse());

        /// <summary>
        /// Gets the number of types in the current path.
        /// </summary>
        public int Depth => _path.Count;

        /// <summary>
        /// Pushes a new type onto the path.
        /// </summary>
        /// <param name="typeName">The simple name of the type to push.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(string typeName)
        {
            _path.Push(typeName);
        }

        /// <summary>
        /// Pops the most recent type from the path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop()
        {
            _path.Pop();
        }

        /// <summary>
        /// Disposes the tracker. The HashSet is not disposed here as it is managed by the pool.
        /// This method only clears the internal path.
        /// </summary>
        public void Dispose()
        {
            // The HashSet is managed by the PooledHashSet's using statement,
            // so we only need to clear our path to avoid keeping references.
            while (_path.Count > 0)
            {
                _path.Pop();
            }
        }
    }
}