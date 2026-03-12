// <copyright file="GeneratorLogger.Service.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Logger for the source generator with diagnostic and debug output support.</summary>

using FluentBuilder.Generator.Configuration;
using FluentBuilder.Generator.Generator;
using FluentBuilder.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;

namespace FluentBuilder.Generator.Services
{
    /// <summary>
    /// Logger for the generator with diagnostic support. Logs are only emitted when logging is enabled in the project file.
    /// All public methods are thread-safe, delegating to underlying thread-safe components.
    /// </summary>
    internal static partial class GeneratorLogger
    {
        /// <summary>
        /// Initializes the logger with the global configuration.
        /// This method delegates to <see cref="InitializeOnce"/> to ensure one-time setup.
        /// </summary>
        /// <param name="config">The global generator configuration. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public static void Initialize(GeneratorConfiguration config) => InitializeOnce(config);

        /// <summary>
        /// Initializes the logger once per generation run with the given configuration.
        /// This method is thread-safe and does nothing if already initialized.
        /// </summary>
        /// <param name="config">The global generator configuration. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public static void InitializeOnce(GeneratorConfiguration config)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            LoggerCore.InitializeOnce(config);
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled (based on configuration).
        /// </summary>
        public static bool IsLoggingEnabled => LoggerCore.IsLoggingEnabled;

        /// <summary>
        /// Logs an error diagnostic (always emitted).
        /// </summary>
        /// <param name="context">Source production context.</param>
        /// <param name="descriptor">Diagnostic descriptor.</param>
        /// <param name="location">Optional source location.</param>
        /// <param name="messageArgs">Message arguments for the diagnostic.</param>
        public static void LogError(SourceProductionContext context, DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            DiagnosticHelper.Report(context, descriptor, location, messageArgs);
            DebugLogger.LogDebug(LogLevel.Error, descriptor, messageArgs);
        }

        /// <summary>
        /// Logs an error to debug output only (no compiler diagnostic) when a <see cref="SourceProductionContext"/> is not available,
        /// such as during the transform phase.
        /// </summary>
        /// <param name="compilation">Compilation (unused, present for API symmetry).</param>
        /// <param name="descriptor">Diagnostic descriptor for message formatting.</param>
        /// <param name="location">Optional source location.</param>
        /// <param name="messageArgs">Message arguments.</param>
        public static void LogError(Compilation compilation, DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            string locationStr = FormatLocation(location);
            DebugLogger.LogDebug(LogLevel.Error, descriptor, messageArgs, locationStr);
        }

        /// <summary>
        /// Logs a warning diagnostic (always emitted).
        /// </summary>
        /// <param name="context">Source production context.</param>
        /// <param name="descriptor">Diagnostic descriptor.</param>
        /// <param name="location">Optional source location.</param>
        /// <param name="messageArgs">Message arguments for the diagnostic.</param>
        public static void LogWarning(SourceProductionContext context, DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            DiagnosticHelper.Report(context, descriptor, location, messageArgs);
            DebugLogger.LogDebug(LogLevel.Warning, descriptor, messageArgs);
        }

        /// <summary>
        /// Logs a warning to debug output only (no compiler diagnostic) when a <see cref="SourceProductionContext"/> is not available,
        /// such as during the transform phase.
        /// </summary>
        /// <param name="compilation">Compilation (unused, present for API symmetry).</param>
        /// <param name="descriptor">Diagnostic descriptor for message formatting.</param>
        /// <param name="location">Optional source location.</param>
        /// <param name="messageArgs">Message arguments.</param>
        public static void LogWarning(Compilation compilation, DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            string locationStr = FormatLocation(location);
            DebugLogger.LogDebug(LogLevel.Warning, descriptor, messageArgs, locationStr);
        }

        /// <summary>
        /// Logs an informational diagnostic (only emitted if logging is enabled).
        /// </summary>
        /// <param name="context">Source production context.</param>
        /// <param name="descriptor">Diagnostic descriptor.</param>
        /// <param name="location">Optional source location.</param>
        /// <param name="messageArgs">Message arguments for the diagnostic.</param>
        public static void LogInfo(SourceProductionContext context, DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        {
            if (IsLoggingEnabled)
            {
                DiagnosticHelper.Report(context, descriptor, location, messageArgs);
            }
            DebugLogger.LogDebug(LogLevel.Info, descriptor, messageArgs);
        }

        /// <summary>
        /// Logs a debug message (only in DEBUG builds and if logging is enabled).
        /// </summary>
        /// <param name="message">Debug message.</param>
        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            if (IsLoggingEnabled)
            {
                LoggerCore.WriteToDebug($"{LoggerCore.LogPrefix} DEBUG - {message}");
            }
        }

        /// <summary>
        /// Logs an exception with debug details. This method forwards to <see cref="ExceptionLogger"/>.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="context">Optional context string.</param>
        [Conditional("DEBUG")]
        public static void LogException(Exception ex, string context = "")
        {
            ExceptionLogger.LogException(ex, context);
        }

        /// <summary>
        /// Formats a source location for debug output.
        /// </summary>
        /// <param name="location">Source location.</param>
        /// <returns>A human-readable location string.</returns>
        private static string FormatLocation(Location? location)
        {
            if (location == null)
                return "unknown location";

            var lineSpan = location.GetLineSpan();
            var start = lineSpan.StartLinePosition;
            return $"at line {start.Line}, character {start.Character}";
        }
    }
}