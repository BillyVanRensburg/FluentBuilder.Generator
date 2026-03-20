using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD016_DelegateBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new DelegateBuilderAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == DelegateBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        [TestMethod]
        public void Delegate_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public delegate void MyDelegate(); }", true);

        [TestMethod]
        public void Delegate_WithoutFluentBuilder_NoDiagnostic() =>
            RunTest(@"
namespace Test { public delegate void MyDelegate(); }", false);

        [TestMethod]
        public void Class_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass {} }", false);

        [TestMethod]
        public void NestedDelegate_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public delegate void NestedDelegate();
    }
}", true);
    }
}
