// <copyright file="Generation.Context.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Managers;
using FluentBuilder.Generator.Parameters;
using FluentBuilder.Generator.Validators;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace FluentBuilder.Generator.Context
{
    /// <summary>
    /// Holds all contextual data and services needed during a single builder generation pass.
    /// Instances are immutable and thread-safe after construction.
    /// </summary>
    /// <remarks>
    /// This context is created for each type that requires a builder and is passed through
    /// the generation pipeline. It centralizes access to the Roslyn compilation, the target
    /// type symbol, user configuration, and various managers and validators.
    /// </remarks>
    internal sealed class GenerationContext
    {
        /// <summary>
        /// Gets the <see cref="SourceProductionContext"/> provided by the Roslyn source generator.
        /// Used to report diagnostics and add generated sources.
        /// </summary>
        public SourceProductionContext SourceContext { get; }

        /// <summary>
        /// Gets the current <see cref="Compilation"/> that contains the type being processed.
        /// Provides access to the entire compilation unit, including metadata references.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets a set of builder type names that have already been visited during the current
        /// generation pass. Used to detect and prevent cyclic dependencies.
        /// </summary>
        public HashSet<string> VisitedBuilders { get; }

        /// <summary>
        /// Gets the <see cref="BuilderValidator"/> instance responsible for validating the
        /// target type and its members before generation.
        /// </summary>
        public BuilderValidator Validator { get; }

        /// <summary>
        /// Gets the <see cref="INamedTypeSymbol"/> representing the type for which a builder
        /// is being generated.
        /// </summary>
        public INamedTypeSymbol TypeSymbol { get; }

        /// <summary>
        /// Gets the <see cref="BuilderConfiguration"/> that holds user‑supplied options
        /// (e.g., namespace, class name, accessibility) derived from attributes and defaults.
        /// </summary>
        public BuilderConfiguration Config { get; }

        /// <summary>
        /// Gets the <see cref="UsingDirectiveManager"/> that collects and manages required
        /// using directives for the generated builder source code.
        /// </summary>
        public UsingDirectiveManager UsingManager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationContext"/> class.
        /// </summary>
        /// <param name="sourceContext">The source production context from Roslyn.</param>
        /// <param name="compilation">The current compilation.</param>
        /// <param name="visitedBuilders">A set of already‑visited builder names for cycle detection.</param>
        /// <param name="validator">The validator to use for pre‑generation checks.</param>
        /// <param name="typeSymbol">The symbol of the type being processed.</param>
        /// <param name="config">The resolved builder configuration.</param>
        /// <param name="usingManager">The manager for collecting using directives.</param>
        /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
        public GenerationContext(
            SourceProductionContext sourceContext,
            Compilation compilation,
            HashSet<string> visitedBuilders,
            BuilderValidator validator,
            INamedTypeSymbol typeSymbol,
            BuilderConfiguration config,
            UsingDirectiveManager usingManager)
        {
            SourceContext = sourceContext;
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            VisitedBuilders = visitedBuilders ?? throw new ArgumentNullException(nameof(visitedBuilders));
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            UsingManager = usingManager ?? throw new ArgumentNullException(nameof(usingManager));
        }
    }
}