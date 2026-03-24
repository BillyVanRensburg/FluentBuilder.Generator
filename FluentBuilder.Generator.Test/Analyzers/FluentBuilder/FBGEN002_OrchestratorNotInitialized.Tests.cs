using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBGEN002_OrchestratorNotInitializedTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new OrchestratorNotInitializedAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == OrchestratorNotInitializedAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Valid cases =====
        [TestMethod]
        public void TopLevelType_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass { }
}", false);

        [TestMethod]
        public void NestedType_ContainersAllPartial_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public partial class Container {
        [FluentBuilder]
        public partial class Nested { }
    }
}", false);

        [TestMethod]
        public void DeeplyNested_AllPartial_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public partial class A {
        public partial class B {
            [FluentBuilder]
            public partial class C { }
        }
    }
}", false);

        // ===== Invalid cases =====
        [TestMethod]
        public void NestedType_ContainerNotPartial_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container { // not partial
        [FluentBuilder]
        public class Nested { }
    }
}", true);

        [TestMethod]
        public void NestedType_ContainerPartialButNestedNotPartial_DoesNotReport() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public partial class Container {
        [FluentBuilder]
        public class Nested { } // Nested not partial, but container is partial -> should be OK
    }
}", false);

        [TestMethod]
        public void DeeplyNested_OneContainerMissingPartial_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public partial class A {
        public class B { // missing partial
            [FluentBuilder]
            public partial class C { }
        }
    }
}", true);

        [TestMethod]
        public void DeeplyNested_SecondContainerMissingPartial_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public partial class A {
        public class B { // missing partial
            [FluentBuilder]
            public partial class C { }
        }
    }
}", true);
    }
}
