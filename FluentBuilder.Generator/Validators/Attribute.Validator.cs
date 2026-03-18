// <copyright file="Attribute.Validator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Base;
using FluentBuilder.Generator.Caching;
using FluentBuilder.Generator.Constants;
using FluentBuilder.Generator.Diagnostics;
using FluentBuilder.Generator.Helpers;
using FluentBuilder.Generator.Parameters;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FluentBuilder.Generator.Validators
{
    /// <summary>
    /// Validates various aspects of types and members during builder generation.
    /// </summary>
    public class AttributeValidator : DiagnosticReporterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValidator"/> class.
        /// </summary>
        /// <param name="context">The source production context used to report diagnostics.</param>
        public AttributeValidator(SourceProductionContext context) : base(context) { }

        /// <summary>
        /// Validates that a FluentName attribute on a symbol has a valid, non‑empty identifier.
        /// </summary>
        /// <param name="symbol">The symbol to validate.</param>
        /// <param name="symbolName">The name of the symbol (used in error messages).</param>
        /// <param name="location">The location in source code where the symbol is defined.</param>
        /// <returns>True if the FluentName attribute is valid or not present; otherwise false.</returns>
        public bool ValidateFluentName(ISymbol symbol, string symbolName, Location location)
        {
            if (symbol == null)
            {
                ReportError(Descriptor.InternalGeneratorError, location ?? Location.None, "unknown", "Symbol cannot be null");
                return false;
            }

            var nameAttr = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == Constant.AttributeName.FluentName);

            if (nameAttr == null)
                return true;

            var nameValue = nameAttr.ConstructorArguments.FirstOrDefault().Value as string;
            var safeLocation = location ?? Location.None;

            if (nameValue == null)
            {
                ReportError(Descriptor.EmptyFluentNameError, safeLocation, symbolName, "null");
                return false;
            }

            if (nameValue == string.Empty)
            {
                ReportError(Descriptor.EmptyFluentNameError, safeLocation, symbolName, "empty string");
                return false;
            }

            if (string.IsNullOrWhiteSpace(nameValue))
            {
                ReportError(Descriptor.EmptyFluentNameError, safeLocation, symbolName, "whitespace");
                return false;
            }

            if (!TypeHelper.IsValidIdentifier(nameValue))
            {
                ReportError(Descriptor.InvalidFluentNameIdentifierError, safeLocation, symbolName, nameValue);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a type is suitable for builder generation (not abstract, not static, not delegate/enum).
        /// </summary>
        /// <param name="typeSymbol">The type symbol to validate.</param>
        /// <returns>True if the type is valid; otherwise false.</returns>
        public bool ValidateBuilderType(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
            {
                ReportError(Descriptor.InternalGeneratorError, Location.None, "unknown", "Type symbol cannot be null");
                return false;
            }

            var safeLocation = GetLocation(typeSymbol);

            if (typeSymbol.TypeKind == TypeKind.Delegate || typeSymbol.TypeKind == TypeKind.Enum)
            {
                ReportError(Descriptor.UnsupportedTypeError, safeLocation, typeSymbol.Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that the type has at least one accessible constructor (public or internal) for builder instantiation.
        /// </summary>
        /// <param name="typeSymbol">The type symbol to validate.</param>
        /// <returns>True if an accessible constructor exists; otherwise false.</returns>
        public bool ValidateConstructorAccessibility(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
            {
                ReportError(Descriptor.InternalGeneratorError, Location.None, "unknown", "Type symbol cannot be null");
                return false;
            }

            if (typeSymbol.IsRecord) return true;

            var hasAccessibleConstructor = typeSymbol.Constructors
                .Any(c => TypeHelper.ShouldIncludeConstructor(c));

            if (!hasAccessibleConstructor)
            {
                var safeLocation = GetLocation(typeSymbol);
                ReportError(Descriptor.NoPublicConstructorError, safeLocation, typeSymbol.Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that no circular reference exists when generating builders for nested types.
        /// </summary>
        /// <param name="builderKey">The unique key of the builder being generated.</param>
        /// <param name="visitedBuilders">Set of builder keys already visited in the current generation chain.</param>
        /// <param name="typeSymbol">The type symbol being validated.</param>
        /// <returns>True if no circular reference is detected; otherwise false.</returns>
        public bool ValidateNoCircularReference(string builderKey, HashSet<string> visitedBuilders, INamedTypeSymbol typeSymbol)
        {
            if (string.IsNullOrEmpty(builderKey))
            {
                ReportError(Descriptor.InternalGeneratorError, GetLocation(typeSymbol),
                    typeSymbol?.Name ?? "unknown", "Builder key cannot be null or empty");
                return false;
            }

            if (visitedBuilders == null)
            {
                ReportError(Descriptor.InternalGeneratorError, GetLocation(typeSymbol),
                    typeSymbol?.Name ?? "unknown", "Visited builders set cannot be null");
                return false;
            }

            if (typeSymbol == null)
            {
                ReportError(Descriptor.InternalGeneratorError, Location.None, "unknown", "Type symbol cannot be null");
                return false;
            }

            if (visitedBuilders.Contains(builderKey))
            {
                var safeLocation = GetLocation(typeSymbol);
                ReportError(Descriptor.CircularReferenceError, safeLocation, typeSymbol.Name);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates that an async method has a return type that contains "Task".
        /// </summary>
        /// <param name="method">The method symbol to validate.</param>
        /// <returns>True if the method is a valid async method; otherwise false.</returns>
        public bool ValidateAsyncMethod(IMethodSymbol method)
        {
            if (method == null)
            {
                ReportError(Descriptor.InternalGeneratorError, Location.None, "unknown", "Method symbol cannot be null");
                return false;
            }

            var returnType = method.ReturnType.ToDisplayString();
            if (!returnType.Contains("Task"))
            {
                var safeLocation = GetLocation(method);
                ReportError(Descriptor.AsyncMethodSignatureError, safeLocation, method.Name);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates that an async validator type has a public parameterless constructor.
        /// </summary>
        /// <param name="validatorType">The validator type symbol.</param>
        /// <param name="location">The location in source code where the validator is used.</param>
        /// <returns>True if the validator is valid; otherwise false.</returns>
        public bool ValidateAsyncValidator(INamedTypeSymbol validatorType, Location location)
        {
            if (validatorType == null)
            {
                ReportError(Descriptor.InternalGeneratorError, location ?? Location.None, "unknown", "Validator type cannot be null");
                return false;
            }

            var hasParameterlessCtor = validatorType.Constructors.Any(c =>
                c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);

            if (!hasParameterlessCtor)
            {
                ReportError(Descriptor.AsyncValidatorMissingConstructor, location, validatorType.Name);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates that a builder namespace (if provided) consists of valid C# identifiers.
        /// </summary>
        /// <param name="namespace">The namespace string to validate.</param>
        /// <param name="location">The location in source code where the namespace is specified.</param>
        /// <returns>True if the namespace is valid or empty; otherwise false.</returns>
        public bool ValidateBuilderNamespace(string? @namespace, Location location)
        {
            if (string.IsNullOrEmpty(@namespace))
                return true;

            string[] parts = @namespace!.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!IdentifierCache.IsValidIdentifier(parts[i]))
                {
                    ReportError(Descriptor.InvalidBuilderNamespace, location, @namespace);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Validates that method prefix and suffix (if provided) are valid C# identifiers.
        /// </summary>
        /// <param name="prefix">The prefix string.</param>
        /// <param name="suffix">The suffix string.</param>
        /// <param name="location">The location in source code where the prefix/suffix are specified.</param>
        /// <returns>True if both are valid or empty; otherwise false.</returns>
        public bool ValidateMethodPrefixSuffix(string prefix, string suffix, Location location)
        {
            if (!string.IsNullOrEmpty(prefix) && !IdentifierCache.IsValidIdentifier(prefix))
            {
                ReportError(Descriptor.InvalidMethodPrefix, location, prefix);
                return false;
            }
            if (!string.IsNullOrEmpty(suffix) && !IdentifierCache.IsValidIdentifier(suffix))
            {
                ReportError(Descriptor.InvalidMethodSuffix, location, suffix);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates generic type parameters for builder compatibility (no variance, no unmanaged constraints, etc.).
        /// </summary>
        /// <param name="typeSymbol">The generic type symbol.</param>
        /// <param name="location">The location in source code where the type is used.</param>
        /// <returns>True if the generic type is supported; otherwise false.</returns>
        public bool ValidateGenericType(INamedTypeSymbol typeSymbol, Location location)
        {
            if (!typeSymbol.IsGenericType)
                return true;

            bool isValid = true;

            foreach (var typeParam in typeSymbol.TypeParameters)
            {
                if (typeParam.Variance != VarianceKind.None)
                {
                    string varianceStr = typeParam.Variance == VarianceKind.In ? "in" : "out";
                    ReportError(Descriptor.GenericTypeWithVariance, location, typeParam.Name, varianceStr);
                    isValid = false;
                }
            }

            if (typeSymbol.IsValueType)
            {
                ReportError(Descriptor.StructWithBuilder, location, typeSymbol.Name);
            }

            foreach (var typeParam in typeSymbol.TypeParameters)
            {
                if (HasUnmanagedConstraint(typeParam))
                {
                    ReportError(Descriptor.UnmanagedConstraint, location, typeParam.Name);
                }
            }

            return isValid;
        }

        private bool HasUnmanagedConstraint(ITypeParameterSymbol typeParam)
        {
            var hasUnmanagedProp = typeParam.GetType().GetProperty("HasUnmanagedTypeConstraint");
            if (hasUnmanagedProp != null)
            {
                return (bool)hasUnmanagedProp.GetValue(typeParam);
            }
            return false;
        }

        /// <summary>
        /// Validates that a specified factory method exists and is suitable.
        /// </summary>
        /// <param name="typeSymbol">The type that should contain the factory method.</param>
        /// <param name="config">The builder configuration that specifies the factory method name.</param>
        /// <returns>True if the factory method is valid (or not specified); otherwise false.</returns>
        public bool ValidateFactoryMethod(INamedTypeSymbol typeSymbol, BuilderConfiguration config)
        {
            if (string.IsNullOrEmpty(config.FactoryMethodName))
                return true;

            var methodName = config.FactoryMethodName;
            var methods = typeSymbol.GetMembers(methodName!).OfType<IMethodSymbol>()
                .Where(m => m.IsStatic &&
                            m.DeclaredAccessibility == Accessibility.Public &&
                            m.ReturnType.Equals(typeSymbol, SymbolEqualityComparer.Default) &&
                            m.Parameters.Length == 0)
                .ToList();

            if (methods.Count == 0)
            {
                ReportError(Descriptor.FactoryMethodNotFound,
                            GetLocation(typeSymbol),
                            methodName!, typeSymbol.Name);
                return false;
            }

            config.FactoryMethod = methods.First();
            config.HasFactoryMethod = true;
            return true;
        }

        /// <summary>
        /// Validates that a custom validation method has the correct signature (public, instance, no parameters, returns void/bool/Task/Task&lt;bool&gt;).
        /// </summary>
        /// <param name="method">The method symbol to validate.</param>
        /// <returns>True if the method is a valid custom validation method; otherwise false.</returns>
        public bool ValidateCustomValidationMethod(IMethodSymbol method)
        {
            if (method == null) return false;

            if (method.DeclaredAccessibility != Accessibility.Public || method.IsStatic)
            {
                ReportError(Descriptor.CustomValidationMethodInvalid, GetLocation(method), method.Name);
                return false;
            }

            if (method.Parameters.Length > 0)
            {
                ReportError(Descriptor.CustomValidationMethodInvalid, GetLocation(method), method.Name);
                return false;
            }

            var returnType = method.ReturnType;
            var returnTypeName = returnType.ToDisplayString();

            bool isValid = returnType.SpecialType == SpecialType.System_Void ||
                           returnType.SpecialType == SpecialType.System_Boolean ||
                           returnTypeName == "System.Threading.Tasks.Task" ||
                           returnTypeName == "System.Threading.Tasks.Task<bool>";

            if (!isValid)
            {
                ReportError(Descriptor.CustomValidationMethodInvalid, GetLocation(method), method.Name);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Warns about FluentName attributes placed on members that are not included in the builder (unused).
        /// </summary>
        /// <param name="typeSymbol">The type symbol whose members are examined.</param>
        public void ValidateUnusedFluentName(INamedTypeSymbol typeSymbol)
        {
            var allMembers = typeSymbol.GetMembers();
            foreach (var member in allMembers)
            {
                var fluentNameAttr = member.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == Constant.AttributeName.FluentName);
                if (fluentNameAttr == null)
                    continue;

                bool isIncluded = false;

                if (member is IFieldSymbol field)
                {
                    isIncluded = TypeHelper.ShouldIncludeMember(field);
                }
                else if (member is IPropertySymbol property)
                {
                    isIncluded = TypeHelper.ShouldIncludeMember(property);
                }
                else if (member is IMethodSymbol method)
                {
                    isIncluded = TypeHelper.ShouldIncludeMethod(method);
                }

                if (!isIncluded)
                {
                    ReportWarning(Descriptor.FluentNameUnused, GetLocation(member), member.Name);
                }
            }
        }

        /// <summary>
        /// Validates a third‑party validator specified via FluentValidateWith attribute.
        /// </summary>
        /// <param name="attr">The attribute data.</param>
        /// <param name="memberType">The type of the member being validated.</param>
        /// <param name="location">The location in source code where the attribute is applied.</param>
        /// <param name="validatorType">Returns the validator type symbol, or null if invalid.</param>
        /// <param name="methodName">Returns the validation method name, or null if invalid.</param>
        /// <returns>True if the validator is valid; otherwise false.</returns>
        public bool ValidateThirdPartyValidator(AttributeData attr, ITypeSymbol memberType, Location location, out ITypeSymbol? validatorType, out string? methodName)
        {
            validatorType = null;
            methodName = null;

            if (attr.ConstructorArguments.Length == 0)
            {
                ReportError(Descriptor.InternalGeneratorError, location, "unknown", "Missing constructor argument for FluentValidateWith");
                return false;
            }

            validatorType = attr.ConstructorArguments[0].Value as ITypeSymbol;
            if (validatorType == null)
            {
                ReportError(Descriptor.InternalGeneratorError, location, "unknown", "Validator type could not be resolved");
                return false;
            }

            methodName = AttributeCache.GetNamedArgument<string>(attr, "MethodName") ?? "Validate";

            if (validatorType is INamedTypeSymbol namedValidator)
            {
                var hasCtor = namedValidator.Constructors.Any(c =>
                    c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
                if (!hasCtor)
                {
                    ReportError(Descriptor.ThirdPartyValidatorMissingConstructor, location, namedValidator.Name);
                    return false;
                }
            }
            else
            {
                ReportError(Descriptor.InternalGeneratorError, location, "unknown", "Validator type is not a named type");
                return false;
            }

            var methods = validatorType.GetMembers(methodName).OfType<IMethodSymbol>()
                .Where(m => !m.IsStatic &&
                            m.DeclaredAccessibility == Accessibility.Public &&
                            m.Parameters.Length == 1 &&
                            SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, memberType) &&
                            m.ReturnType.SpecialType == SpecialType.System_Boolean)
                .ToList();

            if (methods.Count == 0)
            {
                ReportError(Descriptor.ThirdPartyValidatorMethodNotFound, location, methodName, validatorType.Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates and adjusts builder accessibility for file-scoped types.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <param name="config">The builder configuration to update.</param>
        /// <param name="location">The location in source code where the type is defined.</param>
        public void ValidateFileScopedBuilder(INamedTypeSymbol typeSymbol, BuilderConfiguration config, Location location)
        {
            if (TypeHelper.IsFileScoped(typeSymbol))
            {
                if (!string.IsNullOrEmpty(config.BuilderAccessibility) && config.BuilderAccessibility != "file")
                {
                    ReportWarning(Descriptor.FileScopedAccessibilityIgnored, location, config.BuilderAccessibility);
                }
                config.BuilderAccessibility = "file";
            }
        }
    }
}
