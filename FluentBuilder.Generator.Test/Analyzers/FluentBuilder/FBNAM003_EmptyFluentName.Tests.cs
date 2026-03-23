using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBNAM003_EmptyFluentNameTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new EmptyFluentNameAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == EmptyFluentNameAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Empty/whitespace on type (builder rename) =====
        [TestMethod]
        public void TypeWithEmptyString_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    [FluentName("""")]
    public class MyClass { }
}", true);

        [TestMethod]
        public void TypeWithWhitespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    [FluentName(""   "")]
    public class MyClass { }
}", true);

        [TestMethod]
        public void TypeWithValidName_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    [FluentName(""ValidBuilder"")]
    public class MyClass { }
}", false);

        // ===== Empty/whitespace on property =====
        [TestMethod]
        public void PropertyWithEmptyString_ReportsDiagnostic() =>
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
        public void PropertyWithWhitespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""   "")]
        public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void PropertyWithValidName_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""SetProp"")]
        public int Prop { get; set; }
    }
}", false);

        // ===== Empty/whitespace on field =====
        [TestMethod]
        public void FieldWithEmptyString_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName("""")]
        public int field;
    }
}", true);

        // ===== Empty/whitespace on method =====
        [TestMethod]
        public void MethodWithEmptyString_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName("""")]
        public void DoSomething() { }
    }
}", true);

        // ===== Multiple attributes, only empty reported =====
        [TestMethod]
        public void MixedAttributes_OnlyEmptyReported()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Valid"")]
        public int Prop1 { get; set; }
        [FluentName("""")]
        public int Prop2 { get; set; }
        [FluentName(""   "")]
        public int Prop3 { get; set; }
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationErrors.Where(d => d.Id == EmptyFluentNameAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(2, diagnostics.Count, "Should report Prop2 and Prop3");
        }

        // ===== Ignored members are still checked? The attribute is on the member, so we should still warn even if member is ignored.
        // The [FluentIgnore] only prevents builder method generation; the attribute itself is still invalid.
        [TestMethod]
        public void IgnoredPropertyWithEmptyFluentName_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentIgnore]
        [FluentName("""")]
        public int Prop { get; set; }
    }
}", true);

        // ===== Nested types =====
        [TestMethod]
        public void NestedClassWithEmptyFluentName_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder]
        [FluentName("""")]
        public class Nested { }
    }
}", true);
    }
}
