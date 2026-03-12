// <copyright file="LogLevel.Enum.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Defines the log severity levels used throughout the generator.</summary>

namespace FluentBuilder
{
    /// <summary>
    /// Specifies the severity level of log messages emitted by the builder generator.
    /// </summary>
    internal enum LogLevel
    {
        /// <summary>
        /// Debug-level messages with the most detailed information. These are typically
        /// used for diagnosing problems and are not expected to be shown in production.
        /// </summary>
        Debug,

        /// <summary>
        /// Informational messages that highlight the progress of the generator at a coarse-grained level.
        /// </summary>
        Info,

        /// <summary>
        /// Warning messages that indicate a potential problem or an unexpected condition
        /// that does not prevent the generator from completing its task.
        /// </summary>
        Warning,

        /// <summary>
        /// Error messages that indicate a failure in the current operation. The generator
        /// may still continue, but the result may be incomplete or incorrect.
        /// </summary>
        Error
    }
}