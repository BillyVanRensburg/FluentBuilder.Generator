using System;

namespace FluentBuilder.Generator.Constants
{
    internal static partial class Constant
    {
        /// <summary>
        /// Contains the simple (unqualified) names of all custom attributes
        /// recognised by the FluentBuilder source generator.
        /// </summary>
        internal static class AttributeName
        {
            // ==================== CORE BUILDER ATTRIBUTES ====================

            /// <summary>FluentBuilderAttribute</summary>
            internal const string FluentBuilder = nameof(FluentBuilderAttribute);

            /// <summary>FluentBuilderBuildMethodAttribute</summary>
            internal const string FluentBuilderBuildMethod = nameof(FluentBuilderBuildMethodAttribute);

            /// <summary>FluentNameAttribute</summary>
            internal const string FluentName = nameof(FluentNameAttribute);

            /// <summary>FluentIgnoreAttribute</summary>
            internal const string FluentIgnore = nameof(FluentIgnoreAttribute);

            /// <summary>FluentBuilderFactoryMethodAttribute</summary>
            internal const string FluentBuilderFactoryMethod = nameof(FluentBuilderFactoryMethodAttribute);

            /// <summary>FluentDefaultValueAttribute</summary>
            internal const string FluentDefaultValue = nameof(FluentDefaultValueAttribute);

            /// <summary>FluentImplicitAttribute</summary>
            internal const string FluentImplicit = nameof(FluentImplicitAttribute);

            /// <summary>FluentTruthOperatorAttribute</summary>
            internal const string FluentTruthOperator = nameof(FluentTruthOperatorAttribute);

            // ==================== INCLUSION ATTRIBUTE ====================

            /// <summary>FluentIncludeAttribute</summary>
            internal const string FluentInclude = nameof(FluentIncludeAttribute);

            // ==================== COLLECTION OPTIONS ATTRIBUTE ====================

            /// <summary>FluentCollectionOptionsAttribute</summary>
            internal const string FluentCollectionOptions = nameof(FluentCollectionOptionsAttribute);

            // ==================== VALIDATION ATTRIBUTES ====================

            // Base validation
            /// <summary>FluentValidateAttribute</summary>
            internal const string FluentValidate = nameof(FluentValidateAttribute);

            // Specific validators
            /// <summary>FluentValidateEmailAttribute</summary>
            internal const string FluentValidateEmail = nameof(FluentValidateEmailAttribute);

            /// <summary>FluentValidatePhoneAttribute</summary>
            internal const string FluentValidatePhone = nameof(FluentValidatePhoneAttribute);

            /// <summary>FluentValidateUrlAttribute</summary>
            internal const string FluentValidateUrl = nameof(FluentValidateUrlAttribute);

            // Comparison validators
            /// <summary>FluentValidateEqualAttribute</summary>
            internal const string FluentValidateEqual = nameof(FluentValidateEqualAttribute);

            /// <summary>FluentValidateNotEqualAttribute</summary>
            internal const string FluentValidateNotEqual = nameof(FluentValidateNotEqualAttribute);

            /// <summary>FluentValidateGreaterThanAttribute</summary>
            internal const string FluentValidateGreaterThan = nameof(FluentValidateGreaterThanAttribute);

            /// <summary>FluentValidateGreaterThanOrEqualAttribute</summary>
            internal const string FluentValidateGreaterThanOrEqual = nameof(FluentValidateGreaterThanOrEqualAttribute);

            /// <summary>FluentValidateLessThanAttribute</summary>
            internal const string FluentValidateLessThan = nameof(FluentValidateLessThanAttribute);

            /// <summary>FluentValidateLessThanOrEqualAttribute</summary>
            internal const string FluentValidateLessThanOrEqual = nameof(FluentValidateLessThanOrEqualAttribute);

            // Set validators
            /// <summary>FluentValidateOneOfAttribute</summary>
            internal const string FluentValidateOneOf = nameof(FluentValidateOneOfAttribute);

            // ==================== ASYNC SUPPORT ATTRIBUTES ====================

            /// <summary>FluentBuilderAsyncSupportAttribute</summary>
            internal const string FluentBuilderAsyncSupport = nameof(FluentBuilderAsyncSupportAttribute);

            /// <summary>FluentAsyncMethodAttribute</summary>
            internal const string FluentAsyncFluentMethod = nameof(FluentAsyncMethodAttribute);

            /// <summary>FluentValidateAsyncAttribute</summary>
            internal const string FluentValidateAsync = nameof(FluentValidateAsyncAttribute);

            /// <summary>FluentValidationMethodAttribute</summary>
            internal const string FluentValidationMethod = nameof(FluentValidationMethodAttribute);

            /// <summary>FluentValidateWithAttribute</summary>
            internal const string FluentValidateWith = nameof(FluentValidateWithAttribute);
        }
    }
}