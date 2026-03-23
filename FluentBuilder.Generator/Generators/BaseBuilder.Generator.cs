// <copyright file="BaseBuilder.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    internal static class BaseBuilderGenerator
    {
        public static void GenerateTruthOperators(StringBuilder sb, string indent, string builderName, INamedTypeSymbol typeSymbol)
        {
            if (!typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == Constant.AttributeName.FluentTruthOperator))
                return;

            sb.AppendLine($"{indent}    private bool _built;");
            sb.AppendLine($"{indent}    public static bool operator true({builderName} b) => b?._built == true;");
            sb.AppendLine($"{indent}    public static bool operator false({builderName} b) => b?._built != true;");
            sb.AppendLine($"{indent}    public static bool operator !({builderName} b) => b?._built != true;");
            sb.AppendLine();
        }

        public static void GenerateFluentMethods(
            StringBuilder sb,
            string indent,
            string builderName,
            INamedTypeSymbol typeSymbol,
            Compilation compilation,
            HashSet<string> visitedBuilders,
            HashSet<string> fluentSignatures,
            SourceProductionContext context,
            Dictionary<string, string> defaultValues,
            bool isRecord,
            BuilderConfiguration config,
            AttributeValidator validator)
        {
            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (AttributeCache.HasAttribute(member, Constant.AttributeName.FluentIgnore))
                    continue;

                // Determine the fluent method name
                string baseName;
                string explicitName = AttributeCache.GetConstructorArgument<string>(member, Constant.AttributeName.FluentName);
                if (explicitName != null)
                {
                    baseName = explicitName;
                }
                else
                {
                    baseName = member.Name;
                    // Apply prefix and suffix
                    baseName = config.MethodPrefix + baseName + config.MethodSuffix;
                }

                // Validate the name
                if (!TryValidateMemberName(member, baseName, context, out var memberType))
                    continue;

                // Get the field reference based on builder type
                string fieldReference = isRecord ? $"_{member.Name}" : $"_instance.{member.Name}";

                if (isRecord)
                {
                    CodeGenerationHelper.EmitRecordMemberSetter(sb, indent, builderName, memberType, member.Name, baseName, member);
                }
                else
                {
                    CodeGenerationHelper.EmitSetter(sb, indent, builderName, memberType, fieldReference, baseName, member.Name, member);
                }

                GenerateCollectionFeatures(sb, indent, builderName, member, memberType, baseName, fieldReference,
                    context, compilation, visitedBuilders, fluentSignatures, config);
                GenerateValidation(sb, indent, builderName, member, memberType, isRecord, context, validator);
                GenerateNestedBuilder(sb, indent, builderName, memberType, member.Name, baseName, fieldReference,
                    GetBuildMethodName(typeSymbol), compilation, visitedBuilders, context, isRecord);
            }
        }

        private static bool TryValidateMemberName(ISymbol member, string proposedName, SourceProductionContext context, out ITypeSymbol memberType)
        {
            memberType = TypeHelper.GetMemberType(member);
            if (memberType == null)
                return false;

            return true;
        }

        private static bool IsPropertySettable(IPropertySymbol property)
        {
            return property.SetMethod != null &&
                   (property.SetMethod.DeclaredAccessibility == Accessibility.Public || property.SetMethod.IsInitOnly);
        }

        private static void GenerateCollectionFeatures(
            StringBuilder sb, string indent, string builderName,
            ISymbol member, ITypeSymbol memberType, string fluentName, string fieldReference,
            SourceProductionContext context, Compilation compilation, HashSet<string> visitedBuilders,
            HashSet<string> fluentSignatures, BuilderConfiguration config)
        {
            if (!TypeHelper.IsCollectionType(memberType))
                return;

            var options = AttributeCache.GetCollectionOptions(member);
            if (options.GenerateCount)
            {
                CodeGenerationHelper.EmitCountProperty(sb, indent, builderName, memberType,
                    fieldReference, fluentName, options);
            }

            // If GenerateAdd is enabled and the element type has its own builder, emit element builder method
            if (options.GenerateAdd && TypeHelper.TryGetElementType(memberType, out var elementType))
            {
                if (elementType is INamedTypeSymbol namedElement && TypeHelper.HasBuilder(elementType, compilation))
                {
                    string elementBuilderName = GetFluentBuilderName(namedElement);
                    string elementBuildMethodName = GetBuildMethodName(namedElement);

                    // Generate a name for the method, e.g., "AddLine" for element type "Line"
                    string elementMethodName = $"Add{StringCache.GetSimpleTypeName(namedElement)}";
                    // Signature includes the Action<ElementBuilder> parameter
                    string elementSignature = $"{elementMethodName}(System.Action<{elementBuilderName}>)";

                    CodeGenerationHelper.EmitCollectionElementBuilder(
                        sb, indent, builderName, memberType, elementType, fieldReference, member.Name,
                        elementBuilderName, elementBuildMethodName, elementMethodName,
                        context, compilation, visitedBuilders);
                }
            }
        }

        private static void GenerateValidation(
            StringBuilder sb, string indent, string builderName,
            ISymbol member, ITypeSymbol memberType, bool isRecord,
            SourceProductionContext context, AttributeValidator validator)
        {
            // Handle built-in validations
            if (AttributeCache.HasValidateAttribute(member))
            {
                ValidationGenerator.GenerateValidationMethod(sb, indent, builderName, member, memberType, member.Name, isRecord);
            }

            // Handle third-party validators (FluentValidateWith)
            foreach (var attr in member.GetAttributes())
            {
                if (attr.AttributeClass?.Name == Constant.AttributeName.FluentValidateWith)
                {
                    if (validator.ValidateThirdPartyValidator(attr, memberType, member.Locations.FirstOrDefault() ?? Location.None, out var validatorType, out var methodName))
                    {
                        var customMessage = AttributeCache.GetNamedArgument<string>(attr, "CustomMessage");
                        ValidationGenerator.GenerateThirdPartyValidationMethod(sb, indent, builderName, member, member.Name, isRecord, validatorType, methodName, customMessage);
                    }
                }
            }
        }

        private static void GenerateNestedBuilder(
            StringBuilder sb,
            string indent,
            string builderName,
            ITypeSymbol memberType,
            string memberName,
            string fluentName,
            string fieldReference,
            string buildMethodName,
            Compilation compilation,
            HashSet<string> visitedBuilders,
            SourceProductionContext context,
            bool isRecord)
        {
            if (TypeHelper.HasBuilder(memberType, compilation))
            {
                CodeGenerationHelper.EmitNestedBuilder(sb, indent, builderName, memberType, buildMethodName,
                    fieldReference, fluentName, context, compilation, visitedBuilders);
            }
        }

        public static string GetBuildMethodName(INamedTypeSymbol classSymbol)
        {
            var attr = AttributeCache.GetAttribute(classSymbol, Constant.AttributeName.FluentBuilderBuildMethod);
            return attr != null && attr.ConstructorArguments.Length > 0
                ? attr.ConstructorArguments[0].Value as string ?? Constant.DefaultNames.Build
                : Constant.DefaultNames.Build;
        }

        public static string GetDefaultValueWithTypeConversion(string defaultValue, ITypeSymbol memberType)
        {
            if (string.IsNullOrEmpty(defaultValue))
                return defaultValue;

            if (memberType?.SpecialType == SpecialType.System_Decimal && defaultValue.EndsWith("d"))
            {
                return defaultValue.TrimEnd('d') + "m";
            }

            return defaultValue;
        }

        private static string GetFluentBuilderName(INamedTypeSymbol classSymbol)
        {
            if (classSymbol == null) return string.Empty;

            // Check for FluentName attribute using cache
            var nameAttr = AttributeCache.GetAttribute(classSymbol, Constant.AttributeName.FluentName);

            string builderBaseName;
            if (nameAttr != null && nameAttr.ConstructorArguments.Length > 0)
            {
                var name = nameAttr.ConstructorArguments[0].Value as string;
                if (!string.IsNullOrEmpty(name) && IdentifierCache.IsValidIdentifier(name))
                {
                    builderBaseName = name;
                }
                else
                {
                    builderBaseName = StringCache.GetSimpleTypeName(classSymbol) + Constant.DefaultNames.BuilderSuffix;
                }
            }
            else
            {
                builderBaseName = StringCache.GetSimpleTypeName(classSymbol) + Constant.DefaultNames.BuilderSuffix;
            }

            // If the class is generic, append its type parameters
            if (classSymbol.IsGenericType)
            {
                var typeParams = string.Join(", ", classSymbol.TypeArguments.Select(t => StringCache.GetTypeDisplayName(t)));
                return $"{builderBaseName}<{typeParams}>";
            }
            return builderBaseName;
        }

        public static bool DoesBuilderMethodExist(Compilation compilation, INamedTypeSymbol targetType, string builderName, string methodName, SourceProductionContext context)
        {
            string ns = targetType.ContainingNamespace?.ToDisplayString() ?? "";
            string builderFullName = string.IsNullOrEmpty(ns) ? builderName : $"{ns}.{builderName}";

            var builderTypes = compilation.GetSymbolsWithName(builderName, SymbolFilter.Type)
                .OfType<INamedTypeSymbol>()
                .Where(t => t.ToDisplayString() == builderFullName)
                .ToList();

            foreach (var builderType in builderTypes)
            {
                var members = builderType.GetMembers(methodName);
                foreach (var member in members)
                {
                    if (member is IMethodSymbol method && method.Parameters.Length == 0)
                        return true;
                }
            }
            return false;
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
