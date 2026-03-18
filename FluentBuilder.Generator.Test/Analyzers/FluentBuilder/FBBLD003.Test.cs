// <copyright file="FBBLD0003.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0003.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD003
    {
        [TestMethod]
        public void BuilderAccessibilityPrivateProtected_LangVerBelow7_2_Disallowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    public class Container
                    {
                        [FluentBuilder(BuilderAccessibility = BuilderAccessibility.PrivateProtected)]
                        public class Order { }
                    }
                }
                """;

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new FluentBuilderPrivateProtectedAccessibilityAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, analyzers, "7.0");

            var analyzerError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == FluentBuilderPrivateProtectedAccessibilityAnalyzer.DiagnosticId);

            Assert.IsNotNull(analyzerError, "Expected FBBLD003 error was not reported.");
        }

        [TestMethod]
        public void BuilderAccessibilityPrivateProtected_LangVer7_2_OrHigher_Allowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    public class Container
                    {
                        [FluentBuilder(BuilderAccessibility = BuilderAccessibility.PrivateProtected)]
                        public class Order { }
                    }
                }
                """;

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new FluentBuilderPrivateProtectedAccessibilityAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, analyzers, "7.2");

            var analyzerError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == "FBBLD0003");

            Assert.IsNull(analyzerError, "FBBLD0003 error was reported unexpectedly.");
        }
    }
}
