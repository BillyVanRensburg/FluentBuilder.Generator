// <copyright file="RecordBuilder.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    internal static class RecordBuilderGenerator
    {
        public static void Generate(
            StringBuilder sb, string indent, string builderName,
            INamedTypeSymbol typeSymbol, Compilation compilation,
            HashSet<string> visitedBuilders, HashSet<string> fluentMethodNames,
            SourceProductionContext context, Dictionary<string, string> defaultValues,
            BuilderConfiguration config,
            AttributeValidator validator)
        {
            sb.AppendLine($"{indent}    private {StringCache.GetTypeDisplayName(typeSymbol)}? _instance;");

            // Generate truth operators
            BaseBuilderGenerator.GenerateTruthOperators(sb, indent, builderName, typeSymbol);

            GenerateBackingFields(sb, indent, typeSymbol, defaultValues);

            sb.AppendLine($"{indent}    private readonly HashSet<string> _overridden;");
            sb.AppendLine();

            GenerateConstructors(sb, indent, builderName, typeSymbol, defaultValues);

            // Generate fluent methods for members – pass validator
            BaseBuilderGenerator.GenerateFluentMethods(sb, indent, builderName, typeSymbol, compilation,
                visitedBuilders, fluentMethodNames, context, defaultValues, isRecord: true, config, validator);

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

        private static void GenerateBackingFields(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            Dictionary<string, string> defaultValues)
        {
            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (AttributeCache.HasAttribute(member, Constant.AttributeName.FluentIgnore))
                    continue;

                ITypeSymbol memberType = TypeHelper.GetMemberType(member);
                if (memberType == null) continue;

                var typeName = StringCache.GetNullableTypeDisplayName(memberType);
                var defaultValue = GetFieldDefaultValue(member, memberType, defaultValues);

                sb.AppendLine($"{indent}    private {typeName} _{member.Name}{defaultValue};");
            }
        }

        private static string GetFieldDefaultValue(ISymbol member, ITypeSymbol memberType, Dictionary<string, string> defaultValues)
        {
            if (defaultValues.TryGetValue(member.Name, out var defaultValue))
            {
                defaultValue = BaseBuilderGenerator.GetDefaultValueWithTypeConversion(defaultValue, memberType);
                return $" = {defaultValue}";
            }

            if (TypeHelper.IsCollectionType(memberType))
            {
                return $" = new {StringCache.GetNullableTypeDisplayName(memberType)}()";
            }

            return string.Empty;
        }

        private static void GenerateConstructors(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            Dictionary<string, string> defaultValues)
        {
            // Parameterless constructor
            sb.AppendLine($"{indent}    public {builderName}()");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        _overridden = new HashSet<string>();");
            // Only add a blank line if there are default value assignments
            if (defaultValues.Count > 0)
            {
                sb.AppendLine();
            }
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            // Primary constructor
            var primaryConstructor = typeSymbol.Constructors.FirstOrDefault(c =>
                TypeHelper.ShouldIncludeConstructor(c) && c.Parameters.Length > 0);

            if (primaryConstructor != null)
            {
                GeneratePrimaryConstructor(sb, indent, builderName, typeSymbol, primaryConstructor, defaultValues);
            }
        }

        private static void GeneratePrimaryConstructor(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            IMethodSymbol primaryConstructor,
            Dictionary<string, string> defaultValues)
        {
            var parameters = string.Join(", ", primaryConstructor.Parameters.Select(p => $"{StringCache.GetNullableTypeDisplayName(p.Type)} {p.Name}"));

            sb.AppendLine($"{indent}    public {builderName}({parameters})");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        _overridden = new HashSet<string>();");

            foreach (var param in primaryConstructor.Parameters)
            {
                sb.AppendLine($"{indent}        _{param.Name} = {param.Name};");
                sb.AppendLine($"{indent}        _overridden.Add(\"{param.Name}\");");
            }

            GenerateDefaultValueAssignments(sb, indent, typeSymbol, defaultValues,
                excludeMembers: primaryConstructor.Parameters.Select(p => p.Name));

            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        private static void GenerateDefaultValueAssignments(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            Dictionary<string, string> defaultValues,
            IEnumerable<string>? excludeMembers = null)
        {
            excludeMembers ??= Enumerable.Empty<string>();

            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (excludeMembers.Contains(member.Name, System.StringComparer.OrdinalIgnoreCase))
                    continue;

                if (!defaultValues.TryGetValue(member.Name, out var defaultValue) || defaultValue == null)
                    continue;

                var memberType = TypeHelper.GetMemberType(member);
                if (TypeHelper.IsCollectionType(memberType))
                    continue;

                defaultValue = BaseBuilderGenerator.GetDefaultValueWithTypeConversion(defaultValue, memberType);

                sb.AppendLine($"{indent}        if (!_overridden.Contains(\"{member.Name}\"))");
                sb.AppendLine($"{indent}            _{member.Name} = {defaultValue};");
            }
        }

        public static void GenerateBuildMethod(
            StringBuilder sb, string indent, INamedTypeSymbol typeSymbol,
            string buildMethodName, string builderName,
            Compilation compilation, SourceProductionContext context,
            bool isPartial)
        {
            // Check if the builder already has a parameterless method with this name (from a partial part)
            if (BaseBuilderGenerator.DoesBuilderMethodExist(compilation, typeSymbol, builderName, buildMethodName, context))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor.BuilderMethodAlreadyExists,
                    typeSymbol.Locations.FirstOrDefault(),
                    buildMethodName));
                return; // Skip generation
            }

            sb.AppendLine($"{indent}    public {StringCache.GetTypeDisplayName(typeSymbol)} {buildMethodName}()");
            sb.AppendLine($"{indent}    {{");

            if (isPartial)
            {
                sb.AppendLine($"{indent}        OnBeforeBuild();");
            }

            sb.AppendLine($"{indent}        if (_instance == null)");
            sb.AppendLine($"{indent}        {{");

            var primaryConstructor = typeSymbol.Constructors.FirstOrDefault(c =>
                TypeHelper.ShouldIncludeConstructor(c) && c.Parameters.Length > 0);

            if (primaryConstructor?.Parameters.Length > 0)
            {
                GenerateBuildWithConstructor(sb, indent, typeSymbol, primaryConstructor);
            }
            else
            {
                GenerateBuildWithObjectInitializer(sb, indent, typeSymbol);
            }

            sb.AppendLine($"{indent}        }}");

            // Run required members validation
            ValidationGenerator.GenerateRequiredMembersValidation(sb, indent, typeSymbol);

            // Run custom validations (synchronous)
            ValidationGenerator.GenerateCustomValidationCalls(sb, indent, typeSymbol, isAsync: false);

            // Only set _built if the field exists
            if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == Constant.AttributeName.FluentTruthOperator))
            {
                sb.AppendLine($"{indent}        _built = true;");
            }

            if (isPartial)
            {
                sb.AppendLine($"{indent}        var instance = _instance;");
                sb.AppendLine($"{indent}        OnAfterBuild(instance);");
                sb.AppendLine($"{indent}        return instance;");
            }
            else
            {
                sb.AppendLine($"{indent}        return _instance;");
            }

            sb.AppendLine($"{indent}    }}");
        }

        private static void GenerateBuildWithConstructor(
            StringBuilder sb, string indent, INamedTypeSymbol typeSymbol, IMethodSymbol primaryConstructor)
        {
            var primaryArgs = string.Join(", ", primaryConstructor.Parameters.Select(p => $"_{p.Name}"));
            sb.AppendLine($"{indent}            _instance = new {StringCache.GetTypeDisplayName(typeSymbol)}({primaryArgs})");
            sb.AppendLine($"{indent}            {{");

            GenerateObjectInitializerMembers(sb, indent, typeSymbol,
                excludeNames: primaryConstructor.Parameters.Select(p => p.Name));

            sb.AppendLine($"{indent}            }};");
        }

        private static void GenerateBuildWithObjectInitializer(
            StringBuilder sb, string indent, INamedTypeSymbol typeSymbol)
        {
            sb.AppendLine($"{indent}            _instance = new {StringCache.GetTypeDisplayName(typeSymbol)}()");
            sb.AppendLine($"{indent}            {{");

            GenerateObjectInitializerMembers(sb, indent, typeSymbol);

            sb.AppendLine($"{indent}            }};");
        }

        private static void GenerateObjectInitializerMembers(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            IEnumerable<string>? excludeNames = null)
        {
            excludeNames ??= Enumerable.Empty<string>();

            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (AttributeCache.HasAttribute(member, Constant.AttributeName.FluentIgnore))
                    continue;

                if (excludeNames.Contains(member.Name))
                    continue;

                sb.AppendLine($"{indent}                {member.Name} = _{member.Name},");
            }
        }
    }
}