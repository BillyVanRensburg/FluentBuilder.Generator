// <copyright file="ObjectPool.Implementation.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using System.Threading;

namespace FluentBuilder.Generator.Implementations
{
    /// <summary>
    /// A simple, thread‑safe object pool for reusable instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool. Must be a reference type.</typeparam>
    /// <remarks>
    /// <para>
    /// This pool uses a <see cref="ThreadStaticAttribute"/> fast path to store one instance per thread,
    /// avoiding contention in high‑frequency acquire/release scenarios. Additional instances
    /// are stored in a lock‑free <see cref="ConcurrentStack{T}"/>.
    /// </para>
    /// <para>
    /// When an instance is returned, it is passed through a user‑provided reset action
    /// to restore it to a clean state before being made available again.
    /// </para>
    /// </remarks>
    internal sealed class ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly Action<T> _reset;
        private readonly int _maxSize;

        [ThreadStatic]
        private static T? _fastInstance;

        private readonly ConcurrentStack<T> _stack = new();
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="factory">A delegate that creates new instances of <typeparamref name="T"/>.</param>
        /// <param name="reset">
        /// A delegate that resets an instance to a clean state before it is returned to the pool.
        /// This is called on every returned instance.
        /// </param>
        /// <param name="maxSize">The maximum number of instances that can be stored in the shared stack. Default is 10.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> or <paramref name="reset"/> is <c>null</c>.</exception>
        public ObjectPool(Func<T> factory, Action<T> reset, int maxSize = 10)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (reset == null) throw new ArgumentNullException(nameof(reset));

            _factory = factory;
            _reset = reset;
            _maxSize = maxSize;
        }

        /// <summary>
        /// Retrieves an instance from the pool, or creates a new one if the pool is empty.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/> ready for use.</returns>
        /// <remarks>
        /// This method first checks the thread‑static fast path; if an instance is available there,
        /// it is returned immediately without any synchronization. Otherwise, it attempts to pop
        /// an instance from the shared stack. If the stack is empty, a new instance is created
        /// using the factory delegate.
        /// </remarks>
        public T Get()
        {
            var instance = _fastInstance;
            if (instance != null)
            {
                _fastInstance = null;
                return instance;
            }

            if (_stack.TryPop(out instance))
            {
                Interlocked.Decrement(ref _count);
                return instance;
            }

            return _factory();
        }

        /// <summary>
        /// Returns an instance to the pool, making it available for reuse.
        /// </summary>
        /// <param name="instance">The instance to return. May be <c>null</c> (ignored).</param>
        /// <remarks>
        /// <para>
        /// The instance is first passed through the reset delegate to restore it to a clean state.
        /// </para>
        /// <para>
        /// If the thread‑static slot is empty, the instance is stored there and no further action
        /// is taken. This ensures each thread can have its own cached instance without contention.
        /// </para>
        /// <para>
        /// If the thread‑static slot is already occupied, the instance is pushed onto the shared stack,
        /// provided the stack size has not reached <see cref="_maxSize"/>. Instances beyond the limit
        /// are simply discarded and become eligible for garbage collection.
        /// </para>
        /// </remarks>
        public void Return(T? instance)
        {
            if (instance == null) return;

            _reset(instance);

            if (_fastInstance == null)
            {
                _fastInstance = instance;
                return;
            }

            if (_count < _maxSize)
            {
                _stack.Push(instance);
                Interlocked.Increment(ref _count);
            }
        }
    }
}