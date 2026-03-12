// <copyright file="AsyncSupportProperties.Constants.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
        /// Property names used in the <see cref="FluentBuilderAsyncSupportAttribute"/>.
        /// These keys correspond to named arguments of the attribute.
        /// </summary>
        public static class AsyncSupportProperties
        {
            /// <summary>If true, generates an async build method.</summary>
            public const string GenerateAsyncBuild = "GenerateAsyncBuild";

            /// <summary>Custom name for the async build method.</summary>
            public const string AsyncBuildMethodName = "AsyncBuildMethodName";

            /// <summary>Prefix for async validation methods (e.g., "Validate").</summary>
            public const string AsyncValidationPrefix = "AsyncValidationPrefix";

            /// <summary>If true, generates async validation methods.</summary>
            public const string GenerateAsyncValidation = "GenerateAsyncValidation";

            /// <summary>If true, adds CancellationToken parameters to async methods.</summary>
            public const string GenerateCancellationTokens = "GenerateCancellationTokens";
        }
    }
}