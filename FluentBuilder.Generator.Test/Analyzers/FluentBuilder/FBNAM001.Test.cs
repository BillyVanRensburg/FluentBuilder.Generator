using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBNAM001_InvalidFluentNameTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidFluentNameAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == InvalidFluentNameAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Valid names =====
        [TestMethod]
        public void ValidIdentifier_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""ValidName"")]
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void ValidIdentifierWithUnderscore_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""_validName"")]
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void ValidIdentifierWithDigits_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""name123"")]
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void KeywordWithAtPrefix_ReportDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""@class"")]
        public int Prop { get; set; }
    }
}", true);

        // ===== Invalid names =====
        [TestMethod]
        public void NameWithSpace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Invalid Name"")]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void StartsWithDigit_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""1invalid"")]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void ContainsSpecialCharacters_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""name$%"")]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void KeywordWithoutAt_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""class"")]
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void EmptyString_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName("""")]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void NullName_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(null)]
        public int Prop { get; set; }
    }
}", false);

        // ===== FluentName on method =====
        [TestMethod]
        public void MethodWithInvalidFluentName_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Invalid Name"")]
        public void DoSomething() { }
    }
}", true);

        // ===== FluentName on field =====
        [TestMethod]
        public void FieldWithInvalidFluentName_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Invalid Name"")]
        public int field;
    }
}", true);

        // ===== Type without FluentBuilder attribute – FluentName ignored, no diagnostic =====
        [TestMethod]
        public void MemberWithoutBuilderAttribute_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class MyClass {
        [FluentName(""Invalid Name"")]
        public int Prop { get; set; }
    }
}", false);
    }
}
