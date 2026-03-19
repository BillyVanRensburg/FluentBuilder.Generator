using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBGEN001_CircularReferenceTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new CircularReferenceDetectorAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == CircularReferenceDetectorAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Direct cycle =====
        [TestMethod]
        public void DirectCycle_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class A { public B Prop { get; set; } }
    [FluentBuilder] public class B { public A Prop { get; set; } }
}";
            RunTest(source, true);
        }

        // ===== Self-cycle (property of same type) =====
        [TestMethod]
        public void SelfCycle_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Node { public Node Next { get; set; } }
}";
            RunTest(source, true);
        }

        // ===== Indirect cycle (A -> B -> C -> A) =====
        [TestMethod]
        public void IndirectCycle_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class A { public B Prop { get; set; } }
    [FluentBuilder] public class B { public C Prop { get; set; } }
    [FluentBuilder] public class C { public A Prop { get; set; } }
}";
            RunTest(source, true);
        }

        // ===== No cycle =====
        [TestMethod]
        public void NoCycle_NoDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class A { public B Prop { get; set; } }
    [FluentBuilder] public class B { public C Prop { get; set; } }
    [FluentBuilder] public class C { public int X { get; set; } }
}";
            RunTest(source, false);
        }

        // ===== Cycle only through types that have builder attribute =====
        [TestMethod]
        public void CycleThroughNonBuilderType_NoDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class A { public B Prop { get; set; } }
    public class B { public A Prop { get; set; } } // B does NOT have builder
}";
            RunTest(source, false); // B's property is not considered because B lacks builder
        }

        // ===== Multiple properties, only some cause cycle =====
        [TestMethod]
        public void CycleInOnePath_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class A { 
        public B PropB { get; set; } 
        public C PropC { get; set; } 
    }
    [FluentBuilder] public class B { public A PropA { get; set; } } // cycle A->B->A
    [FluentBuilder] public class C { public int X { get; set; } }
}";
            RunTest(source, true);
        }

        // ===== Generic types =====
        [TestMethod]
        public void GenericCycle_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class A<T> { public B<T> Prop { get; set; } }
    [FluentBuilder] public class B<T> { public A<T> Prop { get; set; } }
}";
            RunTest(source, true);
        }

        // ===== Nested types =====
        [TestMethod]
        public void NestedTypesCycle_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public class A { public B Prop { get; set; } }
        [FluentBuilder] public class B { public A Prop { get; set; } }
    }
}";
            RunTest(source, true);
        }

        // ===== Records =====
        [TestMethod]
        public void RecordCycle_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public record A(B Prop);
    [FluentBuilder] public record B(A Prop);
}";
            RunTest(source, true);
        }
    }
}
