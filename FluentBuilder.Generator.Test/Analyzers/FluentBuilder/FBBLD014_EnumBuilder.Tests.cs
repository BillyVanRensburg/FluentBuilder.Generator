using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD014_EnumBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new EnumBuilderAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == EnumBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        [TestMethod]
        public void Enum_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public enum MyEnum { A, B } }", true);

        [TestMethod]
        public void Enum_WithoutFluentBuilder_NoDiagnostic() =>
            RunTest(@"
namespace Test { public enum MyEnum { A, B } }", false);

        [TestMethod]
        public void Class_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass {} }", false);

        [TestMethod]
        public void NestedEnum_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public enum NestedEnum { X, Y }
    }
}", true);
    }
}
