// <copyright file="ClassBuilderGenerator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Generates the builder class for mutable (non‑record) types.</summary>

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Parameters;
using FluentBuilder.Generator.Validators;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class ClassBuilderGenerator
    {
        public static void Generate(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            Compilation compilation,
            HashSet<string> visitedBuilders,
            HashSet<string> fluentMethodNames,
            SourceProductionContext context,
            Dictionary<string, string> defaultValues,
            BuilderConfiguration config,
            AttributeValidator validator)
        {
            // Generate truth operators
            BaseBuilderGenerator.GenerateTruthOperators(sb, indent, builderName, typeSymbol);

            sb.AppendLine($"{indent}    private readonly {StringCache.GetTypeDisplayName(typeSymbol)} _instance;");
            sb.AppendLine($"{indent}    private readonly HashSet<string> _overridden;");
            sb.AppendLine();

            GenerateConstructors(sb, indent, builderName, typeSymbol, defaultValues, config);

            // Generate fluent methods for members
            BaseBuilderGenerator.GenerateFluentMethods(sb, indent, builderName, typeSymbol, compilation,
                visitedBuilders, fluentMethodNames, context, defaultValues, isRecord: false, config, validator);

            GenerateMethodFluentMethods(sb, indent, builderName, typeSymbol, fluentMethodNames, context, config);

            // Add virtual hook methods only if the builder is partial
            if (config.IsPartial)
            {
                GenerateVirtualHooks(sb, indent, typeSymbol);
            }
        }

        private static void GenerateVirtualHooks(StringBuilder sb, string indent, INamedTypeSymbol typeSymbol)
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}    /// <summary>");
            sb.AppendLine($"{indent}    /// Called just before the target instance is created.");
            sb.AppendLine($"{indent}    /// Override this method in a partial class to inject custom logic.");
            sb.AppendLine($"{indent}    /// </summary>");
            sb.AppendLine($"{indent}    protected virtual void OnBeforeBuild() {{ }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    /// <summary>");
            sb.AppendLine($"{indent}    /// Called immediately after the target instance is created.");
            sb.AppendLine($"{indent}    /// Override this method in a partial class to inject custom logic.");
            sb.AppendLine($"{indent}    /// </summary>");
            sb.AppendLine($"{indent}    /// <param name=\"instance\">The newly created instance.</param>");
            sb.AppendLine($"{indent}    protected virtual void OnAfterBuild({StringCache.GetTypeDisplayName(typeSymbol)} instance) {{ }}");
            sb.AppendLine();
        }

        private static void GenerateConstructors(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            Dictionary<string, string> defaultValues,
            BuilderConfiguration config)
        {
            var accessibleParameterlessCtor = typeSymbol.Constructors
                .FirstOrDefault(c => c.Parameters.Length == 0 && TypeHelper.ShouldIncludeConstructor(c));

            if (accessibleParameterlessCtor != null)
            {
                GenerateParameterlessConstructor(sb, indent, builderName, typeSymbol, defaultValues, config);
            }

            foreach (var ctor in typeSymbol.Constructors
                .Where(c => TypeHelper.ShouldIncludeConstructor(c) && c.Parameters.Length > 0))
            {
                GenerateParameterizedConstructor(sb, indent, builderName, typeSymbol, ctor, defaultValues);
            }
        }

        private static void GenerateParameterlessConstructor(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            Dictionary<string, string> defaultValues,
            BuilderConfiguration config)
        {
            sb.AppendLine($"{indent}    public {builderName}()");
            sb.AppendLine($"{indent}    {{");

            if (config.HasFactoryMethod)
            {
                sb.AppendLine($"{indent}        _instance = {StringCache.GetTypeDisplayName(typeSymbol)}.{config.FactoryMethodName}();");
            }
            else
            {
                sb.AppendLine($"{indent}        _instance = new {StringCache.GetTypeDisplayName(typeSymbol)}();");
            }

            sb.AppendLine($"{indent}        _overridden = new HashSet<string>();");
            if (defaultValues.Count > 0)
            {
                sb.AppendLine();
            }
            GenerateDefaultValueAssignments(sb, indent, typeSymbol, defaultValues, includeOverriddenCheck: true);

            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        private static void GenerateParameterizedConstructor(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            IMethodSymbol ctor,
            Dictionary<string, string> defaultValues)
        {
            var parameters = string.Join(", ", ctor.Parameters.Select(p => $"{StringCache.GetNullableTypeDisplayName(p.Type)} {p.Name}"));
            var args = string.Join(", ", ctor.Parameters.Select(p => p.Name));

            sb.AppendLine($"{indent}    public {builderName}({parameters})");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        _instance = new {StringCache.GetTypeDisplayName(typeSymbol)}({args});");
            sb.AppendLine($"{indent}        _overridden = new HashSet<string>();");
            sb.AppendLine();

            // Mark constructor parameters as overridden
            foreach (var param in ctor.Parameters)
            {
                var matchingMember = TypeHelper.GetBuildableMembers(typeSymbol)
                    .FirstOrDefault(m => m.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase));

                if (matchingMember != null)
                {
                    sb.AppendLine($"{indent}        _overridden.Add(\"{matchingMember.Name}\");");
                }
            }
            sb.AppendLine();

            GenerateDefaultValueAssignments(sb, indent, typeSymbol, defaultValues,
                includeOverriddenCheck: true,
                excludeMembers: ctor.Parameters.Select(p => p.Name));

            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        private static void GenerateDefaultValueAssignments(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            Dictionary<string, string> defaultValues,
            bool includeOverriddenCheck,
            IEnumerable<string>? excludeMembers = null)
        {
            excludeMembers ??= Enumerable.Empty<string>();

            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (excludeMembers.Contains(member.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (!defaultValues.TryGetValue(member.Name, out var defaultValue) || defaultValue == null)
                    continue;

                var memberType = TypeHelper.GetMemberType(member);
                if (TypeHelper.IsCollectionType(memberType))
                    continue;

                defaultValue = BaseBuilderGenerator.GetDefaultValueWithTypeConversion(defaultValue, memberType);

                if (includeOverriddenCheck)
                {
                    sb.AppendLine($"{indent}        if (!_overridden.Contains(\"{member.Name}\"))");
                    sb.AppendLine($"{indent}            _instance.{member.Name} = {defaultValue};");
                }
                else
                {
                    sb.AppendLine($"{indent}        _instance.{member.Name} = {defaultValue};");
                }
            }
        }

        private static void GenerateMethodFluentMethods(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            HashSet<string> fluentMethodNames,
            SourceProductionContext context,
            BuilderConfiguration config)
        {
            foreach (var method in typeSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => TypeHelper.ShouldIncludeMethod(m) && !MethodCache.IsAsyncMethod(m)))
            {
                string methodName = AttributeCache.GetConstructorArgument<string>(method, Constant.AttributeName.FluentName) ?? method.Name;

                // Check for conflict with built-in build methods
                if (methodName.Equals(config.BuildMethodName, StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor.FluentMethodNameConflict,
                        method.Locations.FirstOrDefault(),
                        methodName, "Build"));
                    continue;
                }
                if (config.HasAsyncSupport && methodName.Equals(config.AsyncBuildMethodName, StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor.FluentMethodNameConflict,
                        method.Locations.FirstOrDefault(),
                        methodName, "BuildAsync"));
                    continue;
                }

                // Generate signature including parameter types
                string signature = StringCache.GetMethodSignature(method, methodName);

                GenerateMethodCall(sb, indent, builderName, method, methodName);
            }
        }

        private static void GenerateMethodCall(StringBuilder sb, string indent, string builderName, IMethodSymbol method, string methodName)
        {
            var parameters = string.Join(", ", method.Parameters.Select(p => $"{StringCache.GetNullableTypeDisplayName(p.Type)} {p.Name}"));
            var args = string.Join(", ", method.Parameters.Select(p => p.Name));

            sb.AppendLine($"{indent}    public {builderName} {methodName}({parameters})");
            sb.AppendLine($"{indent}    {{");
            if (method.ReturnsVoid)
                sb.AppendLine($"{indent}        _instance.{method.Name}({args});");
            else
                sb.AppendLine($"{indent}        _ = _instance.{method.Name}({args});");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }
    }
}
