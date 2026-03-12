// <copyright file="FBBLD0001.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0001.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [TestClass]
    public class FBBLD0001
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new FluentBuilderAnalyzer());

        [TestMethod]
        public void BuilderAccessibility_NotSpecified_Allowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder]
                    public class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var hasError = result.CompilationErrors.Any(d => d.Id == "FBBLD0001");
            Assert.IsFalse(hasError, "Expected no FBBLD0001 error, but one was reported.");
        }

        [TestMethod]
        public void BuilderAccessibility_Public_Allowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Public)]
                    public class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var hasError = result.CompilationErrors.Any(d => d.Id == "FBBLD0001");
            Assert.IsFalse(hasError, "Expected no FBBLD0001 error for Public accessibility.");
        }

        [TestMethod]
        public void BuilderAccessibility_Internal_Allowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Internal)]
                    public class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var hasError = result.CompilationErrors.Any(d => d.Id == "FBBLD0001");
            Assert.IsFalse(hasError, "Expected no FBBLD0001 error for Internal accessibility.");
        }

        [TestMethod]
        public void BuilderAccessibility_Private_Disallowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Private)]
                    public class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var error = result.CompilationErrors.FirstOrDefault(d => d.Id == "FBBLD0001");
            Assert.IsNotNull(error, "Expected FBBLD0001 error for Private accessibility, but none was reported.");
        }

        [TestMethod]
        public void BuilderAccessibility_Protected_Disallowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Protected)]
                    public class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var error = result.CompilationErrors.FirstOrDefault(d => d.Id == "FBBLD0001");
            Assert.IsNotNull(error, "Expected FBBLD0001 error for Protected accessibility, but none was reported.");
        }

        [TestMethod]
        public void BuilderAccessibility_File_Disallowed()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.File)]
                    public class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var error = result.CompilationErrors.FirstOrDefault(d => d.Id == "FBBLD0001");
            Assert.IsNotNull(error, "Expected FBBLD0001 error for File accessibility, but none was reported.");
        }

     public void TopLevelRecord_PrivateAccessibility_ReportsDiagnostic()
        {
            var inputSource = """
                using FluentBuilder;

                namespace Demo
                {
                    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Private)]
                    public record MyRecord;
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var error = result.CompilationErrors.FirstOrDefault(d => d.Id == "FBBLD0001");
            Assert.IsNotNull(error, "Expected FBBLD0001 error for record with Private accessibility.");
        }

        [TestMethod]
        public void BuilderAccessibility_NoAttribute_Allowed()
        {
            var inputSource = """
                namespace Demo
                {
                    internal class Order { }
                }
                """;

            var result = BaseCode.Action.RunGeneratorAndCompile(inputSource, Analyzers, "13.0");
            var hasError = result.CompilationErrors.Any(d => d.Id == "FBBLD0001");
            Assert.IsFalse(hasError, "Expected no FBBLD0001 error when no attribute is present.");
        }
    }
}
