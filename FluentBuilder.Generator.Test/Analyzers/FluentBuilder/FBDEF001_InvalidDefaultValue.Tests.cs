using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBDEF001_InvalidDefaultValueTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidDefaultValueAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == InvalidDefaultValueAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // Valid assignments
        [TestMethod]
        public void IntDefault_OnIntProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(42)]
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void StringDefault_OnStringProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(""Hello"")]
        public string Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void NullDefault_OnNullableIntProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(null)]
        public int? Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void IntDefault_OnLongProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(42)]
        public long Prop { get; set; }
    }
}", false); // int to long implicit conversion allowed

        [TestMethod]
        public void EnumDefault_OnEnumProperty_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public enum Color { Red, Green, Blue }
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(Color.Red)]
        public Color Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void NullDefault_OnReferenceType_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(null)]
        public string Prop { get; set; }
    }
}", false);

        // Invalid assignments
        [TestMethod]
        public void StringDefault_OnIntProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(""abc"")]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void IntDefault_OnStringProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(123)]
        public string Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void NullDefault_OnValueType_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(null)]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void DoubleDefault_OnIntProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(3.14)]
        public int Prop { get; set; }
    }
}", true); // double to int requires explicit conversion, not implicit

        [TestMethod]
        public void WrongEnumValue_OnEnumProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public enum Color { Red, Green, Blue }
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(42)]
        public Color Prop { get; set; }
    }
}", true); // int to enum requires explicit cast

        [TestMethod]
        public void UnrelatedType_OnProperty_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
using System;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentDefaultValue(new DateTime())]
        public int Prop { get; set; }
    }
}", true);
    }
}
