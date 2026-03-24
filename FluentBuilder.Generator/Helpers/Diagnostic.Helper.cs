// <copyright file="Diagnostic.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace FluentBuilder.Generator.Helpers
{
    /// <summary>
    /// Helper class for creating diagnostics with consistent formatting across the generator.
    /// </summary>
    internal static class DiagnosticHelper
    {
        /// <summary>
        /// Creates a diagnostic with the specified descriptor, location, and message arguments.
        /// </summary>
        /// <param name="descriptor">The diagnostic descriptor.</param>
        /// <param name="location">The location where the diagnostic occurs (may be null).</param>
        /// <param name="messageArgs">The message arguments.</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic Create(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            return Diagnostic.Create(descriptor, location ?? Location.None, messageArgs);
        }

        /// <summary>
        /// Creates a diagnostic and reports it to the source production context.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="descriptor">The diagnostic descriptor.</param>
        /// <param name="location">The location where the diagnostic occurs (may be null).</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void Report(SourceProductionContext context, DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            context.ReportDiagnostic(Create(descriptor, location, messageArgs));
        }

        /// <summary>
        /// Creates an error diagnostic for a transformation failure.
        /// </summary>
        /// <param name="typeName">The name of the type being transformed.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="location">The location where the error occurred (may be null).</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic CreateTransformationError(string typeName, string errorMessage, Location? location = null)
        {
            return Create(Descriptor.TransformationError, location, typeName, errorMessage);
        }

        /// <summary>
        /// Reports a transformation error diagnostic.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="typeName">The name of the type being transformed.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="location">The location where the error occurred (may be null).</param>
        public static void ReportTransformationError(SourceProductionContext context, string typeName, string errorMessage, Location? location = null)
        {
            Report(context, Descriptor.TransformationError, location, typeName, errorMessage);
        }

        /// <summary>
        /// Creates an error diagnostic for a cyclic dependency.
        /// </summary>
        /// <param name="cyclePath">The path of the cyclic dependency.</param>
        /// <param name="location">The location where the cycle was detected (may be null).</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic CreateCyclicDependencyError(string cyclePath, Location? location = null)
        {
            return Create(Descriptor.CyclicDependencyDetected, location, cyclePath);
        }

        /// <summary>
        /// Reports a cyclic dependency error diagnostic.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="cyclePath">The path of the cyclic dependency.</param>
        /// <param name="location">The location where the cycle was detected (may be null).</param>
        public static void ReportCyclicDependencyError(SourceProductionContext context, string cyclePath, Location? location = null)
        {
            Report(context, Descriptor.CyclicDependencyDetected, location, cyclePath);
        }

        /// <summary>
        /// Creates a warning for FluentName attributes without FluentBuilder.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="location">The location of the type (may be null).</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic CreateFluentNameWithoutBuilderWarning(string typeName, Location? location = null)
        {
            return Create(Descriptor.FluentNameWithoutBuilder, location, typeName);
        }

        /// <summary>
        /// Reports a warning for FluentName attributes without FluentBuilder.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="location">The location of the type (may be null).</param>
        public static void ReportFluentNameWithoutBuilderWarning(SourceProductionContext context, string typeName, Location? location = null)
        {
            Report(context, Descriptor.FluentNameWithoutBuilder, location, typeName);
        }

        /// <summary>
        /// Creates an information diagnostic about generator status.
        /// </summary>
        /// <param name="message">The information message.</param>
        /// <param name="location">The location for the diagnostic (may be null).</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic CreateGeneratorInfo(string message, Location? location = null)
        {
            return Create(Descriptor.GeneratorInformation, location, message);
        }

        /// <summary>
        /// Reports an information diagnostic about generator status.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="message">The information message.</param>
        /// <param name="location">The location for the diagnostic (may be null).</param>
        public static void ReportGeneratorInfo(SourceProductionContext context, string message, Location? location = null)
        {
            Report(context, Descriptor.GeneratorInformation, location, message);
        }

        /// <summary>
        /// Creates a warning diagnostic from the generator.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="location">The location for the diagnostic (may be null).</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic CreateGeneratorWarning(string message, Location? location = null)
        {
            return Create(Descriptor.GeneratorWarning, location, message);
        }

        /// <summary>
        /// Reports a warning diagnostic from the generator.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="message">The warning message.</param>
        /// <param name="location">The location for the diagnostic (may be null).</param>
        public static void ReportGeneratorWarning(SourceProductionContext context, string message, Location? location = null)
        {
            Report(context, Descriptor.GeneratorWarning, location, message);
        }

        /// <summary>
        /// Creates a diagnostic for a cancelled generation.
        /// </summary>
        /// <param name="typeName">The name of the type being generated.</param>
        /// <param name="location">The location of the type (may be null).</param>
        /// <returns>A diagnostic instance.</returns>
        public static Diagnostic CreateGenerationCancelledInfo(string typeName, Location? location = null)
        {
            return Create(Descriptor.GenerationCancelled, location, typeName);
        }

        /// <summary>
        /// Reports a diagnostic for a cancelled generation.
        /// </summary>
        /// <param name="context">The source production context.</param>
        /// <param name="typeName">The name of the type being generated.</param>
        /// <param name="location">The location of the type (may be null).</param>
        public static void ReportGenerationCancelledInfo(SourceProductionContext context, string typeName, Location? location = null)
        {
            Report(context, Descriptor.GenerationCancelled, location, typeName);
        }
    }
}
