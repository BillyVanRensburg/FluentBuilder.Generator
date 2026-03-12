// <copyright file="LoggerCore.Services.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Provides thread-safe logging for the FluentBuilder source generator.</summary>

using System;
using System.Diagnostics;
using System.Threading;
using FluentBuilder.Generator.Configuration;

namespace FluentBuilder.Generator.Services
{
    /// <summary>
    /// Provides a centralized, thread-safe logging mechanism for the source generator.
    /// Logging can be enabled or disabled via the <see cref="GeneratorConfiguration"/>.
    /// </summary>
    internal static class LoggerCore
    {
        private static GeneratorConfiguration? _config;
        private static readonly object _syncLock = new();
        private static int _initialized;

        /// <summary>
        /// Prefix added to all log messages for easy identification in the output.
        /// </summary>
        internal const string LogPrefix = "[FluentBuilder Generator]";

        /// <summary>
        /// Gets a value indicating whether logging is currently enabled.
        /// Returns <c>false</c> if the logger has not been initialized.
        /// </summary>
        internal static bool IsLoggingEnabled
        {
            get
            {
                // Use Volatile.Read to ensure we get the latest value of _config.
                var config = Volatile.Read(ref _config);
                return config?.EnableLogging == true;
            }
        }

        /// <summary>
        /// Initializes the logger with the specified configuration.
        /// This method must be called exactly once; subsequent calls are ignored
        /// unless the configuration is different (in which case the new configuration
        /// is ignored and a warning is emitted).
        /// </summary>
        /// <param name="config">The generator configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        internal static void InitializeOnce(GeneratorConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // Attempt to set the configuration if not already initialized.
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
            {
                Volatile.Write(ref _config, config);
            }
            else if (!config.Equals(Volatile.Read(ref _config)))
            {
                // UPDATED: Use Equals for value equality.
                // Optional warning: a different configuration is being ignored.
                // Use Debug.WriteLine directly because logging may not be enabled yet.
                Debug.WriteLine($"{LogPrefix} Warning: Ignoring attempt to re-initialize with a different configuration.");
            }
        }

        /// <summary>
        /// Writes a message to the debug output if logging is enabled.
        /// </summary>
        /// <param name="message">The message to log.</param>
        internal static void Log(string message)
        {
            if (!IsLoggingEnabled) return;
            WriteToDebug(message);
        }

        /// <summary>
        /// Writes an error message to the trace output if logging is enabled.
        /// The message is prefixed with "ERROR" for easy filtering.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        internal static void LogError(string message)
        {
            if (!IsLoggingEnabled) return;
            WriteToTrace("ERROR", message);
        }

        /// <summary>
        /// Writes an error message with exception details to the trace output if logging is enabled.
        /// The message is prefixed with "ERROR" and includes the exception type and stack trace.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">The exception to include.</param>
        internal static void LogError(string message, Exception exception)
        {
            if (!IsLoggingEnabled) return;
            WriteToTrace("ERROR", $"{message} {exception.GetType()}: {exception.Message}\n{exception.StackTrace}");
        }

        /// <summary>
        /// Writes a warning message to the trace output if logging is enabled.
        /// The message is prefixed with "WARN" for easy filtering.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        internal static void LogWarning(string message)
        {
            if (!IsLoggingEnabled) return;
            WriteToTrace("WARN", message);
        }

        /// <summary>
        /// Writes an informational message to the trace output if logging is enabled.
        /// The message is prefixed with "INFO" for easy filtering.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        internal static void LogInfo(string message)
        {
            if (!IsLoggingEnabled) return;
            WriteToTrace("INFO", message);
        }

        /// <summary>
        /// Writes a message to the debug output unconditionally, without the logging-enabled check.
        /// Useful for low-level tracing or errors that should always appear.
        /// </summary>
        /// <param name="message">The message to write.</param>
        internal static void WriteToDebug(string message)
        {
            lock (_syncLock)
            {
                Debug.WriteLine($"{LogPrefix} {message}");
            }
        }

        /// <summary>
        /// Writes a message to the trace output unconditionally, without the logging-enabled check.
        /// Useful for always-on diagnostic output that appears even without a debugger attached.
        /// </summary>
        /// <param name="message">The message to write.</param>
        internal static void WriteToTrace(string message)
        {
            lock (_syncLock)
            {
                Trace.WriteLine($"{LogPrefix} {message}");
            }
        }

        /// <summary>
        /// Core method that writes a formatted message to the trace output with a severity prefix.
        /// </summary>
        /// <param name="severity">The severity prefix (e.g., ERROR, WARN).</param>
        /// <param name="message">The message content.</param>
        private static void WriteToTrace(string severity, string message)
        {
            lock (_syncLock)
            {
                Trace.WriteLine($"{LogPrefix} {severity}: {message}");
            }
        }
    }
}