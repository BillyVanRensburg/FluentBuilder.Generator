// <copyright file="CodeGeneration.Helper.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using FluentBuilder.Generator.Helpers; // For TypeHelper, etc.
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    /// <summary>
    /// Provides helper methods for generating source code for fluent builders.
    /// This class acts as a facade that delegates to dedicated generator classes
    /// within the same namespace.
    /// </summary>
    internal static class CodeGenerationHelper
    {
        /// <summary>
        /// Gets the cached display name of a type.
        /// </summary>
        /// <param name="type">The type symbol.</param>
        /// <returns>A string representation of the type suitable for code generation.</returns>
        /// <remarks>
        /// This method uses aggressive inlining for performance and relies on
        /// <see cref="StringCache.GetTypeDisplayName(ITypeSymbol)"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCachedTypeName(ITypeSymbol type)
            => StringCache.GetTypeDisplayName(type);

        /// <summary>
        /// Formats a value for code generation using the string cache.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value suitable for embedding in source code.</returns>
        /// <remarks>
        /// This method uses aggressive inlining and delegates to
        /// <see cref="StringCache.FormatValue(object)"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatValueCached(object value)
            => StringCache.FormatValue(value);

        /// <summary>
        /// Emits setter methods for a record member.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string (e.g., a series of spaces or tabs).</param>
        /// <param name="builderName">The name of the builder class being generated.</param>
        /// <param name="memberType">The <see cref="ITypeSymbol"/> of the member.</param>
        /// <param name="memberName">The name of the member (used for field names and tracking).</param>
        /// <param name="methodName">The base name for the setter methods (e.g., "WithName").</param>
        /// <param name="memberSymbol">
        /// Optional <see cref="ISymbol"/> of the member. If provided, collection options are read from attributes.
        /// </param>
        /// <remarks>
        /// This method generates:
        /// <list type="bullet">
        /// <item>A direct setter that assigns a value.</item>
        /// <item>An action-based setter for complex types (if enabled).</item>
        /// <item>Collection helper methods (Add, Remove, Clear, AddRange) for collection members (if applicable).</item>
        /// </list>
        /// The generation is controlled by <see cref="CollectionOptions"/> derived from the member symbol.
        /// </remarks>
        public static void EmitRecordMemberSetter(
            StringBuilder sb,
            string indent,
            string builderName,
            ITypeSymbol memberType,
            string memberName,
            string methodName,
            ISymbol? memberSymbol = null)
        {
            if (sb == null || memberType == null || string.IsNullOrEmpty(memberName) || string.IsNullOrEmpty(methodName))
                return;

            var typeName = StringCache.GetNullableTypeDisplayName(memberType);
            var options = memberSymbol != null
                ? AttributeCache.GetCollectionOptions(memberSymbol)
                : new CollectionOptions();

            // Direct setter
            if (options.GenerateDirectSetter)
            {
                SetterMethodGenerator.EmitDirectSetter(
                    sb,
                    indent,
                    builderName,
                    methodName,
                    typeName,
                    $"_{memberName}",
                    memberName);
            }

            // Action setter for complex types
            bool shouldGenerateAction = options.GenerateActionSetter && TypeHelper.ShouldUseAction(memberType);
            if (shouldGenerateAction)
            {
                bool initializeIfNull = TypeHelper.IsCollectionType(memberType);
                SetterMethodGenerator.EmitActionSetter(
                    sb,
                    indent,
                    builderName,
                    methodName,
                    typeName,
                    $"_{memberName}",
                    memberName,
                    initializeIfNull);
            }

            // Collection-specific helpers
            if (options.GenerateAdd || options.GenerateRemove || options.GenerateClear || options.GenerateAddRange)
            {
                EmitCollectionHelpers(sb, indent, builderName, memberType, $"_{memberName}", methodName, memberSymbol);
            }
        }

        /// <summary>
        /// Emits setter methods for a regular (non-record) member.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builderName">The name of the builder class being generated.</param>
        /// <param name="type">The <see cref="ITypeSymbol"/> of the member.</param>
        /// <param name="target">
        /// The expression that accesses the target field or property (e.g., "_instance.Property").
        /// </param>
        /// <param name="name">The base name for the setter methods.</param>
        /// <param name="memberName">The name of the member (used for tracking overridden state).</param>
        /// <param name="memberSymbol">
        /// Optional <see cref="ISymbol"/> of the member. If provided, collection options are read from attributes.
        /// </param>
        /// <remarks>
        /// Similar to <see cref="EmitRecordMemberSetter"/>, but uses a custom target expression
        /// instead of a simple field.
        /// </remarks>
        public static void EmitSetter(
            StringBuilder sb,
            string indent,
            string builderName,
            ITypeSymbol type,
            string target,
            string name,
            string memberName,
            ISymbol? memberSymbol = null)
        {
            if (sb == null || type == null || string.IsNullOrEmpty(target) || string.IsNullOrEmpty(name))
                return;

            var typeName = StringCache.GetNullableTypeDisplayName(type);
            var options = memberSymbol != null
                ? AttributeCache.GetCollectionOptions(memberSymbol)
                : new CollectionOptions();

            // Direct setter
            if (options.GenerateDirectSetter)
            {
                SetterMethodGenerator.EmitDirectSetter(
                    sb,
                    indent,
                    builderName,
                    name,
                    typeName,
                    target,
                    memberName);
            }

            // Action setter for complex types
            bool shouldGenerateAction = options.GenerateActionSetter && TypeHelper.ShouldUseAction(type);
            if (shouldGenerateAction)
            {
                SetterMethodGenerator.EmitActionSetter(
                    sb,
                    indent,
                    builderName,
                    name,
                    typeName,
                    target,
                    memberName,
                    initializeIfNull: false);
            }

            // Collection helpers
            if (options.GenerateAdd || options.GenerateRemove || options.GenerateClear || options.GenerateAddRange)
            {
                EmitCollectionHelpers(sb, indent, builderName, type, target, name, memberSymbol);
            }
        }

        /// <summary>
        /// Emits helper methods for collection members (Add, Remove, Clear, AddRange).
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builder">The name of the builder class.</param>
        /// <param name="type">The collection type symbol.</param>
        /// <param name="target">The expression accessing the collection field.</param>
        /// <param name="name">The base name for the helper methods.</param>
        /// <param name="memberSymbol">Optional symbol of the member for attribute lookup.</param>
        /// <remarks>
        /// This method determines the kind of collection (list-like or dictionary-like) and
        /// delegates to <see cref="CollectionHelperGenerator"/> for emitting appropriate methods.
        /// Generation is controlled by <see cref="CollectionOptions"/>.
        /// </remarks>
        public static void EmitCollectionHelpers(
            StringBuilder sb,
            string indent,
            string builder,
            ITypeSymbol type,
            string target,
            string name,
            ISymbol? memberSymbol)
        {
            if (sb == null || type == null || string.IsNullOrEmpty(target) || string.IsNullOrEmpty(name))
                return;

            if (!TypeHelper.HasAddMethod(type))
                return;

            var options = memberSymbol != null
                ? AttributeCache.GetCollectionOptions(memberSymbol)
                : new CollectionOptions();

            var typeName = StringCache.GetNullableTypeDisplayName(type);

            // Generic collection handling (List<T>, Dictionary<TKey,TValue>, etc.)
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                var typeArgs = namedType.TypeArguments;
                if (typeArgs.Length == 1)
                {
                    var itemType = StringCache.GetNullableTypeDisplayName(typeArgs[0]);

                    if (options.GenerateAdd)
                    {
                        CollectionHelperGenerator.EmitAddForList(
                            sb,
                            indent,
                            builder,
                            name,
                            itemType,
                            typeName,
                            target,
                            name);
                    }

                    if (options.GenerateRemove && TypeHelper.HasRemoveMethod(type))
                    {
                        CollectionHelperGenerator.EmitRemoveForList(
                            sb,
                            indent,
                            builder,
                            name,
                            itemType,
                            target,
                            name);
                    }

                    if (options.GenerateAddRange && TypeHelper.IsListType(type))
                    {
                        CollectionHelperGenerator.EmitAddRangeForList(
                            sb,
                            indent,
                            builder,
                            name,
                            itemType,
                            typeName,
                            target,
                            name);
                    }
                }
                else if (typeArgs.Length == 2)
                {
                    var keyType = StringCache.GetNullableTypeDisplayName(typeArgs[0]);
                    var valueType = StringCache.GetNullableTypeDisplayName(typeArgs[1]);

                    if (options.GenerateAdd)
                    {
                        CollectionHelperGenerator.EmitAddForDictionary(
                            sb,
                            indent,
                            builder,
                            name,
                            keyType,
                            valueType,
                            typeName,
                            target,
                            name);
                    }

                    if (options.GenerateRemove && TypeHelper.HasRemoveMethod(type))
                    {
                        CollectionHelperGenerator.EmitRemoveForDictionary(
                            sb,
                            indent,
                            builder,
                            name,
                            keyType,
                            target,
                            name);
                    }
                }
            }

            // Clear method
            if (options.GenerateClear && TypeHelper.HasClearMethod(type))
            {
                CollectionHelperGenerator.EmitClear(
                    sb,
                    indent,
                    builder,
                    name,
                    target,
                    name);
            }
        }

        /// <summary>
        /// Emits a Count property for a collection member.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builderName">The name of the builder class (unused, kept for consistency).</param>
        /// <param name="type">The collection type symbol.</param>
        /// <param name="target">The expression accessing the collection field.</param>
        /// <param name="name">The property name prefix (e.g., "Items" for "ItemsCount").</param>
        /// <param name="options">Collection options controlling generation; must have <see cref="CollectionOptions.GenerateCount"/> set to <c>true</c>.</param>
        /// <remarks>
        /// Delegates to <see cref="CountPropertyGenerator.EmitCountProperty"/>.
        /// </remarks>
        public static void EmitCountProperty(
            StringBuilder sb,
            string indent,
            string builderName,
            ITypeSymbol type,
            string target,
            string name,
            CollectionOptions options)
        {
            if (sb == null || type == null || string.IsNullOrEmpty(target) || !options.GenerateCount)
                return;

            CountPropertyGenerator.EmitCountProperty(
                sb,
                indent,
                $"{name}Count",
                target);
        }

        /// <summary>
        /// Emits count validation code for a collection in the Build method.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="target">The expression accessing the collection field.</param>
        /// <param name="name">The name of the collection member (used in error messages).</param>
        /// <param name="options">Collection options containing validation rules (<see cref="CollectionOptions.HasCountValidation"/>).</param>
        /// <remarks>
        /// Delegates to <see cref="CountValidationGenerator.EmitCountValidation"/>.
        /// </remarks>
        public static void EmitCountValidation(
            StringBuilder sb,
            string indent,
            string target,
            string name,
            CollectionOptions options)
        {
            CountValidationGenerator.EmitCountValidation(sb, indent, target, name, options);
        }

        /// <summary>
        /// Emits a method that configures and adds an element to a collection using its own builder.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builderName">The name of the builder class.</param>
        /// <param name="collectionType">The type of the collection.</param>
        /// <param name="elementType">The type of the collection elements.</param>
        /// <param name="targetField">The expression accessing the collection field.</param>
        /// <param name="collectionPropertyName">The name of the collection property (for tracking).</param>
        /// <param name="elementBuilderName">The name of the builder for the element type.</param>
        /// <param name="elementBuildMethodName">The name of the Build method on the element builder (e.g., "Build").</param>
        /// <param name="methodName">The name of the method to emit.</param>
        /// <param name="context">The source production context (unused, kept for consistency).</param>
        /// <param name="compilation">The compilation (unused, kept for consistency).</param>
        /// <param name="visitedBuilders">Set of visited builders (unused, kept for consistency).</param>
        /// <remarks>
        /// Delegates to <see cref="NestedBuilderGenerator.EmitCollectionElementBuilderMethod"/>.
        /// </remarks>
        public static void EmitCollectionElementBuilder(
            StringBuilder sb,
            string indent,
            string builderName,
            ITypeSymbol collectionType,
            ITypeSymbol elementType,
            string targetField,
            string collectionPropertyName,
            string elementBuilderName,
            string elementBuildMethodName,
            string methodName,
            SourceProductionContext context,
            Compilation compilation,
            HashSet<string> visitedBuilders)
        {
            if (sb == null || collectionType == null || elementType == null)
                return;

            NestedBuilderGenerator.EmitCollectionElementBuilderMethod(
                sb,
                indent,
                builderName,
                methodName,
                elementBuilderName,
                StringCache.GetNullableTypeDisplayName(collectionType),
                targetField,
                elementBuildMethodName,
                collectionPropertyName);
        }

        /// <summary>
        /// Emits a nested builder method for a complex property.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
        /// <param name="indent">The current indentation string.</param>
        /// <param name="builder">The name of the builder class.</param>
        /// <param name="type">The type of the property.</param>
        /// <param name="buildMethod">The name of the Build method on the nested builder (e.g., "Build").</param>
        /// <param name="target">The expression accessing the target field/property.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="context">The source production context for reporting diagnostics.</param>
        /// <param name="compilation">The compilation (used to check if the target type has a builder).</param>
        /// <param name="visitedBuilders">Set of visited builders to detect cycles (unused, kept for consistency).</param>
        /// <remarks>
        /// If the target type does not have a corresponding builder, a diagnostic warning is reported.
        /// Delegates to <see cref="NestedBuilderGenerator.EmitNestedBuilderMethod"/>.
        /// </remarks>
        public static void EmitNestedBuilder(
            StringBuilder sb,
            string indent,
            string builder,
            ITypeSymbol type,
            string buildMethod,
            string target,
            string name,
            SourceProductionContext context,
            Compilation compilation,
            HashSet<string> visitedBuilders)
        {
            if (sb == null || type == null || string.IsNullOrEmpty(target) || string.IsNullOrEmpty(name))
                return;

            if (!TypeHelper.HasBuilder(type, compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor.NestedBuilderWarning,
                    null,
                    type?.Name ?? "unknown",
                    name));
                return;
            }

            string builderTypeName = type is INamedTypeSymbol nts
                ? GetFluentBuilderName(nts)
                : type.Name + Constant.DefaultNames.BuilderSuffix;

            NestedBuilderGenerator.EmitNestedBuilderMethod(
                sb,
                indent,
                builder,
                name,
                builderTypeName,
                target,
                buildMethod,
                name);
        }

        /// <summary>
        /// Determines the fluent builder name for a given type symbol, respecting any
        /// <see cref="Constant.AttributeName.FluentName"/> custom attribute.
        /// </summary>
        /// <param name="classSymbol">The named type symbol.</param>
        /// <returns>The name of the builder class, or <see cref="string.Empty"/> if the symbol is null.</returns>
        private static string GetFluentBuilderName(INamedTypeSymbol classSymbol)
        {
            if (classSymbol == null) return string.Empty;

            var nameAttr = AttributeCache.GetAttribute(classSymbol, Constant.AttributeName.FluentName);
            if (nameAttr?.ConstructorArguments.Length > 0)
            {
                var name = nameAttr.ConstructorArguments[0].Value as string;
                if (!string.IsNullOrEmpty(name) && IdentifierCache.IsValidIdentifier(name!))
                {
                    return name!;
                }
            }

            return classSymbol.Name + Constant.DefaultNames.BuilderSuffix;
        }
    }
}