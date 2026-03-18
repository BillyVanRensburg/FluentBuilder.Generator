// <copyright file="FBBLD0006.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0006.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD0006
    {
        // ================================================================================
        // Top‑level target – conflict in same namespace
        // ================================================================================

        [TestMethod]
        public void TopLevel_ConflictInNamespace_Error()
        {
            var source = @"
using FluentBuilder;

namespace Demo
{
    public class OrderBuilder { } // conflicting type

    [FluentBuilder]
    public class Order { }
}";
            RunTest(source, expectError: true);
        }

        // ================================================================================
        // Top‑level target – custom builder name via FluentName, conflict with that name
        // ================================================================================

        [TestMethod]
        public void TopLevel_CustomNameConflict_Error()
        {
            var source = @"
using FluentBuilder;

namespace Demo
{
    public class CustomOrderBuilder { } // conflicting type

    [FluentName(""CustomOrderBuilder"")]
    [FluentBuilder]
    public class Order { }
}";
            RunTest(source, expectError: true);
        }

        // ================================================================================
        // Top‑level target – custom namespace via BuilderNamespace, conflict in that namespace
        // ================================================================================

        [TestMethod]
        public void TopLevel_CustomNamespaceConflict_Error()
        {
            var source = @"
using FluentBuilder;

namespace Demo
{
    [FluentBuilder(BuilderNamespace = ""Other"")]
    public class Order { }
}

namespace Other
{
    public class OrderBuilder { } // conflicting type
}";
            RunTest(source, expectError: true);
        }

        // ================================================================================
        // Nested target – conflict with existing nested type in same container
        // ================================================================================

        [TestMethod]
        public void Nested_ConflictInContainer_Error()
        {
            var source = @"
using FluentBuilder;

namespace Demo
{
    public partial class Container
    {
        public class OrderBuilder { } // conflicting type

        [FluentBuilder]
        public class Order { }
    }
}";
            RunTest(source, expectError: true);
        }

        // ================================================================================
        // Nested target – custom builder name via FluentName, conflict with that name in same container
        // ================================================================================

        [TestMethod]
        public void Nested_CustomNameConflict_Error()
        {
            var source = @"
using FluentBuilder;

namespace Demo
{
    public partial class Container
    {
        public class CustomOrderBuilder { } // conflicting type

        [FluentName(""CustomOrderBuilder"")]
        [FluentBuilder]
        public class Order { }
    }
}";
            RunTest(source, expectError: true);
        }

        // ================================================================================
        // Helper method to run a test
        // ================================================================================

        private void RunTest(string source, bool expectError)
        {
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
                new BuilderNameConflictAnalyzer());

            var result = BaseCode.Action.RunGeneratorAndCompile(
                inputSource: source,
                analyzers: analyzers,
                langVersion: "latest");

            var actualError = result.CompilationErrors
                .FirstOrDefault(d => d.Id == "FBBLD0006");

            if (expectError)
            {
                Assert.IsNotNull(actualError, $"Expected FBBLD0006 error but none reported for:\n{source}");
            }
            else
            {
                Assert.IsNull(actualError, $"FBBLD0006 error was reported unexpectedly for:\n{source}");
            }
        }
    }
}
