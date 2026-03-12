// <copyright file="PerformanceCounters.Diagnostic.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

#if DEBUG
using System.Threading;

namespace FluentBuilder.Generator.Diagnostics
{
    /// <summary>
    /// Performance counters for monitoring the effectiveness of various caching and pooling optimizations.
    /// This class is only active in DEBUG builds and helps diagnose performance bottlenecks.
    /// </summary>
    /// <remarks>
    /// All counters use <see cref="Interlocked"/> operations to ensure thread safety when accessed
    /// from multiple generator threads. The statistics can be dumped to the debug output for analysis.
    /// </remarks>
    internal static class PerformanceCounters
    {
        private static int _attributeCacheHits;
        private static int _attributeCacheMisses;
        private static int _stringBuilderGets;
        private static int _stringBuilderReturns;
        private static int _listPoolGets;
        private static int _hashSetPoolGets;

        /// <summary>
        /// Increments the count of attribute cache hits.
        /// </summary>
        public static void IncrementAttributeHit() => Interlocked.Increment(ref _attributeCacheHits);

        /// <summary>
        /// Increments the count of attribute cache misses.
        /// </summary>
        public static void IncrementAttributeMiss() => Interlocked.Increment(ref _attributeCacheMisses);

        /// <summary>
        /// Increments the count of <see cref="System.Text.StringBuilder"/> rentals from the pool.
        /// </summary>
        public static void IncrementStringBuilderGet() => Interlocked.Increment(ref _stringBuilderGets);

        /// <summary>
        /// Increments the count of <see cref="System.Text.StringBuilder"/> returns to the pool.
        /// </summary>
        public static void IncrementStringBuilderReturn() => Interlocked.Increment(ref _stringBuilderReturns);

        /// <summary>
        /// Increments the count of <see cref="System.Collections.Generic.List{T}"/> rentals from the pool.
        /// </summary>
        public static void IncrementListPoolGet() => Interlocked.Increment(ref _listPoolGets);

        /// <summary>
        /// Increments the count of <see cref="System.Collections.Generic.HashSet{T}"/> rentals from the pool.
        /// </summary>
        public static void IncrementHashSetPoolGet() => Interlocked.Increment(ref _hashSetPoolGets);

        /// <summary>
        /// Writes the current performance counter statistics to the debug output.
        /// </summary>
        /// <remarks>
        /// This method calculates the hit rate for the attribute cache and displays all counters
        /// in a formatted table. It is intended to be called at the end of a generation pass or
        /// during shutdown to evaluate optimization effectiveness.
        /// </remarks>
        public static void DumpStats()
        {
            System.Diagnostics.Debug.WriteLine($@"
                Performance Statistics:
                Attribute Cache: {_attributeCacheHits} hits, {_attributeCacheMisses} misses ({(double)_attributeCacheHits / (_attributeCacheHits + _attributeCacheMisses + 1):P} hit rate)
                StringBuilder: {_stringBuilderGets} gets, {_stringBuilderReturns} returns
                ListPool: {_listPoolGets} gets
                HashSetPool: {_hashSetPoolGets} gets
            ");
        }
    }
}
#endif