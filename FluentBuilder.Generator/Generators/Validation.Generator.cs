// <copyright file="Validation.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using FluentBuilder.Generator.Extensions;
using FluentBuilder.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class ValidationGenerator
    {
        public static void GenerateValidationCalls(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol)
        {
            var hasValidations = TypeHelper.GetBuildableMembers(typeSymbol).Any(m => AttributeCache.HasValidateAttribute(m)); // changed
            if (!hasValidations) return;

            sb.AppendLine($"{indent}        // Run validations");
            foreach (var member in TypeHelper.GetBuildableMembers(typeSymbol))
            {
                if (AttributeCache.HasValidateAttribute(member)) // changed
                {
                    sb.AppendLine($"{indent}        Validate{member.Name}();");
                }

                if (TypeHelper.IsCollectionType(TypeHelper.GetMemberType(member)))
                {
                    var options = member.GetCollectionOptions(); // now an extension method
                    if (options.HasCountValidation)
                    {
                        CodeGenerationHelper.EmitCountValidation(sb, indent, $"_instance.{member.Name}", member.Name, options);
                    }
                }
            }
        }

        public static void GenerateRequiredMembersValidation(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol)
        {
            var requiredMembers = TypeHelper.GetRequiredMembers(typeSymbol);
            if (!requiredMembers.Any()) return;

            sb.AppendLine($"{indent}        // Validate required members");
            foreach (var member in requiredMembers)
            {
                var memberName = member.Name;
                sb.AppendLine($"{indent}        if (!_overridden.Contains(\"{memberName}\"))");
                sb.AppendLine($"{indent}            throw new System.InvalidOperationException($\"Required member '{memberName}' is not set.\");");
            }
        }

        public static void GenerateCustomValidationCalls(
            StringBuilder sb,
            string indent,
            INamedTypeSymbol typeSymbol,
            bool isAsync)
        {
            var methods = TypeHelper.GetCustomValidationMethods(typeSymbol);
            if (methods.Count == 0) return;

            sb.AppendLine($"{indent}        // Custom validations");
            foreach (var method in methods)
            {
                bool isAsyncMethod = MethodCache.IsAsyncMethod(method);
                var returnType = method.ReturnType;

                if (isAsync && isAsyncMethod)
                {
                    // Async build calling async validation
                    if (returnType.ToDisplayString() == "System.Threading.Tasks.Task<bool>")
                    {
                        sb.AppendLine($"{indent}        if (!await {method.Name}().ConfigureAwait(false))");
                        sb.AppendLine($"{indent}            throw new System.InvalidOperationException(\"Validation failed: {method.Name}\");");
                    }
                    else // Task
                    {
                        sb.AppendLine($"{indent}        await {method.Name}().ConfigureAwait(false);");
                    }
                }
                else if (!isAsync && !isAsyncMethod)
                {
                    // Sync build calling sync validation
                    if (returnType.SpecialType == SpecialType.System_Boolean)
                    {
                        sb.AppendLine($"{indent}        if (!{method.Name}())");
                        sb.AppendLine($"{indent}            throw new System.InvalidOperationException(\"Validation failed: {method.Name}\");");
                    }
                    else // void
                    {
                        sb.AppendLine($"{indent}        {method.Name}();");
                    }
                }
                else if (isAsync && !isAsyncMethod)
                {
                    // Sync method called from async build – still call it without await
                    if (returnType.SpecialType == SpecialType.System_Boolean)
                    {
                        sb.AppendLine($"{indent}        if (!{method.Name}())");
                        sb.AppendLine($"{indent}            throw new System.InvalidOperationException(\"Validation failed: {method.Name}\");");
                    }
                    else // void
                    {
                        sb.AppendLine($"{indent}        {method.Name}();");
                    }
                }
            }
        }
        /// <summary>
        /// Generates a synchronous validation method for a member.
        /// </summary>
        /// <param name="sb">The string builder to append to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builderName">The name of the builder class.</param>
        /// <param name="member">The member symbol being validated.</param>
        /// <param name="memberType">The type of the member.</param>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="isRecord">Indicates whether the builder is for a record (affects field access).</param>
        public static void GenerateValidationMethod(
            StringBuilder sb,
            string indent,
            string builderName,
            ISymbol member,
            ITypeSymbol memberType,
            string memberName,
            bool isRecord)
        {
            var validateAttrs = member.GetAttributes()
                .Where(a => a.AttributeClass?.Name.StartsWith("FluentValidate") == true &&
                            a.AttributeClass?.Name != Constant.AttributeName.FluentValidateAsync)
                .ToList();

            if (validateAttrs.Count == 0)
                return;

            var target = isRecord ? $"_{memberName}" : $"_instance.{memberName}";

            sb.AppendLine($"{indent}    public {builderName} Validate{memberName}()");
            sb.AppendLine($"{indent}    {{");

            foreach (var attr in validateAttrs)
            {
                var attrName = attr.AttributeClass?.Name;

                switch (attrName)
                {
                    case Constant.AttributeName.FluentValidateEqual:
                        var equalValue = attr.GetConstructorArgumentValue(0);
                        if (equalValue != null)
                        {
                            var valueStr = StringCache.FormatValue(equalValue);
                            var displayValue = FormatValueForDisplay(equalValue);
                            sb.AppendLine($"{indent}        if (!Equals({target}, {valueStr}))");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must be equal to {displayValue}\", nameof({target}));");
                        }
                        break;

                    case Constant.AttributeName.FluentValidateNotEqual:
                        var notEqualValue = attr.GetConstructorArgumentValue(0);
                        if (notEqualValue != null)
                        {
                            var valueStr = StringCache.FormatValue(notEqualValue);
                            var displayValue = FormatValueForDisplay(notEqualValue);
                            sb.AppendLine($"{indent}        if (Equals({target}, {valueStr}))");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must not be equal to {displayValue}\", nameof({target}));");
                        }
                        break;

                    case Constant.AttributeName.FluentValidateOneOf:
                        var values = attr.GetConstructorArgumentArray(0);
                        if (values != null && values.Length > 0)
                        {
                            var valueStrings = values.Select(v => StringCache.FormatValue(v)).ToArray();
                            var valuesList = string.Join(", ", valueStrings);
                            var displayValues = string.Join(", ", values.Select(v => FormatValueForDisplay(v)));

                            sb.AppendLine($"{indent}        if ({target} != null && !System.Linq.Enumerable.Contains(new[] {{ {valuesList} }}, {target}))");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must be one of: {displayValues}\", nameof({target}));");
                        }
                        break;

                    case Constant.AttributeName.FluentValidateGreaterThan:
                        var gtValue = attr.GetConstructorArgumentValue(0);
                        if (gtValue != null)
                        {
                            var valueStr = StringCache.FormatValue(gtValue);
                            var displayValue = FormatValueForDisplay(gtValue);
                            sb.AppendLine($"{indent}        if (System.Collections.Generic.Comparer<{StringCache.GetTypeDisplayName(memberType)}>.Default.Compare({target}, {valueStr}) <= 0)");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must be greater than {displayValue}\", nameof({target}));");
                        }
                        break;

                    case Constant.AttributeName.FluentValidateGreaterThanOrEqual:
                        var gteValue = attr.GetConstructorArgumentValue(0);
                        if (gteValue != null)
                        {
                            var valueStr = StringCache.FormatValue(gteValue);
                            var displayValue = FormatValueForDisplay(gteValue);
                            sb.AppendLine($"{indent}        if (System.Collections.Generic.Comparer<{StringCache.GetTypeDisplayName(memberType)}>.Default.Compare({target}, {valueStr}) < 0)");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must be greater than or equal to {displayValue}\", nameof({target}));");
                        }
                        break;

                    case Constant.AttributeName.FluentValidateLessThan:
                        var ltValue = attr.GetConstructorArgumentValue(0);
                        if (ltValue != null)
                        {
                            var valueStr = StringCache.FormatValue(ltValue);
                            var displayValue = FormatValueForDisplay(ltValue);
                            sb.AppendLine($"{indent}        if (System.Collections.Generic.Comparer<{StringCache.GetTypeDisplayName(memberType)}>.Default.Compare({target}, {valueStr}) >= 0)");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must be less than {displayValue}\", nameof({target}));");
                        }
                        break;

                    case Constant.AttributeName.FluentValidateLessThanOrEqual:
                        var lteValue = attr.GetConstructorArgumentValue(0);
                        if (lteValue != null)
                        {
                            var valueStr = StringCache.FormatValue(lteValue);
                            var displayValue = FormatValueForDisplay(lteValue);
                            sb.AppendLine($"{indent}        if (System.Collections.Generic.Comparer<{StringCache.GetTypeDisplayName(memberType)}>.Default.Compare({target}, {valueStr}) > 0)");
                            sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}} must be less than or equal to {displayValue}\", nameof({target}));");
                        }
                        break;

                    default:
                        GenerateStandardValidation(sb, indent, memberName, memberType, attr, isRecord);
                        break;
                }
            }

            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Generates an asynchronous validation method for a member (FluentValidateAsync).
        /// </summary>
        /// <param name="sb">The string builder to append to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builderName">The name of the builder class.</param>
        /// <param name="member">The member symbol being validated.</param>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="validationPrefix">Prefix for the method name (e.g., "Validate").</param>
        /// <param name="context">The source production context for reporting diagnostics.</param>
        /// <param name="isRecord">Indicates whether the builder is for a record.</param>
        /// <param name="includeCancellationToken">Whether to include a CancellationToken parameter.</param>
        public static void GenerateAsyncValidationMethod(
            StringBuilder sb,
            string indent,
            string builderName,
            ISymbol member,
            string memberName,
            string validationPrefix,
            SourceProductionContext context,
            bool isRecord,
            bool includeCancellationToken)
        {
            var attr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == Constant.AttributeName.FluentValidateAsync);

            if (attr == null)
                return;

            var validatorType = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
            var methodName = attr.ConstructorArguments[1].Value as string;

            // Validate that the validator type has a public parameterless constructor
            if (validatorType is INamedTypeSymbol namedValidator)
            {
                var hasCtor = namedValidator.Constructors.Any(c =>
                    c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
                if (!hasCtor)
                {
                    DiagnosticHelper.Report(context, Descriptor.AsyncValidatorMissingConstructor,
                        member.Locations.FirstOrDefault(), namedValidator.Name);
                    return;
                }
            }

            var validatorTypeName = StringCache.GetTypeDisplayName(validatorType);
            var target = isRecord ? $"_{memberName}" : $"_instance.{memberName}";
            var tokenParam = includeCancellationToken ? "System.Threading.CancellationToken cancellationToken = default" : "";
            var tokenArg = includeCancellationToken ? "cancellationToken" : "";

            // Check if the validator method has a CancellationToken parameter
            bool validatorMethodHasToken = MethodCache.HasMethodWithCancellationToken(validatorType, methodName!);
            var callTokenArg = validatorMethodHasToken && includeCancellationToken ? tokenArg : "";

            sb.AppendLine($"{indent}    public async System.Threading.Tasks.Task<{builderName}> {validationPrefix}{memberName}Async({tokenParam})");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        // Create validator instance");
            sb.AppendLine($"{indent}        var validator = new {validatorTypeName}();");
            sb.AppendLine($"{indent}        try");
            sb.AppendLine($"{indent}        {{");
            sb.AppendLine($"{indent}            var isValid = await validator.{methodName!}({target}{callTokenArg}).ConfigureAwait(false);");
            sb.AppendLine($"{indent}            if (!isValid)");
            sb.AppendLine($"{indent}                throw new System.ArgumentException($\"{{nameof({target})}} failed async validation\", nameof({target}));");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine($"{indent}        finally");
            sb.AppendLine($"{indent}        {{");
            sb.AppendLine($"{indent}            if (validator is System.IDisposable disposable)");
            sb.AppendLine($"{indent}            {{");
            sb.AppendLine($"{indent}                disposable.Dispose();");
            sb.AppendLine($"{indent}            }}");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Generates a validation method for a third-party validator (FluentValidateWith).
        /// </summary>
        /// <param name="sb">The string builder to append to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builderName">The name of the builder class.</param>
        /// <param name="member">The member symbol being validated.</param>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="isRecord">Indicates whether the builder is for a record.</param>
        /// <param name="validatorType">The type of the validator.</param>
        /// <param name="methodName">The name of the validation method on the validator.</param>
        /// <param name="customMessage">Optional custom error message.</param>
        public static void GenerateThirdPartyValidationMethod(
            StringBuilder sb,
            string indent,
            string builderName,
            ISymbol member,
            string memberName,
            bool isRecord,
            ITypeSymbol validatorType,
            string methodName,
            string? customMessage)
        {
            var target = isRecord ? $"_{memberName}" : $"_instance.{memberName}";
            var validatorTypeName = StringCache.GetTypeDisplayName(validatorType);
            var message = customMessage ?? $"Validation failed for {memberName}";

            sb.AppendLine($"{indent}    public {builderName} Validate{memberName}()");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var validator = new {validatorTypeName}();");
            sb.AppendLine($"{indent}        if (!validator.{methodName}({target}))");
            sb.AppendLine($"{indent}            throw new System.ArgumentException(\"{EscapeMessage(message)}\", nameof({target}));");
            sb.AppendLine($"{indent}        return this;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        /// <summary>
        /// Generates standard validation logic (Required, MinLength, MaxLength, etc.) from an attribute.
        /// </summary>
        /// <param name="sb">The string builder to append to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="memberType">The type of the member.</param>
        /// <param name="attr">The attribute data.</param>
        /// <param name="isRecord">Indicates whether the builder is for a record.</param>
        internal static void GenerateStandardValidation(
            StringBuilder sb,
            string indent,
            string memberName,
            ITypeSymbol memberType,
            AttributeData attr,
            bool isRecord)
        {
            var required = AttributeCache.GetNamedArgument<bool?>(attr, "Required") ?? false;
            var minLength = AttributeCache.GetNamedArgument<int?>(attr, "MinLength");
            var maxLength = AttributeCache.GetNamedArgument<int?>(attr, "MaxLength");
            var minValue = AttributeCache.GetNamedArgument<double?>(attr, "MinValue");
            var maxValue = AttributeCache.GetNamedArgument<double?>(attr, "MaxValue");
            var regexPattern = AttributeCache.GetNamedArgument<string>(attr, "RegexPattern");
            var customMessage = AttributeCache.GetNamedArgument<string>(attr, "CustomMessage");

            var target = isRecord ? $"_{memberName}" : $"_instance.{memberName}";

            if (required)
            {
                var message = customMessage ?? "Value is required";
                sb.AppendLine($"{indent}        if ({target} == null)");
                sb.AppendLine($"{indent}            throw new System.ArgumentNullException(nameof({target}), $\"{EscapeMessage(message)}\");");
            }

            if (minLength.HasValue && memberType.SpecialType == SpecialType.System_String)
            {
                var message = customMessage ?? $"Minimum length is {minLength.Value}";
                sb.AppendLine($"{indent}        if ({target} != null && {target}.Length < {minLength.Value})");
                sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}}: {EscapeMessage(message)}\", nameof({target}));");
            }

            if (maxLength.HasValue && memberType.SpecialType == SpecialType.System_String)
            {
                var message = customMessage ?? $"Maximum length is {maxLength.Value}";
                sb.AppendLine($"{indent}        if ({target} != null && {target}.Length > {maxLength.Value})");
                sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}}: {EscapeMessage(message)}\", nameof({target}));");
            }

            if (minValue.HasValue && TypeHelper.IsNumericType(memberType))
            {
                var message = customMessage ?? $"Minimum value is {minValue.Value}";
                var convertedValue = ConvertNumericValue(minValue.Value, memberType);
                sb.AppendLine($"{indent}        if ({target} < {convertedValue})");
                sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}}: {EscapeMessage(message)}\", nameof({target}));");
            }

            if (maxValue.HasValue && TypeHelper.IsNumericType(memberType))
            {
                var message = customMessage ?? $"Maximum value is {maxValue.Value}";
                var convertedValue = ConvertNumericValue(maxValue.Value, memberType);
                sb.AppendLine($"{indent}        if ({target} > {convertedValue})");
                sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}}: {EscapeMessage(message)}\", nameof({target}));");
            }

            if (!string.IsNullOrEmpty(regexPattern) && memberType.SpecialType == SpecialType.System_String)
            {
                var message = customMessage ?? "Value does not match pattern";
                var escapedPattern = regexPattern!.Replace("\"", "\"\"");
                sb.AppendLine($"{indent}        if ({target} != null && !System.Text.RegularExpressions.Regex.IsMatch({target}, @\"{escapedPattern}\"))");
                sb.AppendLine($"{indent}            throw new System.ArgumentException($\"{{nameof({target})}}: {EscapeMessage(message)}\", nameof({target}));");
            }
        }

        private static string ConvertNumericValue(double value, ITypeSymbol targetType)
        {
            switch (targetType.SpecialType)
            {
                case SpecialType.System_Int32:
                    return ((int)value).ToString();
                case SpecialType.System_Int64:
                    return ((long)value).ToString() + "L";
                case SpecialType.System_Single:
                    return ((float)value).ToString(System.Globalization.CultureInfo.InvariantCulture) + "f";
                case SpecialType.System_Double:
                    return value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "d";
                case SpecialType.System_Decimal:
                    return ((decimal)value).ToString(System.Globalization.CultureInfo.InvariantCulture) + "m";
                default:
                    return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private static string FormatValueForDisplay(object value)
        {
            if (value == null) return "null";
            if (value is string str) return str;
            if (value is char c) return $"'{c}'";
            return value.ToString() ?? "null";
        }

        private static string EscapeMessage(string message)
        {
            return message?.Replace("\"", "\\\"") ?? string.Empty;
        }
    }
}
