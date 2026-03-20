// <copyright file="AsyncMethods.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Parameters;
using FluentBuilder.Generator.Validators;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class AsyncMethodsGenerator
    {
        public static void GenerateAsyncFluentMethods(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            HashSet<string> fluentMethodNames,
            SourceProductionContext context,
            AttributeValidator validator,
            BuilderConfiguration config)
        {
            foreach (var method in typeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind != MethodKind.Ordinary) continue;
                if (!TypeHelper.ShouldIncludeMethod(method)) continue;
                if (!MethodCache.IsAsyncMethod(method)) continue;

                if (!validator.ValidateAsyncMethod(method))
                    continue;

                string methodName = MethodCache.GetAsyncMethodName(method);
                string asyncMethodName = methodName + "Async";

                // Generate signature including parameter types
                string signature = StringCache.GetMethodSignature(method, asyncMethodName);
                fluentMethodNames.Add(signature);

                AsyncExtensionsGenerator.GenerateAsyncFluentMethod(
                    sb, indent, builderName, method, asyncMethodName,
                    config.GenerateCancellationTokens);
            }
        }

        public static void GenerateAsyncValidationMethods(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            string asyncValidationPrefix,
            SourceProductionContext context,
            bool isRecord,
            bool includeCancellationToken)
        {
            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (AttributeCache.HasAttribute(member, Constant.AttributeName.FluentValidateAsync))
                {
                    ValidationGenerator.GenerateAsyncValidationMethod(
                        sb, indent, builderName, member,
                        member.Name, asyncValidationPrefix, context, isRecord,
                        includeCancellationToken);
                }
            }
        }

        public static void GenerateAsyncBuildMethod(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            string buildMethodName,
            string asyncBuildMethodName,
            string asyncValidationPrefix,
            bool asyncValidationEnabled,
            bool includeCancellationToken)
        {
            var tokenParam = includeCancellationToken ? "System.Threading.CancellationToken cancellationToken = default" : "";
            var tokenArg = includeCancellationToken ? "cancellationToken" : "";

            sb.AppendLine();
            sb.AppendLine($"{indent}    public async System.Threading.Tasks.Task<{typeSymbol.Name}> {asyncBuildMethodName}({tokenParam})");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var instance = {buildMethodName}();");

            var asyncMethods = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => MethodCache.IsAsyncMethod(m) &&
                           TypeHelper.ShouldIncludeMethod(m) &&
                           m.Parameters.Length == 0);

            foreach (var method in asyncMethods)
            {
                bool methodHasToken = MethodCache.HasCancellationTokenParameter(method);
                var callTokenArg = (methodHasToken && includeCancellationToken) ? tokenArg : "";
                sb.AppendLine($"{indent}        await instance.{method.Name}({callTokenArg}).ConfigureAwait(false);");
            }

            if (asyncValidationEnabled)
            {
                foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
                {
                    if (AttributeCache.HasAttribute(member, Constant.AttributeName.FluentValidateAsync))
                    {
                        sb.AppendLine($"{indent}        await {asyncValidationPrefix}{member.Name}Async({tokenArg}).ConfigureAwait(false);");
                    }
                }
            }

            // Run custom validation methods (both sync and async)
            ValidationGenerator.GenerateCustomValidationCalls(sb, indent, typeSymbol, isAsync: true);

            sb.AppendLine($"{indent}        return instance;");
            sb.AppendLine($"{indent}    }}");
        }
    }
}
