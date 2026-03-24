// <copyright file="Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Configuration;
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Exceptions;
using FluentBuilder.Generator.Extensions;
using FluentBuilder.Generator.Generator;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Managers;
using FluentBuilder.Generator.Parameters;
using FluentBuilder.Generator.Pools;
using FluentBuilder.Generator.Services;
using FluentBuilder.Generator.Validators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;

namespace FluentBuilder.Generator
{
    /// <summary>
    /// A source generator that automatically creates fluent builder classes for types
    /// decorated with the <see cref="FluentBuilderAttribute"/>.
    /// </summary>
    [Generator]
    public class FluentBuilderGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Initializes the source generator and sets up the incremental generation pipeline.
        /// </summary>
        /// <param name="context">The incremental generator initialization context provided by Roslyn.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Clear all caches at start
            // Note: CacheManager should be thread-safe (using locks or [ThreadStatic]) if generation is ever parallelized.
            CacheManager.InitializeGeneration();

            // Read MSBuild properties
            var optionsProvider = context.AnalyzerConfigOptionsProvider;
            var configProvider = optionsProvider.Select(static (options, _) =>
            {
                var config = new GeneratorConfiguration();

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.DefaultBuilderSuffix, out var suffix))
                    config.DefaultBuilderSuffix = suffix;

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.DefaultBuilderNamespace, out var ns))
                    config.DefaultBuilderNamespace = ns;

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.DefaultBuilderAccessibility, out var accessibility))
                    config.DefaultBuilderAccessibility = accessibility;

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.DefaultMethodPrefix, out var prefix))
                    config.DefaultMethodPrefix = prefix;

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.DefaultMethodSuffix, out var suffixMethod))
                    config.DefaultMethodSuffix = suffixMethod;

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.DefaultGeneratePartial, out var partial) &&
                    bool.TryParse(partial, out var partialValue))
                {
                    config.DefaultGeneratePartial = partialValue;
                }

                if (options.GlobalOptions.TryGetValue(BuildPropertyNames.EnableLogging, out var enableLogging) &&
                    bool.TryParse(enableLogging, out var enableLoggingValue))
                {
                    config.EnableLogging = enableLoggingValue;
                }

                return config;
            });

            // Get the attribute symbol once and cache it per compilation
            var attributeSymbolProvider = context.CompilationProvider
                .Select(static (compilation, _) => compilation.GetTypeByMetadataName(Constant.AttributeType.FluentBuilder));

            // Create syntax provider that filters for types with attributes
            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (syntaxNode, _) =>
                    syntaxNode is TypeDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, cancellationToken) =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var typeDecl = (TypeDeclarationSyntax)ctx.Node;
                        var semanticModel = ctx.SemanticModel;

                        if (semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken) is not INamedTypeSymbol typeSymbol)
                            return (TypeTransformResult?)null;

                        var compilation = semanticModel.Compilation;
                        var fluentBuilderAttrType = compilation.GetTypeByMetadataName(Constant.AttributeType.FluentBuilder);

                        if (fluentBuilderAttrType is null)
                        {
                            GeneratorLogger.LogDebug("FluentBuilder attribute type not found in compilation");
                            return (TypeTransformResult?)null;
                        }

                        if (typeSymbol.HasAttribute(fluentBuilderAttrType))
                        {
                            GeneratorLogger.LogDebug($"Found type with FluentBuilder attribute: {typeSymbol.Name}");
                            return new TypeTransformResult(typeSymbol, compilation, true);
                        }

                        return (TypeTransformResult?)null;
                    }
                    catch (OperationCanceledException)
                    {
                        GeneratorLogger.LogDebug("Transformation cancelled");
                        return (TypeTransformResult?)null;
                    }
                    catch (Exception ex)
                    {
                        // Log the error, but we cannot report a diagnostic here due to lack of SourceProductionContext.
                        // If logging is enabled, the user will see this in the debug output.
                        var typeName = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, cancellationToken)?.Name ?? "unknown";
                        GeneratorLogger.LogDebug($"Error transforming type {typeName}: {ex.Message}");
                        GeneratorLogger.LogException(ex, "Error in transform pipeline");
                        return (TypeTransformResult?)null;
                    }
                });

            // Filter out null results
            var nonNullTypes = typeProvider
                .Where(static result => result.HasValue)
                .Select(static (result, _) => result!.Value);

            // Combine with attribute symbol provider to ensure it's not null
            var validTypes = nonNullTypes
                .Combine(attributeSymbolProvider)
                .Where(static tuple => tuple.Right is not null)
                .Select(static (tuple, _) => tuple.Left);

            // Combine with configuration
            var typesWithConfig = validTypes.Combine(configProvider);

            // Register the output
            context.RegisterSourceOutput(typesWithConfig, (sourceProductionContext, tuple) =>
            {
                var (typeInfo, config) = tuple;
                var cancellationToken = sourceProductionContext.CancellationToken;

                // Initialize logger once per generation run (thread-safe)
                GeneratorLogger.InitializeOnce(config);

                // Validate configuration and create services with the real config
                var realConfigService = new BuilderConfigurationService(config, sourceProductionContext);
                var realOrchestrator = new OrchestratorManager(realConfigService);

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    GenerateBuilder(sourceProductionContext, typeInfo, realOrchestrator, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    var location = typeInfo.TypeSymbol.Locations.FirstOrDefault() ?? Location.None;
                    GeneratorLogger.LogInfo(
                        sourceProductionContext,
                        Descriptor.GenerationCancelled,
                        location,
                        typeInfo.TypeSymbol.Name);
                }
                catch (CyclicDependencyException ex)
                {
                    var location = typeInfo.TypeSymbol.Locations.FirstOrDefault() ?? Location.None;
                    DiagnosticHelper.ReportCyclicDependencyError(
                        sourceProductionContext,
                        ex.Message, // Use the exception message which should contain the path
                        location);
                    GeneratorLogger.LogException(ex, $"Cycle detected: {ex.Message}");
                }
                //catch (Exception ex)
                //{
                //    var location = typeInfo.TypeSymbol.Locations.FirstOrDefault() ?? Location.None;
                //    GeneratorLogger.LogError(
                //        sourceProductionContext,
                //        Descriptor.InternalGeneratorError,
                //        location,
                //        typeInfo.TypeSymbol.Name,
                //        ex.Message);
                //    GeneratorLogger.LogException(ex, $"Internal error generating builder for {typeInfo.TypeSymbol.Name}");
                //}
            });
        }

        /// <summary>
        /// Generates the fluent builder source code for a specific type.
        /// </summary>
        /// <param name="context">The source production context used to add source code and report diagnostics.</param>
        /// <param name="typeInfo">Information about the type for which a builder is being generated.</param>
        /// <param name="orchestrator">The orchestrator manager responsible for coordinating the builder generation process.</param>
        /// <param name="cancellationToken">A cancellation token to observe for cancellation requests.</param>
        private static void GenerateBuilder(
            SourceProductionContext context,
            TypeTransformResult typeInfo,
            OrchestratorManager orchestrator,
            CancellationToken cancellationToken)
        {
            var typeSymbol = typeInfo.TypeSymbol;
            var compilation = typeInfo.Compilation;

            cancellationToken.ThrowIfCancellationRequested();

            var validator = new AttributeValidator(context);
            var fluentNameValidator = new FluentNameValidator(context);

            // Validate FluentName attributes on the type before generating
            fluentNameValidator.ValidateType(typeSymbol);
            if (fluentNameValidator.HasErrors)
                return;

            using var pooledSet = new PooledHashSet<string>(initialize: true);
            // Use fully qualified name to avoid ambiguity in cycle detection
            var fullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            using var tracker = new CycleTracker(pooledSet.Set, fullyQualifiedName, typeSymbol.Name);

            try
            {
                orchestrator.GenerateBuilder(
                    context,
                    typeSymbol,
                    compilation,
                    pooledSet.Set,
                    validator);

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (CyclicDependencyException)
            {
                // Re-throw to be caught by the outer handler which reports the diagnostic.
                // The exception message already contains the path.
                throw;
            }
        }
    }
}
