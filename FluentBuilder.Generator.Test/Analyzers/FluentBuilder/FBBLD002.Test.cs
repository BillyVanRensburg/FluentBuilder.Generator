// <copyright file="FBBLD0002.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0002.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD002
    {
        [TestMethod]
        public void BuilderAccessibilityFile_LangVerBelow11_Disallowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.File)]
                    public class Order { }
                }
                """;

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new FluentBuilderFileAccessibilityAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, analyzers, "10.0");

            var analyzerError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == FluentBuilderFileAccessibilityAnalyzer.DiagnosticId);

            Assert.IsNotNull(analyzerError, "Expected FBBLD002 error was not reported.");
        }

        [TestMethod]
        public void BuilderAccessibilityFile_LangVer11OrHigher_Allowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.File)]
                    public class Order { }
                }
                """;

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new FluentBuilderFileAccessibilityAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, analyzers, "11.0");

            var analyzerError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == "FBBLD0002");

            Assert.IsNull(analyzerError, "FBBLD0002 error was reported unexpectedly.");
        }
    }
}
