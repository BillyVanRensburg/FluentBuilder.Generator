// <copyright file="FBBLD0005.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0005.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [TestClass]
    public class FBBLD0005
    {
        // ================================================================================
        // Container: public
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerPublic_Allowed() => RunTest("Public", "Public", false);
        [TestMethod] public void BuilderAccessibilityInternalToContainerPublic_Allowed() => RunTest("Internal", "Public", false);
        [TestMethod] public void BuilderAccessibilityPrivateToContainerPublic_Allowed() => RunTest("Private", "Public", false);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerPublic_Allowed() => RunTest("Protected", "Public", false);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerPublic_Allowed() => RunTest("ProtectedInternal", "Public", false);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerPublic_Allowed() => RunTest("PrivateProtected", "Public", false, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerPublic_Disallowed() => RunTest("File", "Public", true);

        // ================================================================================
        // Container: internal
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerInternal_Disallowed() => RunTest("Public", "Internal", true);
        [TestMethod] public void BuilderAccessibilityInternalToContainerInternal_Allowed() => RunTest("Internal", "Internal", false);
        [TestMethod] public void BuilderAccessibilityPrivateToContainerInternal_Allowed() => RunTest("Private", "Internal", false);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerInternal_Allowed() => RunTest("Protected", "Internal", false);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerInternal_Disallowed() => RunTest("ProtectedInternal", "Internal", true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerInternal_Allowed() => RunTest("PrivateProtected", "Internal", false, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerInternal_Disallowed() => RunTest("File", "Internal", true);

        // ================================================================================
        // Container: file
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerFile_Disallowed() => RunTest("Public", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityInternalToContainerFile_Disallowed() => RunTest("Internal", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityPrivateToContainerFile_Disallowed() => RunTest("Private", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerFile_Disallowed() => RunTest("Protected", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerFile_Disallowed() => RunTest("ProtectedInternal", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerFile_Disallowed() => RunTest("PrivateProtected", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerFile_Disallowed() => RunTest("File", "File", true, false, true);

        // ================================================================================
        // Container: private (must be nested inside a public container)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerPrivate_Disallowed() => RunTest("Public", "Private", true, true, true);
        [TestMethod] public void BuilderAccessibilityInternalToContainerPrivate_Disallowed() => RunTest("Internal", "Private", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateToContainerPrivate_Allowed() => RunTest("Private", "Private", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerPrivate_Disallowed() => RunTest("Protected", "Private", true, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerPrivate_Disallowed() => RunTest("ProtectedInternal", "Private", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerPrivate_Disallowed() => RunTest("PrivateProtected", "Private", true, true, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerPrivate_Disallowed() => RunTest("File", "Private", true, true, true);

        // ================================================================================
        // Container: protected (must be nested inside a public container)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerProtected_Disallowed() => RunTest("Public", "Protected", true, true, true);
        [TestMethod] public void BuilderAccessibilityInternalToContainerProtected_Disallowed() => RunTest("Internal", "Protected", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateToContainerProtected_Allowed() => RunTest("Private", "Protected", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerProtected_Allowed() => RunTest("Protected", "Protected", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerProtected_Disallowed() => RunTest("ProtectedInternal", "Protected", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerProtected_Allowed() => RunTest("PrivateProtected", "Protected", false, true, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerProtected_Disallowed() => RunTest("File", "Protected", true, true, true);

        // ================================================================================
        // Container: protected internal (must be nested inside a public container)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerProtectedInternal_Disallowed() => RunTest("Public", "ProtectedInternal", true, true, true);
        [TestMethod] public void BuilderAccessibilityInternalToContainerProtectedInternal_Allowed() => RunTest("Internal", "ProtectedInternal", false, true, true); // CHANGED
        [TestMethod] public void BuilderAccessibilityPrivateToContainerProtectedInternal_Allowed() => RunTest("Private", "ProtectedInternal", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerProtectedInternal_Allowed() => RunTest("Protected", "ProtectedInternal", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerProtectedInternal_Allowed() => RunTest("ProtectedInternal", "ProtectedInternal", false, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerProtectedInternal_Allowed() => RunTest("PrivateProtected", "ProtectedInternal", false, true, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerProtectedInternal_Disallowed() => RunTest("File", "ProtectedInternal", true, true, true);

        // ================================================================================
        // Container: private protected (must be nested inside a public container)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToContainerPrivateProtected_Disallowed() => RunTest("Public", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityInternalToContainerPrivateProtected_Disallowed() => RunTest("Internal", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateToContainerPrivateProtected_Allowed() => RunTest("Private", "PrivateProtected", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedToContainerPrivateProtected_Disallowed() => RunTest("Protected", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToContainerPrivateProtected_Disallowed() => RunTest("ProtectedInternal", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToContainerPrivateProtected_Allowed() => RunTest("PrivateProtected", "PrivateProtected", false, true, true);
        [TestMethod] public void BuilderAccessibilityFileToContainerPrivateProtected_Disallowed() => RunTest("File", "PrivateProtected", true, true, true);

        // --------------------------------------------------------------------------------
        // Helper method to build source and run test
        // --------------------------------------------------------------------------------
        private void RunTest(
            string builderAccessibility,
            string containerAccessibility,
            bool expectError,
            bool containerNeedsWrapper = false,
            bool useLatestLang = false)
        {
            // Convert the container accessibility string to a valid C# keyword
            string containerKeyword = GetKeyword(containerAccessibility);

            string source;

            if (containerNeedsWrapper)
            {
                source = $@"
using FluentBuilder;

namespace Demo
{{
    public partial class Outer
    {{
        {containerKeyword} partial class Container
        {{
            [FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builderAccessibility})]
            public class Target {{ }}
        }}
    }}
}}";
            }
            else
            {
                source = $@"
using FluentBuilder;

namespace Demo
{{
    {containerKeyword} partial class Container
    {{
        [FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builderAccessibility})]
        public class Target {{ }}
    }}
}}";
            }

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BuilderAccessibilityVsContainerAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(
                inputSource: source,
                analyzers: analyzers,
                langVersion: useLatestLang ? "latest" : null);

            var actualError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == "FBBLD0005");

            if (expectError)
            {
                Assert.IsNotNull(actualError, $"Expected FBBLD0005 error but none reported for:\n{source}");
            }
            else
            {
                Assert.IsNull(actualError, $"FBBLD0005 error was reported unexpectedly for:\n{source}");
                // Also ensure no other compilation errors (e.g., from the compiler itself)
                Assert.AreEqual(0, result.CompilationErrors.Count,
                    $"Unexpected compilation errors: {string.Join(", ", result.CompilationErrors.Select(e => e.GetMessage()))}");
            }
        }

        private static string GetKeyword(string accessibility)
        {
            return accessibility switch
            {
                "Public" => "public",
                "Internal" => "internal",
                "Private" => "private",
                "Protected" => "protected",
                "ProtectedInternal" => "protected internal",
                "PrivateProtected" => "private protected",
                "File" => "file",
                _ => throw new System.ArgumentOutOfRangeException(nameof(accessibility), $"Unknown accessibility: {accessibility}")
            };
        }
    }
}
