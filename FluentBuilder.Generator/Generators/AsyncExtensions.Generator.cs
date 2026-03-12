// <copyright file="AsyncExtensions.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class AsyncExtensionsGenerator
    {
        public static void GenerateAsyncExtensions(
            StringBuilder sb, string indent, string builderName,
            INamedTypeSymbol typeSymbol, string asyncBuildMethodName,
            string asyncValidationPrefix, bool asyncValidationEnabled,
            bool includeCancellationToken)
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}// Extension methods for async fluent chaining");
            sb.AppendLine($"{indent}public static class {builderName}Extensions");
            sb.AppendLine($"{indent}{{");

            var asyncMethods = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => MethodCache.IsAsyncMethod(m) &&
                           TypeHelper.ShouldIncludeMethod(m));

            foreach (var method in asyncMethods)
            {
                string methodName = MethodCache.GetAsyncMethodName(method);
                string asyncMethodName = methodName + "Async";
                var parameters = BuildParameterList(method.Parameters);
                var args = BuildArgumentList(method.Parameters);
                var tokenParamExt = includeCancellationToken ? ", System.Threading.CancellationToken cancellationToken = default" : "";
                var tokenArgExt = includeCancellationToken ? ", cancellationToken" : "";
                bool methodHasToken = MethodCache.HasCancellationTokenParameter(method);
                var callTokenArg = (methodHasToken && includeCancellationToken) ? tokenArgExt : "";

                // If there are no original parameters and we have a token, we need to avoid a leading comma.
                string fullParamList = "";
                if (parameters.Length > 0)
                {
                    fullParamList = parameters + tokenParamExt;
                }
                else
                {
                    fullParamList = tokenParamExt.Length > 0 ? tokenParamExt.Substring(2) : ""; // remove leading ", "
                }

                sb.AppendLine($"{indent}    public static async System.Threading.Tasks.Task<{builderName}> {asyncMethodName}(");
                sb.AppendLine($"{indent}        this System.Threading.Tasks.Task<{builderName}> task{(fullParamList.Length > 0 ? $", {fullParamList}" : "")})");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        if (task is null) throw new System.ArgumentNullException(nameof(task));");
                sb.AppendLine($"{indent}        var builder = await task.ConfigureAwait(false);");
                sb.AppendLine($"{indent}        if (builder is null) throw new System.InvalidOperationException(\"Builder cannot be null\");");

                if (args.Length > 0)
                {
                    sb.AppendLine($"{indent}        return await builder.{asyncMethodName}({args}{callTokenArg}).ConfigureAwait(false);");
                }
                else
                {
                    sb.AppendLine($"{indent}        return await builder.{asyncMethodName}({callTokenArg}).ConfigureAwait(false);");
                }

                sb.AppendLine($"{indent}    }}");
                sb.AppendLine();
            }

            // Extension method for BuildAsync on Task<Builder>
            var tokenParamBuild = includeCancellationToken ? "System.Threading.CancellationToken cancellationToken = default" : "";
            var tokenArgBuild = includeCancellationToken ? "cancellationToken" : "";

            sb.AppendLine($"{indent}    public static async System.Threading.Tasks.Task<{typeSymbol.Name}> {asyncBuildMethodName}(");
            sb.AppendLine($"{indent}        this System.Threading.Tasks.Task<{builderName}> task{(!string.IsNullOrEmpty(tokenParamBuild) ? $", {tokenParamBuild}" : "")})");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        if (task is null) throw new System.ArgumentNullException(nameof(task));");
            sb.AppendLine($"{indent}        var builder = await task.ConfigureAwait(false);");
            sb.AppendLine($"{indent}        if (builder is null) throw new System.InvalidOperationException(\"Builder cannot be null\");");
            sb.AppendLine($"{indent}        return await builder.{asyncBuildMethodName}({tokenArgBuild}).ConfigureAwait(false);");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            if (asyncValidationEnabled)
            {
                foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
                {
                    if (AttributeCache.HasAttribute(member, Constant.AttributeName.FluentValidateAsync))
                    {
                        sb.AppendLine($"{indent}    public static async System.Threading.Tasks.Task<{builderName}> {asyncValidationPrefix}{member.Name}Async(");
                        sb.AppendLine($"{indent}        this System.Threading.Tasks.Task<{builderName}> task{(!string.IsNullOrEmpty(tokenParamBuild) ? $", {tokenParamBuild}" : "")})");
                        sb.AppendLine($"{indent}    {{");
                        sb.AppendLine($"{indent}        if (task is null) throw new System.ArgumentNullException(nameof(task));");
                        sb.AppendLine($"{indent}        var builder = await task.ConfigureAwait(false);");
                        sb.AppendLine($"{indent}        if (builder is null) throw new System.InvalidOperationException(\"Builder cannot be null\");");
                        sb.AppendLine($"{indent}        return await builder.{asyncValidationPrefix}{member.Name}Async({tokenArgBuild}).ConfigureAwait(false);");
                        sb.AppendLine($"{indent}    }}");
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine($"{indent}}}");
        }

        public static void GenerateAsyncFluentMethod(
            StringBuilder sb, string indent, string builderName,
            IMethodSymbol method, string asyncMethodName,
            bool includeCancellationToken)
        {
            var parameters = BuildParameterList(method.Parameters);
            var args = BuildArgumentList(method.Parameters);
            var returnType = StringCache.GetTypeDisplayName(method.ReturnType);

            var tokenParam = includeCancellationToken ? ", System.Threading.CancellationToken cancellationToken = default" : "";
            var tokenArg = includeCancellationToken ? ", cancellationToken" : "";
            bool methodHasToken = MethodCache.HasCancellationTokenParameter(method);
            var callTokenArg = (methodHasToken && includeCancellationToken) ? tokenArg : "";

            // If there are no original parameters and we have a token, avoid leading comma.
            string fullParamList = "";
            if (parameters.Length > 0)
            {
                fullParamList = parameters + tokenParam;
            }
            else
            {
                fullParamList = tokenParam.Length > 0 ? tokenParam.Substring(2) : "";
            }

            sb.AppendLine($"{indent}    public async System.Threading.Tasks.Task<{builderName}> {asyncMethodName}({fullParamList})");
            sb.AppendLine($"{indent}    {{");

            if (returnType == "System.Threading.Tasks.Task")
            {
                sb.AppendLine($"{indent}        await _instance.{method.Name}({args}{callTokenArg}).ConfigureAwait(false);");
            }
            else
            {
                sb.AppendLine($"{indent}        _ = await _instance.{method.Name}({args}{callTokenArg}).ConfigureAwait(false);");
            }

            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        private static string BuildParameterList(ImmutableArray<IParameterSymbol> parameters)
        {
            var list = new List<string>();
            foreach (var p in parameters)
            {
                if (p == null) continue;
                var typeName = StringCache.GetNullableTypeDisplayName(p.Type);
                var paramName = p.Name;
                if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(paramName))
                    continue;
                list.Add($"{typeName} {paramName}");
            }
            return string.Join(", ", list);
        }

        private static string BuildArgumentList(ImmutableArray<IParameterSymbol> parameters)
        {
            var list = new List<string>();
            foreach (var p in parameters)
            {
                if (p == null) continue;
                if (!string.IsNullOrEmpty(p.Name))
                    list.Add(p.Name);
            }
            return string.Join(", ", list);
        }
    }
}