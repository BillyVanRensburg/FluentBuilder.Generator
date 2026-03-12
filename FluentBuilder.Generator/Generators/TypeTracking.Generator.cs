// <copyright file="TypeTracking.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Managers;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace FluentBuilder.Generator.Generator
{
    /// <summary>
    /// Tracks all types, members, and attributes referenced during builder generation
    /// to ensure the required using directives are collected.
    /// </summary>
    internal static class TypeTrackingGenerator
    {
        /// <summary>
        /// Recursively tracks the given type symbol and all its relevant members and attributes.
        /// </summary>
        public static void TrackAllTypes(
            INamedTypeSymbol typeSymbol,
            Compilation compilation,
            UsingDirectiveManager usingManager)
        {
            // Track all buildable members
            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                // TrackMember now handles both the member's type and any attributes on it
                usingManager.TrackMember(member);

                // Track collection-specific options (e.g., AddRange, Count validation)
                var options = AttributeCache.GetCollectionOptions(member);
                usingManager.TrackCollectionHelpers(member, options);

                // Track nested builders (types that themselves have a builder)
                var memberType = TypeHelper.GetMemberType(member);
                if (memberType != null && TypeHelper.HasBuilder(memberType, compilation))
                {
                    usingManager.TrackType(memberType);
                }
            }

            // Track methods that should be included (e.g., factory methods, validation methods)
            foreach (var method in typeSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => TypeHelper.ShouldIncludeMethod(m)))
            {
                usingManager.TrackMethod(method);
            }

            // Track async-specific features (e.g., Task, CancellationToken)
            usingManager.TrackAsyncFeatures(typeSymbol);

            // Track attributes applied directly to the type
            foreach (var attr in typeSymbol.GetAttributes())
            {
                usingManager.TrackAttribute(attr);
            }

            // ADDED: Track containing types for using directives
            var current = typeSymbol.ContainingType;
            while (current != null)
            {
                usingManager.TrackType(current);
                current = current.ContainingType;
            }
        }
    }
}