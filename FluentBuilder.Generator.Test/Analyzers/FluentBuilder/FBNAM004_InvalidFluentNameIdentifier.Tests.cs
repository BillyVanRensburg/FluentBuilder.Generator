using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBNAM004_InvalidFluentNameIdentifierTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidFluentNameIdentifierAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == InvalidFluentNameIdentifierAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Valid identifiers =====
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
        public void KeywordWithAtPrefix_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""@class"")]
        public int Prop { get; set; }
    }
}", false);

        // ===== Invalid identifiers =====
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
        public void KeywordWithoutAt_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""class"")]
        public int Prop { get; set; }
    }
}", true);

        // ===== Mixed attributes (only invalid reported) =====
        [TestMethod]
        public void MixedAttributes_OnlyInvalidReported()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Valid"")]
        public int Prop1 { get; set; }
        [FluentName(""Invalid Name"")]
        public int Prop2 { get; set; }
        [FluentName(""@class"")]
        public int Prop3 { get; set; }
        [FluentName("""")]
        public int Prop4 { get; set; }
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationErrors.Where(d => d.Id == InvalidFluentNameIdentifierAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(1, diagnostics.Count, "Should report only Prop2");
            Assert.IsTrue(diagnostics[0].GetMessage().Contains("Invalid Name"), "Diagnostic should mention 'Invalid Name'");
        }

        // ===== Empty/whitespace are skipped =====
        [TestMethod]
        public void EmptyString_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName("""")]
        public int Prop { get; set; }
    }
}", false); // Handled by FBNAM003

        [TestMethod]
        public void WhitespaceString_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""   "")]
        public int Prop { get; set; }
    }
}", false);
    }
}
