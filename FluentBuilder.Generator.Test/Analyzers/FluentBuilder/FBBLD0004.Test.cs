// <copyright file="FBBLD0004.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0004.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [TestClass]
    public class FBBLD0004
    {
        // ================================================================================
        // Target: public (top-level)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToPublic_Allowed() => RunTest("Public", "Public", false);
        [TestMethod] public void BuilderAccessibilityInternalToPublic_Allowed() => RunTest("Internal", "Public", false);
        [TestMethod] public void BuilderAccessibilityPrivateToPublic_Allowed() => RunTest("Private", "Public", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToPublic_Allowed() => RunTest("Protected", "Public", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToPublic_Allowed() => RunTest("ProtectedInternal", "Public", false, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToPublic_Allowed() => RunTest("PrivateProtected", "Public", false, true, true);
        // CHANGED: File builder is allowed on top-level public target (accessibility rules allow it)
        [TestMethod] public void BuilderAccessibilityFileToPublic_Allowed() => RunTest("File", "Public", false, false, true);

        // ================================================================================
        // Target: internal (top-level)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToInternal_Disallowed() => RunTest("Public", "Internal", true);
        [TestMethod] public void BuilderAccessibilityInternalToInternal_Allowed() => RunTest("Internal", "Internal", false);
        [TestMethod] public void BuilderAccessibilityPrivateToInternal_Allowed() => RunTest("Private", "Internal", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToInternal_Disallowed() => RunTest("Protected", "Internal", true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToInternal_Disallowed() => RunTest("ProtectedInternal", "Internal", true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToInternal_Allowed() => RunTest("PrivateProtected", "Internal", false, true, true);
        // CHANGED: File builder is allowed on top-level internal target (accessibility rules allow it)
        [TestMethod] public void BuilderAccessibilityFileToInternal_Allowed() => RunTest("File", "Internal", false, false, true);

        // ================================================================================
        // Target: file (top-level, C#11+)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToFile_Disallowed() => RunTest("Public", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityInternalToFile_Disallowed() => RunTest("Internal", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityPrivateToFile_Disallowed() => RunTest("Private", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToFile_Disallowed() => RunTest("Protected", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToFile_Disallowed() => RunTest("ProtectedInternal", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToFile_Disallowed() => RunTest("PrivateProtected", "File", true, false, true);
        [TestMethod] public void BuilderAccessibilityFileToFile_Disallowed() => RunTest("File", "File", true, false, true);

        // ================================================================================
        // Target: private (nested)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToPrivate_Disallowed() => RunTest("Public", "Private", true, true);
        [TestMethod] public void BuilderAccessibilityInternalToPrivate_Disallowed() => RunTest("Internal", "Private", true, true);
        [TestMethod] public void BuilderAccessibilityPrivateToPrivate_Allowed() => RunTest("Private", "Private", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToPrivate_Disallowed() => RunTest("Protected", "Private", true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToPrivate_Disallowed() => RunTest("ProtectedInternal", "Private", true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToPrivate_Disallowed() => RunTest("PrivateProtected", "Private", true, true, true);
        [TestMethod] public void BuilderAccessibilityFileToPrivate_Disallowed() => RunTest("File", "Private", true, true, true);

        // ================================================================================
        // Target: protected (nested)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToProtected_Disallowed() => RunTest("Public", "Protected", true, true);
        [TestMethod] public void BuilderAccessibilityInternalToProtected_Disallowed() => RunTest("Internal", "Protected", true, true);
        [TestMethod] public void BuilderAccessibilityPrivateToProtected_Allowed() => RunTest("Private", "Protected", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToProtected_Allowed() => RunTest("Protected", "Protected", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToProtected_Disallowed() => RunTest("ProtectedInternal", "Protected", true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToProtected_Allowed() => RunTest("PrivateProtected", "Protected", false, true, true);
        [TestMethod] public void BuilderAccessibilityFileToProtected_Disallowed() => RunTest("File", "Protected", true, true, true);

        // ================================================================================
        // Target: protected internal (nested)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToProtectedInternal_Disallowed() => RunTest("Public", "ProtectedInternal", true, true);
        [TestMethod] public void BuilderAccessibilityInternalToProtectedInternal_Allowed() => RunTest("Internal", "ProtectedInternal", false, true);
        [TestMethod] public void BuilderAccessibilityPrivateToProtectedInternal_Allowed() => RunTest("Private", "ProtectedInternal", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedToProtectedInternal_Allowed() => RunTest("Protected", "ProtectedInternal", false, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToProtectedInternal_Allowed() => RunTest("ProtectedInternal", "ProtectedInternal", false, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToProtectedInternal_Allowed() => RunTest("PrivateProtected", "ProtectedInternal", false, true, true);
        [TestMethod] public void BuilderAccessibilityFileToProtectedInternal_Disallowed() => RunTest("File", "ProtectedInternal", true, true, true);

        // ================================================================================
        // Target: private protected (nested)
        // ================================================================================

        [TestMethod] public void BuilderAccessibilityPublicToPrivateProtected_Disallowed() => RunTest("Public", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityInternalToPrivateProtected_Disallowed() => RunTest("Internal", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateToPrivateProtected_Allowed() => RunTest("Private", "PrivateProtected", false, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedToPrivateProtected_Disallowed() => RunTest("Protected", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityProtectedInternalToPrivateProtected_Disallowed() => RunTest("ProtectedInternal", "PrivateProtected", true, true, true);
        [TestMethod] public void BuilderAccessibilityPrivateProtectedToPrivateProtected_Allowed() => RunTest("PrivateProtected", "PrivateProtected", false, true, true);
        [TestMethod] public void BuilderAccessibilityFileToPrivateProtected_Disallowed() => RunTest("File", "PrivateProtected", true, true, true);

        // --------------------------------------------------------------------------------
        // Helper method to build source and run test
        // --------------------------------------------------------------------------------
        private void RunTest(
            string builderAccessibility,
            string targetAccessibility,
            bool expectError,
            bool targetNeedsWrapper = false,
            bool useLatestLang = false)
        {
            string source;

            // Map accessibility name to proper C# keyword
            string keyword = GetKeyword(targetAccessibility);

            if (targetNeedsWrapper)
            {
                // Target is nested inside a public container.
                // file‑local types cannot be nested; guard against misuse.
                if (targetAccessibility == "File")
                    throw new InvalidOperationException("file‑local types cannot be nested; targetNeedsWrapper must be false.");

                source = $@"
using FluentBuilder;

namespace Demo
{{
    public partial class Container
    {{
        [FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builderAccessibility})]
        {keyword} class Order
        {{
        }}
    }}
}}";
            }
            else
            {
                // Target is top‑level.
                source = $@"
using FluentBuilder;

namespace Demo
{{
    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builderAccessibility})]
    {keyword} class Order
    {{
    }}
}}";
            }

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BuilderAccessibilityVsTargetTypeAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(
                inputSource: source,
                analyzers: analyzers,
                langVersion: useLatestLang ? "latest" : null);

            var actualError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == "FBBLD0004");

            if (expectError)
            {
                Assert.IsNotNull(actualError, $"Expected FBBLD0004 error but none reported for:\n{source}");
            }
            else
            {
                Assert.IsNull(actualError, $"FBBLD0004 error was reported unexpectedly for:\n{source}");
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
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility), $"Unknown accessibility: {accessibility}")
            };
        }
    }
}
