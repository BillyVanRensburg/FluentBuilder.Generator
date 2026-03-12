// <copyright file="BuilderConfiguration.Parameters.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Constants;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace FluentBuilder.Generator.Parameters
{
    /// <summary>
    /// Holds configuration settings for a specific builder type.
    /// </summary>
    public class BuilderConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the builder class.
        /// </summary>
        public string BuilderName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the synchronous build method (e.g., "Build").
        /// </summary>
        public string BuildMethodName { get; set; } = Constant.DefaultNames.Build;

        /// <summary>
        /// Gets or sets the name of the asynchronous build method (e.g., "BuildAsync").
        /// </summary>
        public string AsyncBuildMethodName { get; set; } = Constant.DefaultNames.BuildAsync;

        /// <summary>
        /// Gets or sets a value indicating whether async build methods should be generated.
        /// </summary>
        public bool HasAsyncSupport { get; set; }

        /// <summary>
        /// Gets or sets the prefix used for async validation methods (e.g., "Validate").
        /// </summary>
        public string AsyncValidationPrefix { get; set; } = Constant.DefaultNames.Validate;

        /// <summary>
        /// Gets or sets a value indicating whether async validation methods should be generated.
        /// </summary>
        public bool AsyncValidationEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an implicit conversion operator should be generated.
        /// </summary>
        public bool HasImplicitOperator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the builder class should be generated as partial.
        /// </summary>
        public bool IsPartial { get; set; }

        /// <summary>
        /// Gets or sets the accessibility modifier for the builder class (e.g., "public", "internal").
        /// </summary>
        public string BuilderAccessibility { get; set; } = "public";

        /// <summary>
        /// Gets or sets a value indicating whether the target type is a record.
        /// </summary>
        public bool IsRecord { get; set; }

        /// <summary>
        /// Gets a set of custom fluent method names derived from <see cref="FluentNameAttribute"/>.
        /// </summary>
        public HashSet<string> FluentMethodNames { get; } = new();

        /// <summary>
        /// Gets or sets the namespace for the builder class.
        /// If null or empty, the builder is placed in the target type's namespace.
        /// </summary>
        public string? BuilderNamespace { get; set; }

        /// <summary>
        /// Gets or sets the prefix to apply to all fluent method names.
        /// </summary>
        public string MethodPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the suffix to apply to all fluent method names.
        /// </summary>
        public string MethodSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Gets the type parameters of the target generic type, if any.
        /// </summary>
        public List<ITypeSymbol> TypeParameters { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the target type has any init-only properties.
        /// </summary>
        public bool HasInitOnlyProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a factory method should be used instead of a constructor.
        /// </summary>
        public bool HasFactoryMethod { get; set; }

        /// <summary>
        /// Gets or sets the name of the factory method.
        /// </summary>
        public string? FactoryMethodName { get; set; }

        /// <summary>
        /// Gets or sets the factory method symbol (if found and valid).
        /// </summary>
        public IMethodSymbol? FactoryMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether async methods should include a CancellationToken parameter.
        /// </summary>
        public bool GenerateCancellationTokens { get; set; } = true;

        // ADDED: List of containing types, outermost first.
        /// <summary>
        /// Gets the list of containing types (if the target type is nested), outermost first.
        /// </summary>
        public List<INamedTypeSymbol> ContainingTypes { get; } = new();
    }
}