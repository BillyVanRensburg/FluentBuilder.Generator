// <copyright file="Orchestrator.Manager.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using FluentBuilder.Generator.Context;
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Generator;
using FluentBuilder.Generator.Pools;
using FluentBuilder.Generator.Services;
using FluentBuilder.Generator.Validators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FluentBuilder.Generator.Managers
{
    /// <summary>
    /// Orchestrates the complete builder generation process for a given type.
    /// </summary>
    internal sealed class OrchestratorManager
    {
        private readonly BuilderConfigurationService _configurationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorManager"/> class.
        /// </summary>
        /// <param name="configurationService">Service used to retrieve builder configuration from type symbols.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="configurationService"/> is null.</exception>
        public OrchestratorManager(BuilderConfigurationService configurationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        /// <summary>
        /// Generates a builder class for the specified type symbol.
        /// </summary>
        /// <param name="context">The source production context used to report diagnostics and add sources.</param>
        /// <param name="typeSymbol">The type symbol for which a builder should be generated.</param>
        /// <param name="compilation">The current compilation, used for semantic analysis.</param>
        /// <param name="visitedBuilders">A set of builder keys already processed (used to detect circular references).</param>
        /// <param name="attributeValidator">Validator for attribute-related checks.</param>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        public void GenerateBuilder(
            SourceProductionContext context,
            INamedTypeSymbol typeSymbol,
            Compilation compilation,
            HashSet<string> visitedBuilders,
            AttributeValidator attributeValidator)
        {
            if (typeSymbol is null) throw new ArgumentNullException(nameof(typeSymbol));
            if (compilation is null) throw new ArgumentNullException(nameof(compilation));
            if (visitedBuilders is null) throw new ArgumentNullException(nameof(visitedBuilders));
            if (attributeValidator is null) throw new ArgumentNullException(nameof(attributeValidator));

            // Early exit if the type doesn't have the [FluentBuilder] attribute
            if (!AttributeCache.HasAttribute(typeSymbol, Constant.AttributeName.FluentBuilder))
                return;

            var config = _configurationService.GetConfiguration(typeSymbol);
            var validator = new BuilderValidator(attributeValidator);

            // Perform all validations; if fatal, stop generation
            if (!validator.Validate(typeSymbol, config, visitedBuilders, context))
                return;

            // Track all types for using directives
            var usingManager = new UsingDirectiveManager();
            TypeTrackingGenerator.TrackAllTypes(typeSymbol, compilation, usingManager);

            // Prepare generation context
            var genContext = new GenerationContext(
                sourceContext: context,
                compilation: compilation,
                visitedBuilders: visitedBuilders,
                validator: validator,
                typeSymbol: typeSymbol,
                config: config,
                usingManager: usingManager);

            // Rent a pooled StringBuilder and generate the code
            using var rented = StringBuilderPool.Rent();
            var sb = rented.Builder;

            RunGenerationPipeline(sb, genContext);

            // Output the generated source
            var code = sb.ToString();

            // --- Generate a unique file name based on the type's fully qualified name ---
            var fileName = GetUniqueFileName(typeSymbol, config.BuilderName);

            context.AddSource(fileName, SourceText.From(code, Encoding.UTF8));
        }

        /// <summary>
        /// Generates a unique file name for the builder source, based on the type's fully qualified name.
        /// </summary>
        /// <param name="typeSymbol">The type symbol for which the builder is generated.</param>
        /// <param name="builderName">The name of the builder class.</param>
        /// <returns>A file name safe for use in source generators.</returns>
        private static string GetUniqueFileName(INamedTypeSymbol typeSymbol, string builderName)
        {
            // Get fully qualified name, e.g., "global::Namespace.Outer+Inner.MyClass"
            var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // Remove the "global::" prefix if present
            if (fullName.StartsWith("global::"))
                fullName = fullName.Substring(8);

            // Replace invalid file name characters with underscores
            // Invalid chars: < > : " / \ | ? * and also . and + are replaced for consistency
            var sanitized = Regex.Replace(fullName, @"[^\w\d_]", "_");

            // Avoid double underscores and trim
            sanitized = Regex.Replace(sanitized, @"_+", "_").Trim('_');

            // Limit length to avoid path too long issues (arbitrary limit of 100 chars)
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            return $"{sanitized}_{builderName}.g.cs";
        }

        /// <summary>
        /// Executes the code generation pipeline in order.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to which source code is appended.</param>
        /// <param name="context">The generation context containing all necessary data.</param>
        private void RunGenerationPipeline(StringBuilder sb, GenerationContext context)
        {
            var indent = "";
            var config = context.Config;

            BuilderHeaderGenerator.GenerateHeader(sb);
            UsingDirectivesGenerator.GenerateUsings(sb, context.UsingManager);

            // Determine namespace to use
            string ns = !string.IsNullOrEmpty(config.BuilderNamespace)
                ? config.BuilderNamespace!
                : context.TypeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;

            bool hasNamespace = !string.IsNullOrEmpty(ns);
            if (hasNamespace)
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                indent = "    ";
            }

            // Emit containing types as partial classes (if any)
            foreach (var container in config.ContainingTypes)
            {
                // Use only the type name; accessibility is already declared in user code.
                sb.AppendLine($"{indent}partial class {container.Name}");
                sb.AppendLine($"{indent}{{");
                indent += "    ";
            }

            // Generate class declaration for the builder itself
            ClassDeclarationGenerator.GenerateClassDeclaration(sb, indent, config, config.BuilderName);

            // Generate the main builder body (record or class)
            if (config.IsRecord)
            {
                RecordBuilderGenerator.Generate(
                    sb, indent, config.BuilderName, context.TypeSymbol, context.Compilation,
                    context.VisitedBuilders, new HashSet<string>(), context.SourceContext,
                    DefaultValuesGenerator.CollectDefaultValues(context.TypeSymbol, context.SourceContext),
                    config, context.Validator.AttributeValidator);
            }
            else
            {
                ClassBuilderGenerator.Generate(
                    sb, indent, config.BuilderName, context.TypeSymbol, context.Compilation,
                    context.VisitedBuilders, new HashSet<string>(), context.SourceContext,
                    DefaultValuesGenerator.CollectDefaultValues(context.TypeSymbol, context.SourceContext),
                    config, context.Validator.AttributeValidator);
            }

            // Async support
            if (config.HasAsyncSupport)
            {
                AsyncMethodsGenerator.GenerateAsyncFluentMethods(
                    sb, indent, config.BuilderName, context.TypeSymbol,
                    new HashSet<string>(), context.SourceContext,
                    context.Validator.AttributeValidator, config);

                if (config.AsyncValidationEnabled)
                {
                    AsyncMethodsGenerator.GenerateAsyncValidationMethods(
                        sb, indent, config.BuilderName, context.TypeSymbol,
                        config.AsyncValidationPrefix, context.SourceContext,
                        config.IsRecord, config.GenerateCancellationTokens);
                }
            }

            // Build method (with conflict check)
            GenerateBuildMethod(sb, indent, context);

            // Only add a blank line after Build if there is more content to follow
            bool hasMoreContent = config.HasAsyncSupport || config.HasImplicitOperator;
            if (hasMoreContent)
            {
                sb.AppendLine();
            }

            if (config.HasAsyncSupport)
            {
                AsyncMethodsGenerator.GenerateAsyncBuildMethod(
                    sb, indent, config.BuilderName, context.TypeSymbol,
                    config.BuildMethodName, config.AsyncBuildMethodName,
                    config.AsyncValidationPrefix, config.AsyncValidationEnabled,
                    config.GenerateCancellationTokens);
            }

            // Implicit operators
            GenerateImplicitOperators(sb, indent, context);

            // Close the builder class
            sb.AppendLine($"{indent}}}");

            // Close all containing types in reverse order
            for (int i = 0; i < config.ContainingTypes.Count; i++)
            {
                indent = indent.Substring(0, indent.Length - 4);
                sb.AppendLine($"{indent}}}");
            }

            // Generate async extensions INSIDE the namespace
            if (config.HasAsyncSupport)
            {
                AsyncExtensionsGenerator.GenerateAsyncExtensions(
                    sb, indent, config.BuilderName, context.TypeSymbol,
                    config.AsyncBuildMethodName, config.AsyncValidationPrefix,
                    config.AsyncValidationEnabled, config.GenerateCancellationTokens);
            }

            // Close namespace if opened
            if (hasNamespace)
            {
                sb.AppendLine("}");
            }
        }

        /// <summary>
        /// Generates the Build method for the builder class.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="context">The generation context.</param>
        private void GenerateBuildMethod(StringBuilder sb, string indent, GenerationContext context)
        {
            var typeSymbol = context.TypeSymbol;
            var config = context.Config;
            var compilation = context.Compilation;
            var sourceContext = context.SourceContext;

            bool methodExists = BaseBuilderGenerator.DoesBuilderMethodExist(
                compilation, typeSymbol, config.BuilderName, config.BuildMethodName, sourceContext);

            if (methodExists)
            {
                sourceContext.ReportDiagnostic(Diagnostic.Create(
                    Descriptor.BuilderMethodAlreadyExists,
                    typeSymbol.Locations.FirstOrDefault() ?? Location.None,
                    config.BuildMethodName));
                return; // Do not generate the method if it already exists
            }

            if (config.IsRecord || config.HasInitOnlyProperties)
            {
                RecordBuilderGenerator.GenerateBuildMethod(
                    sb, indent, typeSymbol, config.BuildMethodName, config.BuilderName,
                    compilation, sourceContext, config.IsPartial);
            }
            else
            {
                sb.AppendLine($"{indent}    public {StringCache.GetTypeDisplayName(typeSymbol)} {config.BuildMethodName}()");
                sb.AppendLine($"{indent}    {{");

                if (config.IsPartial)
                {
                    sb.AppendLine($"{indent}        OnBeforeBuild();");
                    sb.AppendLine();
                }

                ValidationGenerator.GenerateValidationCalls(sb, indent, typeSymbol);
                ValidationGenerator.GenerateRequiredMembersValidation(sb, indent, typeSymbol);
                ValidationGenerator.GenerateCustomValidationCalls(sb, indent, typeSymbol, isAsync: false);

                bool hasTruthOperator = AttributeCache.HasAttribute(typeSymbol, Constant.AttributeName.FluentTruthOperator);
                if (hasTruthOperator)
                {
                    sb.AppendLine($"{indent}        _built = true;");
                }

                if (config.IsPartial)
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
        }

        /// <summary>
        /// Generates implicit conversion operators for the builder class.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="context">The generation context.</param>
        private static void GenerateImplicitOperators(StringBuilder sb, string indent, GenerationContext context)
        {
            if (context.Config.HasImplicitOperator)
            {
                ImplicitOperatorsGenerator.GenerateImplicitOperator(
                    sb, indent, context.TypeSymbol, context.Config.BuilderName, context.Config.BuildMethodName);
            }

            if (context.Config.HasAsyncSupport)
            {
                ImplicitOperatorsGenerator.GenerateAsyncImplicitOperator(
                    sb, indent, context.TypeSymbol, context.Config.BuilderName, context.Config.AsyncBuildMethodName);
            }
        }
    }
}