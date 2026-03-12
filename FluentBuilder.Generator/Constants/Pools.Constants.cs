// <copyright file="Pools.Constants.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System.Text;

namespace FluentBuilder.Generator.Constants
{
    internal static partial class Constant
    {
        /// <summary>
        /// Configuration constants for the <see cref="StringBuilder"/> pool used
        /// to reduce allocations during code generation.
        /// </summary>
        internal static class StringBuilderPool
        {
            /// <summary>Maximum number of builders to keep in the pool.</summary>
            internal const int MaxPoolSize = 30;

            /// <summary>Initial capacity for new <see cref="StringBuilder"/> instances.</summary>
            internal const int InitialCapacity = 4096;

            /// <summary>Maximum capacity allowed for a builder to be returned to the pool.</summary>
            internal const int MaxCapacity = 8192;
        }
    }
}