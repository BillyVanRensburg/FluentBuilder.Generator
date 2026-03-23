using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD018_ReadOnlyPropertyTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new ReadOnlyPropertyAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationWarnings.Any(d => d.Id == ReadOnlyPropertyAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Properties that should trigger warning =====
        [TestMethod]
        public void ReadOnlyProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; }
    }
}", true);

        [TestMethod]
        public void InitOnlyProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; init; }
    }
}", true); // init-only is still read-only after construction, builder cannot set it via property setter.

        [TestMethod]
        public void ExpressionBodyProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop => 42;
    }
}", true);

        // ===== Properties that should NOT trigger warning =====
        [TestMethod]
        public void PropertyWithSetter_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void PropertyWithPrivateSetter_NoWarningFromThisAnalyzer() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; private set; }
    }
}", false); // FBBLD017 handles private setter

        [TestMethod]
        public void IgnoredReadOnlyProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentIgnore]
        public int Prop { get; }
    }
}", false);

        [TestMethod]
        public void NonPublicProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        internal int Prop { get; }
        private int Prop2 { get; }
    }
}", false);

        [TestMethod]
        public void StaticProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public static int Prop { get; }
    }
}", false);

        // ===== Type without builder attribute =====
        [TestMethod]
        public void ReadOnlyPropertyInTypeWithoutBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class MyClass {
        public int Prop { get; }
    }
}", false);

        // ===== Record with primary constructor =====
        [TestMethod]
        public void RecordReadOnlyProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public record MyRecord {
        public int Prop { get; }
    }
}", true);

        // ===== Nested type =====
        [TestMethod]
        public void NestedReadOnlyProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder]
        public class Nested {
            public int Prop { get; }
        }
    }
}", true);

        // ===== Multiple properties – only read-only reported =====
        [TestMethod]
        public void MixedProperties_OnlyReadOnlyReported()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int ReadOnlyProp { get; }
        public int WritableProp { get; set; }
        public int InitOnlyProp { get; init; }
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationWarnings.Where(d => d.Id == ReadOnlyPropertyAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(2, diagnostics.Count, "Should report ReadOnlyProp and InitOnlyProp");
        }
    }
}
