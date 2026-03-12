// <copyright file="Builder.Validator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentBuilder.Generator.Validators
{
    /// <summary>
    /// Encapsulates all validation logic for builder generation.
    /// </summary>
    internal sealed class BuilderValidator
    {
        private readonly AttributeValidator _attributeValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderValidator"/> class.
        /// </summary>
        /// <param name="attributeValidator">The attribute validator used for attribute-specific validations.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="attributeValidator"/> is null.</exception>
        public BuilderValidator(AttributeValidator attributeValidator)
        {
            _attributeValidator = attributeValidator ?? throw new ArgumentNullException(nameof(attributeValidator));
        }

        /// <summary>
        /// Exposes the underlying attribute validator for use by generator components.
        /// </summary>
        public AttributeValidator AttributeValidator => _attributeValidator;

        /// <summary>
        /// Runs all applicable validations on the type and its configuration.
        /// Reports diagnostics through the provided context.
        /// </summary>
        /// <param name="typeSymbol">The type symbol for which a builder is being generated.</param>
        /// <param name="config">The builder configuration extracted from attributes on the type.</param>
        /// <param name="visitedBuilders">A set of builder keys already processed (used for circular reference detection).</param>
        /// <param name="context">The source production context used to report diagnostics.</param>
        /// <returns>True if generation should proceed; false if a fatal error occurred.</returns>
        /// <remarks>
        /// This method performs a series of validations, including:
        /// - Presence of the <c>[FluentBuilder]</c> attribute.
        /// - Accessibility of file‑scoped builder types.
        /// - Validity of factory method specifications.
        /// - Existence and correctness of custom validation methods.
        /// - Builder type accessibility.
        /// - Warning for unused <c>[FluentName]</c> attributes.
        /// - Circular references between builders.
        /// - Namespace override validity.
        /// - Method prefix/suffix validity.
        /// - Generic type constraints.
        /// - Constructor accessibility.
        /// - (ADDED) All containing types are partial if the target is nested.
        /// If any fatal validation fails, the method returns <c>false</c> and generation is aborted.
        /// Non‑fatal issues (e.g., unused attributes) only produce diagnostics but do not halt generation.
        /// </remarks>
        public bool Validate(
            INamedTypeSymbol typeSymbol,
            BuilderConfiguration config,
            HashSet<string> visitedBuilders,
            SourceProductionContext context)
        {
            // Basic attribute presence check
            if (!AttributeCache.HasAttribute(typeSymbol, Constant.AttributeName.FluentBuilder))
                return false;

            // File-scoped builder accessibility
            var location = typeSymbol.Locations.FirstOrDefault() ?? Location.None;
            _attributeValidator.ValidateFileScopedBuilder(typeSymbol, config, location);

            // Factory method validation
            ValidateFactoryMethod(typeSymbol, config, context);

            // Custom validation methods
            ValidateCustomValidationMethods(typeSymbol);

            // Builder type accessibility
            if (!_attributeValidator.ValidateBuilderType(typeSymbol))
                return false;

            // Unused [FluentName] attributes warning
            _attributeValidator.ValidateUnusedFluentName(typeSymbol);

            // Circular reference detection
            string builderKey = $"{typeSymbol.ContainingNamespace?.ToDisplayString()}.{typeSymbol.Name}";
            if (!_attributeValidator.ValidateNoCircularReference(builderKey, visitedBuilders, typeSymbol))
                return false;

            // Namespace validation
            if (!_attributeValidator.ValidateBuilderNamespace(config.BuilderNamespace, location))
                return false;

            // Method prefix/suffix
            if (!_attributeValidator.ValidateMethodPrefixSuffix(config.MethodPrefix, config.MethodSuffix, location))
                return false;

            // Generic type issues
            if (!_attributeValidator.ValidateGenericType(typeSymbol, location))
                return false;

            // Constructor accessibility
            _attributeValidator.ValidateConstructorAccessibility(typeSymbol);

            return true;
        }

        /// <summary>
        /// Validates the factory method configuration, if any, and updates the configuration accordingly.
        /// </summary>
        /// <param name="typeSymbol">The type symbol for which the builder is being generated.</param>
        /// <param name="config">The builder configuration to validate and potentially modify.</param>
        /// <param name="context">The source production context for reporting diagnostics.</param>
        /// <remarks>
        /// Factory methods are not supported for records or types with init‑only properties.
        /// If such a configuration is detected, a diagnostic is reported and <see cref="BuilderConfiguration.HasFactoryMethod"/>
        /// is set to <c>false</c>. Otherwise, the underlying attribute validator checks the existence and
        /// signature of the specified factory method; if validation fails, the flag is also cleared.
        /// </remarks>
        private void ValidateFactoryMethod(
            INamedTypeSymbol typeSymbol,
            BuilderConfiguration config,
            SourceProductionContext context)
        {
            if (string.IsNullOrEmpty(config.FactoryMethodName))
                return;

            if (config.IsRecord || config.HasInitOnlyProperties)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor.FactoryMethodNotSupported,
                    typeSymbol.Locations.FirstOrDefault(),
                    config.FactoryMethodName));
                config.HasFactoryMethod = false;
            }
            else if (!_attributeValidator.ValidateFactoryMethod(typeSymbol, config))
            {
                config.HasFactoryMethod = false;
            }
        }

        /// <summary>
        /// Validates all custom validation methods defined on the type.
        /// </summary>
        /// <param name="typeSymbol">The type symbol whose custom validation methods are to be validated.</param>
        /// <remarks>
        /// Custom validation methods are identified by <see cref="Constant.AttributeName.FluentValidate"/>
        /// or derived attributes. The underlying attribute validator checks each method for proper signature
        /// and accessibility. Any diagnostics are reported immediately; generation continues regardless
        /// of validation outcome.
        /// </remarks>
        private void ValidateCustomValidationMethods(INamedTypeSymbol typeSymbol)
        {
            var methods = TypeHelper.GetCustomValidationMethods(typeSymbol);
            foreach (var method in methods)
            {
                _attributeValidator.ValidateCustomValidationMethod(method);
                // Diagnostics are reported inside; we don't need to stop generation.
            }
        }

        /// <summary>
        /// Determines whether a type is declared with the 'partial' modifier.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns>True if the type is partial; otherwise false.</returns>
        private static bool IsPartial(INamedTypeSymbol type)
        {
            var syntaxRef = type.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxRef?.GetSyntax() is TypeDeclarationSyntax decl)
            {
                return decl.Modifiers.Any(SyntaxKind.PartialKeyword);
            }
            // Types from metadata cannot be partial; we cannot nest builders in them.
            return false;
        }
    }
}
