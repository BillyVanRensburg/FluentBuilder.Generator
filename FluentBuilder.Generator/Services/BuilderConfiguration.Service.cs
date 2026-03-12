// <copyright file="BuilderConfiguration.Service.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

#nullable enable

using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Configuration;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Extensions;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Services
{
    /// <summary>
    /// Service responsible for extracting builder configuration from a given type symbol.
    /// Results are cached to avoid repeated computation.
    /// </summary>
    public class BuilderConfigurationService
    {
        private readonly GeneratorConfiguration _globalConfig;
        private readonly ConcurrentDictionary<INamedTypeSymbol, BuilderConfiguration> _cache = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderConfigurationService"/> class.
        /// </summary>
        /// <param name="globalConfig">The global configuration from MSBuild properties.</param>
        public BuilderConfigurationService(GeneratorConfiguration globalConfig)
        {
            _globalConfig = globalConfig ?? throw new ArgumentNullException(nameof(globalConfig));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderConfigurationService"/> class
        /// and validates the provided configuration, reporting diagnostics as needed.
        /// The input configuration is not modified; a validated copy is used internally.
        /// </summary>
        /// <param name="globalConfig">The global configuration from MSBuild properties.</param>
        /// <param name="context">The source production context for reporting diagnostics.</param>
        public BuilderConfigurationService(GeneratorConfiguration globalConfig, SourceProductionContext context)
        {
            if (globalConfig == null) throw new ArgumentNullException(nameof(globalConfig));
            _globalConfig = ValidateAndFix(Clone(globalConfig), context);
        }

        /// <summary>
        /// Builds a <see cref="BuilderConfiguration"/> for the specified type symbol.
        /// Results are cached per symbol.
        /// </summary>
        /// <param name="typeSymbol">The named type symbol representing the target class or record.</param>
        /// <returns>A fully populated configuration object.</returns>
        public BuilderConfiguration GetConfiguration(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol is null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return _cache.GetOrAdd(typeSymbol, CreateConfiguration);
        }

        private BuilderConfiguration CreateConfiguration(INamedTypeSymbol typeSymbol)
        {
            var config = new BuilderConfiguration
            {
                IsRecord = typeSymbol.IsRecord,
                HasImplicitOperator = typeSymbol.HasAttribute(Constant.AttributeName.FluentImplicit)
            };

            // Capture containing types (outermost first)
            var current = typeSymbol.ContainingType;
            while (current != null)
            {
                config.ContainingTypes.Insert(0, current);
                current = current.ContainingType;
            }

            // Handle generic type parameters
            if (typeSymbol.IsGenericType)
            {
                foreach (var typeArg in typeSymbol.TypeArguments)
                {
                    config.TypeParameters.Add(typeArg);
                }
            }

            ConfigureBuilderName(typeSymbol, config);
            ConfigureBuildMethodName(typeSymbol, config);
            AsyncConfigurationProcessor.ConfigureAsyncSupport(typeSymbol, config);

            // Read all builder configuration from the FluentBuilder attribute in one pass
            if (typeSymbol.TryGetAttribute(Constant.AttributeName.FluentBuilder, out var builderAttr))
            {
                // --- FIXED: Handle BuilderAccessibility enum properly ---
                string? accessibilityFromAttr = null;
                var accessibilityArg = builderAttr.NamedArguments.FirstOrDefault(kvp => kvp.Key == nameof(FluentBuilderAttribute.BuilderAccessibility));
                if (accessibilityArg.Key != null)
                {
                    var typedConstant = accessibilityArg.Value;
                    if (typedConstant.Kind == TypedConstantKind.Enum && typedConstant.Type is INamedTypeSymbol enumType)
                    {
                        // Get the integer value of the enum
                        var intValue = typedConstant.Value is int intVal ? intVal : Convert.ToInt32(typedConstant.Value);
                        // Find the enum member with that value
                        var member = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.HasConstantValue && f.ConstantValue?.Equals(intValue) == true);
                        if (member != null)
                        {
                            accessibilityFromAttr = member.Name;
                        }
                    }
                    else if (typedConstant.Value is string stringVal) // fallback for any legacy string usage
                    {
                        accessibilityFromAttr = stringVal;
                    }
                }

                if (!string.IsNullOrEmpty(accessibilityFromAttr))
                {
                    config.BuilderAccessibility = MapEnumNameToAccessibility(accessibilityFromAttr);
                }
                else
                {
                    config.BuilderAccessibility = _globalConfig.DefaultBuilderAccessibility;
                }
                // ---------------------------------------------------------

                config.BuilderNamespace = GetNamedArgument<string>(builderAttr, nameof(FluentBuilderAttribute.BuilderNamespace))
                                          ?? _globalConfig.DefaultBuilderNamespace;

                config.MethodPrefix = GetNamedArgument<string>(builderAttr, nameof(FluentBuilderAttribute.MethodPrefix))
                                      ?? _globalConfig.DefaultMethodPrefix;

                config.MethodSuffix = GetNamedArgument<string>(builderAttr, nameof(FluentBuilderAttribute.MethodSuffix))
                                      ?? _globalConfig.DefaultMethodSuffix;

                config.IsPartial = GetNamedArgument<bool?>(builderAttr, nameof(FluentBuilderAttribute.GeneratePartial))
                                   ?? _globalConfig.DefaultGeneratePartial;
            }
            else
            {
                // Fallback to global defaults
                config.BuilderAccessibility = _globalConfig.DefaultBuilderAccessibility;
                config.BuilderNamespace = _globalConfig.DefaultBuilderNamespace;
                config.MethodPrefix = _globalConfig.DefaultMethodPrefix;
                config.MethodSuffix = _globalConfig.DefaultMethodSuffix;
                config.IsPartial = _globalConfig.DefaultGeneratePartial;
            }

            config.HasInitOnlyProperties = TypeHelper.HasInitOnlyProperties(typeSymbol);

            return config;
        }

        /// <summary>
        /// Creates a shallow copy of the given <see cref="GeneratorConfiguration"/>.
        /// </summary>
        private static GeneratorConfiguration Clone(GeneratorConfiguration original)
        {
            return new GeneratorConfiguration
            {
                DefaultBuilderAccessibility = original.DefaultBuilderAccessibility,
                DefaultBuilderNamespace = original.DefaultBuilderNamespace,
                DefaultBuilderSuffix = original.DefaultBuilderSuffix,
                DefaultMethodPrefix = original.DefaultMethodPrefix,
                DefaultMethodSuffix = original.DefaultMethodSuffix,
                DefaultGeneratePartial = original.DefaultGeneratePartial
                // Include any other properties that exist.
            };
        }

        /// <summary>
        /// Validates the global configuration values and fixes any invalid ones, reporting warnings.
        /// </summary>
        /// <param name="config">The configuration to validate (a copy, not the original).</param>
        /// <param name="context">The source production context for reporting diagnostics.</param>
        /// <returns>A validated configuration (the same instance, possibly modified).</returns>
        private static GeneratorConfiguration ValidateAndFix(GeneratorConfiguration config, SourceProductionContext context)
        {
            // Validate DefaultBuilderAccessibility
            if (!IsValidAccessibility(config.DefaultBuilderAccessibility))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor.InvalidBuilderAccessibility,
                    Location.None,
                    config.DefaultBuilderAccessibility));

                // Safe fallback
                config.DefaultBuilderAccessibility = "public";
            }

            // Validate DefaultBuilderNamespace
            if (!IsValidNamespace(config.DefaultBuilderNamespace))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor.InvalidBuilderNamespace,
                    Location.None,
                    config.DefaultBuilderNamespace));

                config.DefaultBuilderNamespace = string.Empty; // Fallback to empty, meaning use containing namespace
            }

            // Ensure DefaultBuilderSuffix is not null or empty
            if (string.IsNullOrEmpty(config.DefaultBuilderSuffix))
            {
                config.DefaultBuilderSuffix = Constant.Defaults.BuilderSuffixFallback;
                // Optionally report a warning
            }

            // Ensure prefixes and suffixes are not null
            config.DefaultMethodPrefix ??= string.Empty;
            config.DefaultMethodSuffix ??= string.Empty;

            return config;
        }

        /// <summary>
        /// Checks if the provided string is a valid C# accessibility modifier.
        /// </summary>
        private static bool IsValidAccessibility(string value)
        {
            return value switch
            {
                "public" => true,
                "internal" => true,
                "private" => true,
                "protected" => true,
                "protected internal" => true,
                "private protected" => true,
                "file" => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the provided string is a valid namespace name (empty or dot-separated valid identifiers).
        /// </summary>
        private static bool IsValidNamespace(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return true; // Empty is allowed (means use containing namespace)

            var parts = value!.Split('.');
            foreach (var part in parts)
            {
                if (!SyntaxFacts.IsValidIdentifier(part))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Helper to extract a named argument value from an attribute.
        /// </summary>
        private static T? GetNamedArgument<T>(AttributeData attribute, string argumentName)
        {
            foreach (var arg in attribute.NamedArguments)
            {
                if (arg.Key == argumentName && arg.Value.Value is T value)
                    return value;
            }
            return default;
        }

        // --- Map enum member name to C# accessibility keyword (inline strings) ---
        private static string MapEnumNameToAccessibility(string enumName)
        {
            return enumName switch
            {
                nameof(BuilderAccessibility.Public)           => "public",
                nameof(BuilderAccessibility.Internal)         => "internal",
                nameof(BuilderAccessibility.Private)          => "private",
                nameof(BuilderAccessibility.Protected)        => "protected",
                nameof(BuilderAccessibility.ProtectedInternal) => "protected internal",
                nameof(BuilderAccessibility.PrivateProtected) => "private protected",
                nameof(BuilderAccessibility.File)             => "file",
                _ => "public" // safe fallback
            };
        }
        // -------------------------------------------------------------

        /// <summary>
        /// Determines the builder class name and partial status based on attributes and defaults.
        /// </summary>
        private void ConfigureBuilderName(INamedTypeSymbol typeSymbol, BuilderConfiguration config)
        {
            // Check if the class has a FluentName attribute for the builder
            var name = typeSymbol.GetAttributeStringValue(
                Constant.AttributeName.FluentName,
                defaultValue: null);

            if (!string.IsNullOrEmpty(name) && IdentifierCache.IsValidIdentifier(name!))
            {
                config.BuilderName = name!;
                return;
            }

            // Fall back to default naming convention using global suffix
            config.BuilderName = typeSymbol.Name + _globalConfig.DefaultBuilderSuffix;
        }

        /// <summary>
        /// Configures the synchronous build method name from attributes, if present.
        /// Defaults to "Build" if not specified.
        /// </summary>
        private static void ConfigureBuildMethodName(INamedTypeSymbol typeSymbol, BuilderConfiguration config)
        {
            var buildMethodName = typeSymbol.GetAttributeStringValue(
                Constant.AttributeName.FluentBuilderBuildMethod,
                defaultValue: null);

            config.BuildMethodName = buildMethodName ?? Constant.Defaults.BuildMethodNameFallback;
        }
    }

    /// <summary>
    /// Processes async configuration for builders.
    /// </summary>
    internal static class AsyncConfigurationProcessor
    {
        /// <summary>
        /// Configures async support settings by inspecting the <see cref="FluentBuilderAsyncSupportAttribute"/>
        /// or by detecting async methods/validations.
        /// </summary>
        public static void ConfigureAsyncSupport(INamedTypeSymbol typeSymbol, BuilderConfiguration config)
        {
            var asyncAttr = typeSymbol.GetAttribute(Constant.AttributeName.FluentBuilderAsyncSupport);

            if (asyncAttr is null)
            {
                ConfigureDefaultAsyncSupport(typeSymbol, config);
                return;
            }

            ConfigureAsyncSupportFromAttribute(asyncAttr, config);
            EnsureAsyncValidationConsistency(typeSymbol, config);
        }

        /// <summary>
        /// Sets async support based on presence of async methods or validation attributes when no explicit attribute is present.
        /// Uses a single pass over all members to minimize allocations.
        /// </summary>
        private static void ConfigureDefaultAsyncSupport(INamedTypeSymbol typeSymbol, BuilderConfiguration config)
        {
            bool hasAsyncMethods = false;
            bool hasAsyncValidations = false;

            // Single pass over all members (consider inherited members? Currently only declared ones.)
            foreach (var member in typeSymbol.GetMembers())
            {
                if (!hasAsyncMethods && member is IMethodSymbol method)
                {
                    if (MethodCache.IsAsyncMethod(method))
                    {
                        hasAsyncMethods = true;
                        if (hasAsyncValidations) break;
                    }
                }

                if (!hasAsyncValidations && IsBuildableMember(member))
                {
                    if (member.HasAttribute(Constant.AttributeName.FluentValidateAsync))
                    {
                        hasAsyncValidations = true;
                        if (hasAsyncMethods) break;
                    }
                }
            }

            config.HasAsyncSupport = hasAsyncMethods || hasAsyncValidations;
            config.AsyncValidationEnabled = hasAsyncValidations;
        }

        /// <summary>
        /// Processes the <see cref="FluentBuilderAsyncSupportAttribute"/> and applies its named/constructor arguments.
        /// </summary>
        private static void ConfigureAsyncSupportFromAttribute(AttributeData asyncAttr, BuilderConfiguration config)
        {
            // Default to true if attribute exists
            config.HasAsyncSupport = true;

            // Process named arguments
            foreach (var arg in asyncAttr.NamedArguments)
                ProcessAsyncNamedArgument(arg, config);

            // Check constructor argument (if provided)
            if (asyncAttr.ConstructorArguments.Length > 0)
                ProcessAsyncConstructorArgument(asyncAttr.ConstructorArguments[0], config);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessAsyncNamedArgument(KeyValuePair<string, TypedConstant> arg, BuilderConfiguration config)
        {
            switch (arg.Key)
            {
                case Constant.AsyncSupportProperties.GenerateAsyncBuild when arg.Value.Value is bool generate:
                    config.HasAsyncSupport = generate;
                    break;

                case Constant.AsyncSupportProperties.AsyncBuildMethodName when arg.Value.Value is string methodName && !string.IsNullOrEmpty(methodName):
                    config.AsyncBuildMethodName = methodName;
                    break;

                case Constant.AsyncSupportProperties.AsyncValidationPrefix when arg.Value.Value is string prefix && !string.IsNullOrEmpty(prefix):
                    config.AsyncValidationPrefix = prefix;
                    break;

                case Constant.AsyncSupportProperties.GenerateAsyncValidation when arg.Value.Value is bool validate:
                    config.AsyncValidationEnabled = validate;
                    break;

                case Constant.AsyncSupportProperties.GenerateCancellationTokens when arg.Value.Value is bool tokens:
                    config.GenerateCancellationTokens = tokens;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessAsyncConstructorArgument(TypedConstant constructorArg, BuilderConfiguration config)
        {
            if (constructorArg.Value is string methodName && !string.IsNullOrEmpty(methodName))
                config.AsyncBuildMethodName = methodName;
        }

        /// <summary>
        /// Ensures that async validation is enabled if any members have the <see cref="FluentValidateAsyncAttribute"/>,
        /// regardless of attribute settings.
        /// </summary>
        private static void EnsureAsyncValidationConsistency(INamedTypeSymbol typeSymbol, BuilderConfiguration config)
        {
            if (config.AsyncValidationEnabled)
                return;

            // Manual loop over buildable members – avoids LINQ allocation and exits early.
            foreach (var member in typeSymbol.GetMembers())
            {
                if (IsBuildableMember(member) &&
                    member.HasAttribute(Constant.AttributeName.FluentValidateAsync))
                {
                    config.AsyncValidationEnabled = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Determines if a member is buildable (public instance property with a setter or init-only, or public instance field).
        /// </summary>
        private static bool IsBuildableMember(ISymbol member)
        {
            if (member.DeclaredAccessibility != Accessibility.Public || member.IsStatic)
                return false;

            return member switch
            {
                IFieldSymbol _ => true,
                IPropertySymbol prop => prop.SetMethod != null || prop.IsInitOnly(),
                _ => false
            };
        }

        // Extension method to check if a property is init-only (requires Roslyn 3.8+)
        private static bool IsInitOnly(this IPropertySymbol property) =>
            property.SetMethod?.IsInitOnly == true;
    }
}