using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD015_StructBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new StructBuilderAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == StructBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        [TestMethod]
        public void Struct_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public struct MyStruct { } }", true);

        [TestMethod]
        public void Struct_WithoutFluentBuilder_NoDiagnostic() =>
            RunTest(@"
namespace Test { public struct MyStruct { } }", false);

        [TestMethod]
        public void Class_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass {} }", false);

        [TestMethod]
        public void NestedStruct_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public struct NestedStruct { }
    }
}", true);
    }
}
