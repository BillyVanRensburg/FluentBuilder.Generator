// <copyright file="ValidateFluentNameAttributes.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Validators;
using Microsoft.CodeAnalysis;

namespace FluentBuilder.Generator.Helpers
{
    /// <summary>
    /// Provides helper methods for validating FluentName attributes on types and members.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Validates FluentName attributes on the given type and its buildable members.
        /// </summary>
        /// <param name="context">The source production context for reporting diagnostics.</param>
        /// <param name="typeSymbol">The type symbol to validate.</param>
        internal static void ValidateFluentNameAttributes(
            SourceProductionContext context,
            INamedTypeSymbol typeSymbol)
        {
            var validator = new FluentNameValidator(context);

            // Validate the type itself for FluentName
            validator.ValidateType(typeSymbol);

            // Validate all buildable members for FluentName
            var buildableMembers = TypeHelper.GetBuildableMembers(typeSymbol);
            validator.ValidateMembers(buildableMembers);

            // Validate all methods for FluentName
            var methods = TypeHelper.GetIncludableMethods(typeSymbol);
            validator.ValidateMembers(methods);
        }
    }
}