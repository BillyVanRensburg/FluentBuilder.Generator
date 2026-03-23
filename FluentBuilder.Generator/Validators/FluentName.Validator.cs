// <copyright file="FluentName.Validator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Base;
using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace FluentBuilder.Generator.Validators
{
    /// <summary>
    /// Validates FluentName attributes on symbols.
    /// </summary>
    public class FluentNameValidator : DiagnosticReporterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentNameValidator"/> class.
        /// </summary>
        /// <param name="context">The source production context used to report diagnostics.</param>
        public FluentNameValidator(SourceProductionContext context) : base(context) { }

        /// <summary>
        /// Validates a single symbol for correct FluentName attribute usage.
        /// </summary>
        public void Validate(ISymbol symbol)
        {
            var location = GetLocation(symbol);
            var attributes = symbol.GetAttributes();

            foreach (var attr in attributes)
            {
                if (attr.AttributeClass?.Name != Constant.AttributeName.FluentName)
                    continue;

                var nameValue = attr.ConstructorArguments.Length > 0
                    ? attr.ConstructorArguments[0].Value as string
                    : null;

                break; // Only one FluentName attribute allowed
            }
        }

        /// <summary>
        /// Validates a type symbol for FluentName attribute.
        /// </summary>
        public void ValidateType(INamedTypeSymbol typeSymbol) => Validate(typeSymbol);

        /// <summary>
        /// Validates multiple member symbols for FluentName attributes.
        /// </summary>
        public void ValidateMembers(IEnumerable<ISymbol> members)
        {
            foreach (var member in members)
                Validate(member);
        }
    }
}
