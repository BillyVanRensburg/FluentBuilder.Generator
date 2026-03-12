// <copyright file="DiagnosticReporter.Base.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FluentBuilder.Generator.Base
{
    /// <summary>
    /// Base class that provides diagnostic reporting and error tracking for components
    /// that need to report diagnostics during source generation.
    /// </summary>
    public abstract class DiagnosticReporterBase
    {
        private readonly List<Diagnostic> _diagnostics = new();

        /// <summary>
        /// The source production context used to report diagnostics.
        /// </summary>
        protected readonly SourceProductionContext Context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticReporterBase"/> class.
        /// </summary>
        /// <param name="context">The source production context used to report diagnostics.</param>
        protected DiagnosticReporterBase(SourceProductionContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Reports an error diagnostic.
        /// </summary>
        /// <param name="descriptor">The diagnostic descriptor.</param>
        /// <param name="location">The location in source code where the error occurred.</param>
        /// <param name="args">Optional arguments to format the diagnostic message.</param>
        protected void ReportError(DiagnosticDescriptor descriptor, Location location, params object[] args)
        {
            var diagnostic = Diagnostic.Create(descriptor, location, args);
            Context.ReportDiagnostic(diagnostic);
            _diagnostics.Add(diagnostic);
        }

        /// <summary>
        /// Reports a warning diagnostic.
        /// </summary>
        /// <param name="descriptor">The diagnostic descriptor.</param>
        /// <param name="location">The location in source code where the warning occurred.</param>
        /// <param name="args">Optional arguments to format the diagnostic message.</param>
        protected void ReportWarning(DiagnosticDescriptor descriptor, Location location, params object[] args)
        {
            var diagnostic = Diagnostic.Create(descriptor, location, args);
            Context.ReportDiagnostic(diagnostic);
            _diagnostics.Add(diagnostic);
        }

        /// <summary>
        /// Gets a safe location from a symbol, falling back to Location.None.
        /// </summary>
        protected Location GetLocation(ISymbol symbol) => symbol?.Locations.FirstOrDefault() ?? Location.None;

        /// <inheritdoc />
        public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    }
}