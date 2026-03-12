// <copyright file="DefaultValues.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FluentBuilder.Generator.Generator
{
    internal static class DefaultValuesGenerator
    {
        internal static Dictionary<string, string> CollectDefaultValues(
            INamedTypeSymbol typeSymbol,
            SourceProductionContext context)
        {
            var defaultValues = new Dictionary<string, string>();

            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (!AttributeCache.HasAttribute(member, Constant.AttributeName.FluentDefaultValue))
                    continue;

                var defaultValueAttr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == Constant.AttributeName.FluentDefaultValue);

                if (defaultValueAttr == null)
                    continue;

                var memberType = TypeHelper.GetMemberType(member);
                if (memberType == null)
                    continue;

                var result = DefaultValueCache.GetDefaultValue(member, defaultValueAttr, memberType);

                if (result.IsValid && !string.IsNullOrEmpty(result.FormattedValue))
                {
                    defaultValues[member.Name] = result.FormattedValue;
                }
                else if (result.Error != null)
                {
                    context.ReportDiagnostic(result.Error);
                }
            }

            return defaultValues;
        }
    }
}