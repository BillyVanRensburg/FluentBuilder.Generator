// <copyright file="UsingDirective.Manager.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBuilder.Generator.Managers
{
    /// <summary>
    /// Collects and manages the set of using directives required for the generated builder.
    /// </summary>
    /// <remarks>
    /// This class tracks types, methods, attributes, and special features (e.g., async, validation)
    /// to determine all necessary namespace imports. It ensures that the generated code
    /// compiles without missing using directives and avoids adding internal generator namespaces.
    /// </remarks>
    internal class UsingDirectiveManager
    {
        private readonly HashSet<string> _requiredUsings = new HashSet<string>
        {
            "System",
            "System.Collections.Generic",
        };

        private readonly HashSet<string> _namespacesToTrack = new HashSet<string>();

        // Internal generator namespaces that should be excluded from using directives.
        private static readonly string[] InternalNamespaces = new[]
        {
            "FluentBuilder.Generator",
            "FluentBuilder.Generator.Helpers",
            "FluentBuilder.Generator.Builders",
            "FluentBuilder.Generator.Extensions",
            "FluentBuilder.Generator.Pooling"
        };

        /// <summary>
        /// Adds a namespace to the required usings, if it is not excluded.
        /// </summary>
        /// <param name="namespace">The namespace to add.</param>
        /// <remarks>
        /// This method is used for explicitly required namespaces, such as those needed
        /// for special features (e.g., <c>System.Threading.Tasks</c> for async).
        /// </remarks>
        public void AddRequiredUsing(string? @namespace)
        {
            if (string.IsNullOrEmpty(@namespace)) return;
            if (ShouldExcludeNamespace(@namespace)) return;
            _requiredUsings.Add(@namespace!);
        }

        /// <summary>
        /// Recursively tracks a type symbol and its containing namespace,
        /// including generic type arguments and array element types.
        /// </summary>
        /// <param name="type">The type symbol to track.</param>
        /// <remarks>
        /// This method adds the type's namespace to the tracked set and recursively
        /// processes any generic arguments or element types.
        /// </remarks>
        public void TrackType(ITypeSymbol? type)
        {
            if (type == null) return;

            var ns = type.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrEmpty(ns) && !ShouldExcludeNamespace(ns))
            {
                // ns is guaranteed non-null here due to IsNullOrEmpty check
                _namespacesToTrack.Add(ns!);
            }

            // Track generic arguments
            if (type is INamedTypeSymbol namedType)
            {
                foreach (var arg in namedType.TypeArguments)
                {
                    TrackType(arg);
                }
            }

            // Track array element type
            if (type is IArrayTypeSymbol arrayType)
            {
                TrackType(arrayType.ElementType);
            }
        }

        /// <summary>
        /// Tracks a method symbol, including its return type and parameter types.
        /// </summary>
        /// <param name="method">The method symbol to track.</param>
        public void TrackMethod(IMethodSymbol? method)
        {
            if (method == null) return;

            TrackType(method.ReturnType);
            foreach (var param in method.Parameters)
            {
                TrackType(param.Type);
            }
        }

        /// <summary>
        /// Tracks a member symbol (field, property, method) and any attributes on it.
        /// </summary>
        /// <param name="member">The member symbol to track.</param>
        /// <remarks>
        /// This method tracks the member's type or signature, and also iterates over
        /// all attributes applied to the member, forwarding them to <see cref="TrackAttribute"/>.
        /// </remarks>
        public void TrackMember(ISymbol? member)
        {
            if (member == null) return;

            // Track the member's type or signature
            switch (member)
            {
                case IFieldSymbol field:
                    TrackType(field.Type);
                    break;
                case IPropertySymbol property:
                    TrackType(property.Type);
                    break;
                case IMethodSymbol method:
                    TrackMethod(method);
                    break;
            }

            // Track attributes applied to the member (e.g., validation attributes)
            foreach (var attr in member.GetAttributes())
            {
                TrackAttribute(attr);
            }
        }

        /// <summary>
        /// Tracks an attribute and its constructor/named arguments.
        /// </summary>
        /// <param name="attr">The attribute data to track.</param>
        /// <remarks>
        /// This method adds the namespace of the attribute class itself and any types
        /// appearing in constructor or named arguments. It also handles special
        /// validation attributes that require additional usings (e.g., regex, LINQ).
        /// </remarks>
        public void TrackAttribute(AttributeData? attr)
        {
            // Explicit null check to satisfy nullable analysis and avoid CS8602
            if (attr == null || attr.AttributeClass == null) return;

            // Track the attribute class itself
            TrackType(attr.AttributeClass);

            // Track constructor argument types
            foreach (var arg in attr.ConstructorArguments)
            {
                if (arg.Type != null && !ShouldExcludeNamespace(arg.Type.ContainingNamespace?.ToDisplayString()))
                {
                    TrackType(arg.Type);
                }
            }

            // Track named argument types
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Value.Type != null &&
                    !ShouldExcludeNamespace(namedArg.Value.Type.ContainingNamespace?.ToDisplayString()))
                {
                    TrackType(namedArg.Value.Type);
                }
            }

            // Handle special validation attributes that require extra usings
            if (attr.AttributeClass?.Name != null)
            {
                HandleSpecialValidationAttribute(attr);
            }
        }

        /// <summary>
        /// Tracks async-related features and adds required usings (e.g., System.Threading.Tasks).
        /// </summary>
        /// <param name="typeSymbol">The type symbol being processed.</param>
        /// <remarks>
        /// This method checks for:
        /// <list type="bullet">
        ///   <item>The <c>[FluentBuilderAsyncSupport]</c> attribute on the type.</item>
        ///   <item>Any async methods defined in the type.</item>
        ///   <item>The <c>[FluentValidateAsync]</c> attribute on buildable members.</item>
        /// </list>
        /// If any are found, <c>System.Threading.Tasks</c> is added as a required using.
        /// </remarks>
        public void TrackAsyncFeatures(INamedTypeSymbol typeSymbol)
        {
            // Check for [FluentBuilderAsyncSupport] attribute
            if (typeSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == Constant.AttributeName.FluentBuilderAsyncSupport))
            {
                AddRequiredUsing("System.Threading.Tasks");
            }

            // Check for async methods in the type
            if (typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(MethodCache.IsAsyncMethod))
            {
                AddRequiredUsing("System.Threading.Tasks");
            }

            // Check for [FluentValidateAsync] on buildable members
            var buildableMembers = TypeHelper.GetBuildableMembers(typeSymbol);
            foreach (var member in buildableMembers)
            {
                if (member.GetAttributes().Any(attr => attr.AttributeClass?.Name == Constant.AttributeName.FluentValidateAsync))
                {
                    AddRequiredUsing("System.Threading.Tasks");
                    break;
                }
            }
        }

        /// <summary>
        /// Adds using directives required by collection-specific options.
        /// </summary>
        /// <param name="member">The member to which the options apply.</param>
        /// <param name="options">The collection options for that member.</param>
        /// <remarks>
        /// If the options indicate that <c>AddRange</c>, <c>Count</c>, or count validation
        /// should be generated, <c>System.Collections.Generic</c> is added as a required using.
        /// </remarks>
        public void TrackCollectionHelpers(ISymbol member, CollectionOptions options)
        {
            if (options.GenerateAddRange || options.GenerateCount || options.HasCountValidation)
            {
                AddRequiredUsing("System.Collections.Generic");
            }
        }

        /// <summary>
        /// Determines whether a namespace should be excluded from using directives.
        /// </summary>
        /// <param name="ns">The namespace to check.</param>
        /// <returns><c>true</c> if the namespace is internal to the generator; otherwise <c>false</c>.</returns>
        private bool ShouldExcludeNamespace(string? ns)
        {
            if (string.IsNullOrEmpty(ns)) return false;

            foreach (var internalNs in InternalNamespaces)
            {
                if (ns == internalNs || ns!.StartsWith(internalNs + "."))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Handles special validation attributes that require additional using directives.
        /// </summary>
        /// <param name="attr">The attribute data.</param>
        /// <remarks>
        /// Based on the attribute name, this method adds the appropriate namespaces:
        /// <list type="bullet">
        ///   <item><see cref="Constant.AttributeName.FluentValidate"/> – may add <c>System.Text.RegularExpressions</c> if regex is used, or <c>System.Collections.Generic</c> if numeric validation is used.</item>
        ///   <item><see cref="Constant.AttributeName.FluentValidateOneOf"/> – adds <c>System.Linq</c>.</item>
        ///   <item><see cref="Constant.AttributeName.FluentValidateEmail"/>, <see cref="Constant.AttributeName.FluentValidatePhone"/>, <see cref="Constant.AttributeName.FluentValidateUrl"/> – add <c>System.Text.RegularExpressions</c>.</item>
        /// </list>
        /// </remarks>
        private void HandleSpecialValidationAttribute(AttributeData attr)
        {
            var attrName = attr.AttributeClass?.Name;
            switch (attrName)
            {
                case Constant.AttributeName.FluentValidate:
                    if (HasRegexValidation(attr))
                        AddRequiredUsing("System.Text.RegularExpressions");
                    if (HasNumericValidation(attr))
                        AddRequiredUsing("System.Collections.Generic");
                    break;

                case Constant.AttributeName.FluentValidateOneOf:
                    AddRequiredUsing("System.Linq");
                    break;

                case Constant.AttributeName.FluentValidateEmail:
                case Constant.AttributeName.FluentValidatePhone:
                case Constant.AttributeName.FluentValidateUrl:
                    AddRequiredUsing("System.Text.RegularExpressions");
                    break;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="Constant.AttributeName.FluentValidate"/> attribute includes a regex pattern.
        /// </summary>
        /// <param name="attr">The attribute data.</param>
        /// <returns><c>true</c> if the attribute has a named argument <c>RegexPattern</c> with a non-null value.</returns>
        private bool HasRegexValidation(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "RegexPattern" && namedArg.Value.Value != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether a <see cref="Constant.AttributeName.FluentValidate"/> attribute includes numeric range validation.
        /// </summary>
        /// <param name="attr">The attribute data.</param>
        /// <returns><c>true</c> if the attribute has <c>MinValue</c> or <c>MaxValue</c> arguments with values.</returns>
        private bool HasNumericValidation(AttributeData attr)
        {
            foreach (var namedArg in attr.NamedArguments)
            {
                if ((namedArg.Key == "MinValue" || namedArg.Key == "MaxValue") &&
                    namedArg.Value.Value != null)
                {
                    var value = Convert.ToDouble(namedArg.Value.Value);
                    if (value > double.MinValue && value < double.MaxValue)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Emits all collected using directives to the StringBuilder,
        /// sorted with System namespaces first.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the using directives to.</param>
        /// <remarks>
        /// This method combines the base required usings (<c>System</c> and <c>System.Collections.Generic</c>)
        /// with all tracked namespaces, filters out excluded namespaces, and sorts them so that
        /// all <c>System.*</c> namespaces appear before others.
        /// </remarks>
        public void EmitUsings(StringBuilder sb)
        {
            var allUsings = new HashSet<string>();

            // Add all required usings (base + explicitly added)
            foreach (var ns in _requiredUsings)
            {
                if (!ShouldExcludeNamespace(ns))
                    allUsings.Add(ns);
            }

            // Add tracked namespaces
            foreach (var ns in _namespacesToTrack)
            {
                if (!ShouldExcludeNamespace(ns))
                {
                    if (ns.StartsWith("System.") && allUsings.Contains("System"))
                        continue;
                    allUsings.Add(ns);
                }
            }

            // Separate System.* namespaces
            var systemUsings = new List<string>();
            var otherUsings = new List<string>();

            foreach (var ns in allUsings)
            {
                if (ns.StartsWith("System"))
                    systemUsings.Add(ns);
                else
                    otherUsings.Add(ns);
            }

            systemUsings.Sort();
            otherUsings.Sort();

            foreach (var ns in systemUsings)
                sb.AppendLine($"using {ns};");
            foreach (var ns in otherUsings)
                sb.AppendLine($"using {ns};");
        }

        /// <summary>
        /// Resets the manager to its initial state, ready for a new generation pass.
        /// </summary>
        /// <remarks>
        /// Clears all collected namespaces and re-adds the base required usings.
        /// This should be called before processing a new type to avoid cross‑contamination.
        /// </remarks>
        public void Reset()
        {
            _requiredUsings.Clear();
            _namespacesToTrack.Clear();
            _requiredUsings.Add("System");
            _requiredUsings.Add("System.Collections.Generic");
        }
    }
}