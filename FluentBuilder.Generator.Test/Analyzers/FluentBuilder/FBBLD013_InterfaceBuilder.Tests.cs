using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD013_InterfaceBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new InterfaceBuilderAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == InterfaceBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        [TestMethod]
        public void Interface_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public interface IMyInterface {} }", true);

        [TestMethod]
        public void Interface_WithoutFluentBuilder_NoDiagnostic() =>
            RunTest(@"
namespace Test { public interface IMyInterface {} }", false);

        [TestMethod]
        public void Class_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass {} }", false);

        [TestMethod]
        public void NestedInterface_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public interface INested {}
    }
}", true);
    }
}
