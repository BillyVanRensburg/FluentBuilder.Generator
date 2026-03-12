// <copyright file="General.Constant.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

namespace FluentBuilder.Generator.Constants
{
    internal static partial class Constant
    {
        /// <summary>
        /// Default fallback values used when configuration is missing or invalid.
        /// </summary>
        public static class Defaults
        {
            /// <summary>Fallback builder suffix if none is configured.</summary>
            public const string BuilderSuffixFallback = "Builder";

            /// <summary>Fallback build method name if none is configured.</summary>
            public const string BuildMethodNameFallback = "Build";
        }
    }
}