// <copyright file="GeneratorVersion.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Provides version information for the FluentBuilder source generator.</summary>

using System;
using System.Reflection;

namespace FluentBuilder.Generator
{
    /// <summary>
    /// Provides version information about the FluentBuilder source generator assembly.
    /// This information is used internally for logging, diagnostics, and debugging.
    /// </summary>
    public static class GeneratorInfo
    {
        /// <summary>
        /// Gets the version of the generator assembly.
        /// </summary>
        /// <value>
        /// A <see cref="Version"/> object representing the assembly version, 
        /// typically derived from the <c>AssemblyVersion</c> attribute or the 
        /// <c>Version</c> property in the project file.
        /// </value>
        /// <remarks>
        /// This property reads the version from the executing assembly's metadata.
        /// It is evaluated once when the class is first accessed and then cached.
        /// </remarks>
        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version
            ?? new Version(1, 0, 0, 0); // Fallback in case version is null (should not happen)

        /// <summary>
        /// Returns a formatted string containing the generator name and version.
        /// </summary>
        /// <returns>
        /// A string in the format "FluentBuilder.Generator v{Version}", 
        /// for example "FluentBuilder.Generator v1.2.3.4".
        /// </returns>
        /// <remarks>
        /// This method is intended for use in log messages and diagnostic output.
        /// It uses the <see cref="Version"/> property to obtain the current assembly version.
        /// </remarks>
        public static string GetInfo() => $"{nameof(FluentBuilder.Generator)} v{Version}";
    }
}