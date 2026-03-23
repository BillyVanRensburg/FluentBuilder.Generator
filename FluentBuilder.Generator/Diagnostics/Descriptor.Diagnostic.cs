// <copyright file="Descriptor.Diagnostic.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using Microsoft.CodeAnalysis;

namespace FluentBuilder.Generator.Diagnostics
{
    /// <summary>
    /// Diagnostic descriptors for all errors and warnings produced by the FluentBuilder generator.
    /// Contains predefined <see cref="DiagnosticDescriptor"/> instances for each diagnostic ID (FB001‑FB999).
    /// </summary>
    internal static class Descriptor
    {
        // ==================== ERROR CODES (FB001-FB020) ====================

        /// <summary>
        /// FB015: Read-only property detected.
        /// </summary>
        public static readonly DiagnosticDescriptor ReadOnlyPropertyWarning = new DiagnosticDescriptor(
            id: "FB015",
            title: "Read-only property detected",
            messageFormat: "Property '{0}' is read-only. Builder methods won't be generated. Consider adding a setter or using [Ignore] attribute.",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB016: Collection type without Add method.
        /// </summary>
        public static readonly DiagnosticDescriptor CollectionWithoutAddMethod = new DiagnosticDescriptor(
            id: "FB016",
            title: "Collection type without Add method",
            messageFormat: "Type '{0}' appears to be a collection but doesn't have an Add method. Collection helper methods won't be generated.",
            category: "FluentBuilder",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        /// <summary>
        /// FB017: FluentName cannot be empty.
        /// </summary>
        public static readonly DiagnosticDescriptor EmptyFluentNameError = new DiagnosticDescriptor(
            id: "FB017",
            title: "FluentName cannot be empty",
            messageFormat: "FluentName attribute on '{0}' cannot have an empty or whitespace value. Value was: '{1}'.",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB018: FluentName is not a valid identifier.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFluentNameIdentifierError = new DiagnosticDescriptor(
            id: "FB018",
            title: "FluentName is not a valid identifier",
            messageFormat: "FluentName attribute on '{0}' has invalid value '{1}'. Name must be a valid C# identifier (letters, digits, underscore, cannot start with digit, not a keyword).",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB019: Generator orchestrator not initialized.
        /// </summary>
        public static readonly DiagnosticDescriptor OrchestratorNotInitializedError = new DiagnosticDescriptor(
            id: "FB019",
            title: "Generator orchestrator not initialized",
            messageFormat: "Generator orchestrator was not properly initialized for type '{0}'. This is an internal error in the FluentBuilder generator.",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB020: Internal generator error.
        /// </summary>
        public static readonly DiagnosticDescriptor InternalGeneratorError = new DiagnosticDescriptor(
            id: "FB020",
            title: "Internal generator error",
            messageFormat: "Internal error generating builder for '{0}': {1}",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB021: Type transformation error.
        /// </summary>
        public static readonly DiagnosticDescriptor TransformationError = new DiagnosticDescriptor(
            id: "FB021",
            title: "Type transformation error",
            messageFormat: "Error transforming type '{0}': {1}",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB022: Generation cancelled.
        /// </summary>
        public static readonly DiagnosticDescriptor GenerationCancelled = new DiagnosticDescriptor(
            id: "FB022",
            title: "Generation cancelled",
            messageFormat: "Builder generation for '{0}' was cancelled",
            category: "FluentBuilder",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        /// <summary>
        /// FB023: Cyclic dependency detected.
        /// </summary>
        public static readonly DiagnosticDescriptor CyclicDependencyDetected = new DiagnosticDescriptor(
            id: "FB023",
            title: "Cyclic dependency detected",
            messageFormat: "Cyclic dependency detected in builder chain: {0}",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB024: FluentName attribute without FluentBuilder.
        /// </summary>
        public static readonly DiagnosticDescriptor FluentNameWithoutBuilder = new DiagnosticDescriptor(
            id: "FB024",
            title: "FluentName attribute without FluentBuilder",
            messageFormat: "Type '{0}' has FluentName attributes but is not decorated with [FluentBuilder]. These attributes will be ignored.",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB025: Generator information.
        /// </summary>
        public static readonly DiagnosticDescriptor GeneratorInformation = new DiagnosticDescriptor(
            id: "FB025",
            title: "Generator Information",
            messageFormat: "{0}",
            category: "FluentBuilder",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        /// <summary>
        /// FB026: Generator warning.
        /// </summary>
        public static readonly DiagnosticDescriptor GeneratorWarning = new DiagnosticDescriptor(
            id: "FB026",
            title: "Generator Warning",
            messageFormat: "{0}",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB027: Async validator missing public parameterless constructor.
        /// </summary>
        public static readonly DiagnosticDescriptor AsyncValidatorMissingConstructor = new DiagnosticDescriptor(
            id: "FB027",
            title: "Async validator missing public parameterless constructor",
            messageFormat: "Async validator type '{0}' must have a public parameterless constructor",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB028: Invalid builder accessibility.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidBuilderAccessibility = new DiagnosticDescriptor(
            id: "FB028",
            title: "Invalid builder accessibility",
            messageFormat: "The builder accessibility '{0}' is not a valid C# access modifier",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB029: Invalid builder namespace.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidBuilderNamespace = new DiagnosticDescriptor(
            id: "FB029",
            title: "Invalid builder namespace",
            messageFormat: "The builder namespace '{0}' is not a valid C# namespace identifier",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB030: Invalid method prefix.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidMethodPrefix = new DiagnosticDescriptor(
            id: "FB030",
            title: "Invalid method prefix",
            messageFormat: "The method prefix '{0}' is not a valid C# identifier",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB031: Invalid method suffix.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidMethodSuffix = new DiagnosticDescriptor(
            id: "FB031",
            title: "Invalid method suffix",
            messageFormat: "The method suffix '{0}' is not a valid C# identifier",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB032: Generic type parameter is variant (in/out) which may cause issues with builder methods.
        /// </summary>
        public static readonly DiagnosticDescriptor GenericTypeWithVariance = new DiagnosticDescriptor(
            id: "FB032",
            title: "Generic type parameter is variant",
            messageFormat: "Generic type parameter '{0}' is declared as '{1}'. Variance may cause issues with builder method signatures.",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB033: Builder generated for a struct (unusual but allowed).
        /// </summary>
        public static readonly DiagnosticDescriptor StructWithBuilder = new DiagnosticDescriptor(
            id: "FB033",
            title: "Builder generated for a struct",
            messageFormat: "Type '{0}' is a struct. The builder pattern is typically used with classes, but generating a builder for a struct is supported.",
            category: "FluentBuilder",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        /// <summary>
        /// FB034: Type parameter has 'unmanaged' constraint.
        /// </summary>
        public static readonly DiagnosticDescriptor UnmanagedConstraint = new DiagnosticDescriptor(
            id: "FB034",
            title: "Type parameter has 'unmanaged' constraint",
            messageFormat: "Type parameter '{0}' has the 'unmanaged' constraint. Ensure the builder's usage respects unmanaged requirements.",
            category: "FluentBuilder",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        /// <summary>
        /// FB035: Fluent method name conflicts with builder's built-in method.
        /// </summary>
        public static readonly DiagnosticDescriptor FluentMethodNameConflict = new DiagnosticDescriptor(
            id: "FB035",
            title: "Fluent method name conflicts with builder's built-in method",
            messageFormat: "The fluent method name '{0}' conflicts with the builder's '{1}' method and will not be generated",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB036: Builder method already exists in partial class.
        /// </summary>
        public static readonly DiagnosticDescriptor BuilderMethodAlreadyExists = new DiagnosticDescriptor(
            id: "FB036",
            title: "Builder method already exists in partial class",
            messageFormat: "The builder method '{0}' already exists in a partial part of the builder and will not be generated",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB037: Factory method not found.
        /// </summary>
        public static readonly DiagnosticDescriptor FactoryMethodNotFound = new DiagnosticDescriptor(
            id: "FB037",
            title: "Factory method not found",
            messageFormat: "Factory method '{0}' not found on type '{1}'. The method must be public, static, parameterless, and return the type.",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB038: Factory method has invalid signature.
        /// </summary>
        public static readonly DiagnosticDescriptor FactoryMethodInvalidSignature = new DiagnosticDescriptor(
            id: "FB038",
            title: "Factory method has invalid signature",
            messageFormat: "Factory method '{0}' must be public, static, parameterless, and return the type '{1}'",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB039: Factory method not supported for records or immutable classes.
        /// </summary>
        public static readonly DiagnosticDescriptor FactoryMethodNotSupported = new DiagnosticDescriptor(
            id: "FB039",
            title: "Factory method not supported for records or immutable classes",
            messageFormat: "Factory method is not supported for records or classes with init‑only properties. The builder will use constructors instead.",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB040: Custom validation method has invalid signature.
        /// </summary>
        public static readonly DiagnosticDescriptor CustomValidationMethodInvalid = new DiagnosticDescriptor(
            id: "FB040",
            title: "Custom validation method has invalid signature",
            messageFormat: "Custom validation method '{0}' must be public, parameterless, and return void, bool, Task, or Task<bool>",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB041: FluentName attribute is unused.
        /// </summary>
        public static readonly DiagnosticDescriptor FluentNameUnused = new DiagnosticDescriptor(
            id: "FB041",
            title: "FluentName attribute is unused",
            messageFormat: "The [FluentName] attribute on '{0}' is ignored because the member is not included in the builder",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// FB042: Third-party validator missing public parameterless constructor.
        /// </summary>
        public static readonly DiagnosticDescriptor ThirdPartyValidatorMissingConstructor = new DiagnosticDescriptor(
            id: "FB042",
            title: "Validator missing public parameterless constructor",
            messageFormat: "Validator type '{0}' must have a public parameterless constructor",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB043: Third-party validator method not found or has invalid signature.
        /// </summary>
        public static readonly DiagnosticDescriptor ThirdPartyValidatorMethodNotFound = new DiagnosticDescriptor(
            id: "FB043",
            title: "Validator method not found or invalid",
            messageFormat: "Validator method '{0}' not found on type '{1}' with correct signature (public instance method taking the member's type and returning bool)",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// FB044: Builder accessibility ignored for file-scoped type.
        /// </summary>
        public static readonly DiagnosticDescriptor FileScopedAccessibilityIgnored = new DiagnosticDescriptor(
            id: "FB044",
            title: "Builder accessibility ignored",
            messageFormat: "The builder for a file-scoped type must be file-scoped. The specified accessibility '{0}' is ignored.",
            category: "FluentBuilder",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        // ADDED: New diagnostic for containing types not being partial.
        /// <summary>
        /// FB045: Containing type must be partial to host a nested builder.
        /// </summary>
        // public static readonly DiagnosticDescriptor ContainingTypeMustBePartial = new DiagnosticDescriptor(
        //    id: "FB045",
        //    title: "Containing type must be partial",
        //    messageFormat: "The containing type '{0}' must be declared as 'partial' to allow the builder to be nested inside it.",
        //    category: "FluentBuilder",
        //    DiagnosticSeverity.Error,
        //    isEnabledByDefault: true);


        /// <summary>
        /// FB999: Legacy generator error (kept for backward compatibility).
        /// </summary>
        public static readonly DiagnosticDescriptor GeneratorErrorLegacy = new DiagnosticDescriptor(
            id: "FB999",
            title: "Generator Error",
            messageFormat: "Error generating builder for '{0}': {1}",
            category: "FluentBuilder",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
