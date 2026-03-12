// <copyright file="DefaultNames.Constants.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
        /// Contains default names used by the generator when no user override is provided.
        /// </summary>
        internal static class DefaultNames
        {
            // ==================== METHOD NAMES ====================

            /// <summary>Default name for the synchronous build method.</summary>
            internal const string Build = "Build";

            /// <summary>Default name for the asynchronous build method.</summary>
            internal const string BuildAsync = "BuildAsync";

            /// <summary>Default name for validation methods.</summary>
            internal const string Validate = "Validate";

            // ==================== BUILDER NAMES ====================

            /// <summary>Default suffix appended to the type name to form the builder class name.</summary>
            internal const string BuilderSuffix = "Builder";

            // ==================== FIELD NAMES ====================

            /// <summary>Default name for the backing field holding the instance under construction.</summary>
            internal const string InstanceField = "_instance";

            /// <summary>Default name for a flag indicating whether Build() has already been called.</summary>
            internal const string BuiltField = "_built";

            /// <summary>Default name for a flag tracking whether a value has been overridden.</summary>
            internal const string OverriddenField = "_overridden";
        }
    }
}