// <copyright file="AttributeTypes.Constants.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;

namespace FluentBuilder.Generator.Constants
{
    internal static partial class Constant
    {
        /// <summary>
        /// Contains the fully qualified type names (as returned by <see cref="Type.FullName"/>)
        /// of all custom attributes recognised by the FluentBuilder source generator.
        /// Useful for comparing attribute types in a source generator context.
        /// </summary>
        internal static class AttributeType
        {
            // ==================== CORE BUILDER ATTRIBUTES ====================

            /// <summary>Fully qualified name of FluentBuilderAttribute</summary>
            internal static readonly string FluentBuilder = typeof(FluentBuilderAttribute).FullName;

            /// <summary>Fully qualified name of FluentBuilderBuildMethodAttribute</summary>
            internal static readonly string FluentBuilderBuildMethod = typeof(FluentBuilderBuildMethodAttribute).FullName;

            /// <summary>Fully qualified name of FluentNameAttribute</summary>
            internal static readonly string FluentName = typeof(FluentNameAttribute).FullName;

            /// <summary>Fully qualified name of FluentIgnoreAttribute</summary>
            internal static readonly string FluentIgnore = typeof(FluentIgnoreAttribute).FullName;

            /// <summary>Fully qualified name of FluentBuilderFactoryMethodAttribute</summary>
            internal static readonly string FluentBuilderFactoryMethod = typeof(FluentBuilderFactoryMethodAttribute).FullName;

            /// <summary>Fully qualified name of FluentDefaultValueAttribute</summary>
            internal static readonly string FluentDefaultValue = typeof(FluentDefaultValueAttribute).FullName;

            /// <summary>Fully qualified name of FluentImplicitAttribute</summary>
            internal static readonly string FluentImplicit = typeof(FluentImplicitAttribute).FullName;

            /// <summary>Fully qualified name of FluentTruthOperatorAttribute</summary>
            internal static readonly string FluentTruthOperator = typeof(FluentTruthOperatorAttribute).FullName;

            // ==================== INCLUSION ATTRIBUTE ====================

            /// <summary>Fully qualified name of FluentIncludeAttribute</summary>
            internal static readonly string FluentInclude = typeof(FluentIncludeAttribute).FullName;

            // ==================== COLLECTION OPTIONS ATTRIBUTE ====================

            /// <summary>Fully qualified name of FluentCollectionOptionsAttribute</summary>
            internal static readonly string FluentCollectionOptions = typeof(FluentCollectionOptionsAttribute).FullName;

            // ==================== VALIDATION ATTRIBUTES ====================

            // Base validation
            /// <summary>Fully qualified name of FluentValidateAttribute</summary>
            internal static readonly string FluentValidate = typeof(FluentValidateAttribute).FullName;

            // Specific validators
            /// <summary>Fully qualified name of FluentValidateEmailAttribute</summary>
            internal static readonly string FluentValidateEmail = typeof(FluentValidateEmailAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidatePhoneAttribute</summary>
            internal static readonly string FluentValidatePhone = typeof(FluentValidatePhoneAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateUrlAttribute</summary>
            internal static readonly string FluentValidateUrl = typeof(FluentValidateUrlAttribute).FullName;

            // Comparison validators
            /// <summary>Fully qualified name of FluentValidateEqualAttribute</summary>
            internal static readonly string FluentValidateEqual = typeof(FluentValidateEqualAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateNotEqualAttribute</summary>
            internal static readonly string FluentValidateNotEqual = typeof(FluentValidateNotEqualAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateGreaterThanAttribute</summary>
            internal static readonly string FluentValidateGreaterThan = typeof(FluentValidateGreaterThanAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateGreaterThanOrEqualAttribute</summary>
            internal static readonly string FluentValidateGreaterThanOrEqual = typeof(FluentValidateGreaterThanOrEqualAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateLessThanAttribute</summary>
            internal static readonly string FluentValidateLessThan = typeof(FluentValidateLessThanAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateLessThanOrEqualAttribute</summary>
            internal static readonly string FluentValidateLessThanOrEqual = typeof(FluentValidateLessThanOrEqualAttribute).FullName;

            // Set validators
            /// <summary>Fully qualified name of FluentValidateOneOfAttribute</summary>
            internal static readonly string FluentValidateOneOf = typeof(FluentValidateOneOfAttribute).FullName;

            // ==================== ASYNC SUPPORT ATTRIBUTES ====================

            /// <summary>Fully qualified name of FluentBuilderAsyncSupportAttribute</summary>
            internal static readonly string FluentBuilderAsyncSupport = typeof(FluentBuilderAsyncSupportAttribute).FullName;

            /// <summary>Fully qualified name of FluentAsyncMethodAttribute</summary>
            internal static readonly string FluentAsyncFluentMethod = typeof(FluentAsyncMethodAttribute).FullName;

            /// <summary>Fully qualified name of FluentValidateAsyncAttribute</summary>
            internal static readonly string FluentValidateAsync = typeof(FluentValidateAsyncAttribute).FullName;
        }
    }
}