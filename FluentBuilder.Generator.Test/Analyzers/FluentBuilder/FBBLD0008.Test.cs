using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD008_AbstractClassBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new AbstractClassBuilderAnalyzer());

        // Helper to run a test and assert diagnostic presence.
        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == AbstractClassBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Basic tests =====
        [TestMethod]
        public void AbstractClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public abstract class MyClass {} }", true);

        [TestMethod]
        public void NonAbstractClass_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass {} }", false);

        [TestMethod]
        public void AbstractClass_WithoutFluentBuilder_NoDiagnostic() =>
            RunTest(@"
namespace Test { public abstract class MyClass {} }", false);

        [TestMethod]
        public void Interface_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public interface IMyInterface {} }", false);

        [TestMethod]
        public void AbstractRecord_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public abstract record MyRecord(int X); }", true);

        // ===== Accessibility variations =====
        [TestMethod]
        public void InternalAbstractClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] internal abstract class MyClass {} }", true);

        [TestMethod]
        public void PrivateAbstractClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] private abstract class MyClass {} }", true); // private at top-level is invalid, but nested private works

        // ===== Nested types =====
        [TestMethod]
        public void NestedAbstractClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public abstract class NestedAbstract {}
    }
}", true);

        [TestMethod]
        public void NestedNonAbstractClass_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public class NestedNonAbstract {}
    }
}", false);

        // ===== Generic abstract class =====
        [TestMethod]
        public void GenericAbstractClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public abstract class GenericAbstract<T> { public T Value { get; set; } }
}", true);

        [TestMethod]
        public void GenericAbstractClassWithConstraints_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public abstract class GenericAbstract<T> where T : class { public T Value { get; set; } }
}", true);

        // ===== Partial abstract class =====
        [TestMethod]
        public void PartialAbstractClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public abstract partial class MyPartialClass {}
}", true);

        // ===== Attribute with parameters =====
        [TestMethod]
        public void AbstractClass_WithFluentBuilderAndAccessibility_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Internal)]
    public abstract class MyClass {}
}", true);

        // ===== Multiple classes in one file =====
        [TestMethod]
        public void MultipleClasses_MixedAbstractAndConcrete_OnlyAbstractReported()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public abstract class Abstract1 {}
    [FluentBuilder] public class Concrete1 {}
    [FluentBuilder] public abstract class Abstract2 {}
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationErrors.Where(d => d.Id == AbstractClassBuilderAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(2, diagnostics.Count, "Should report two diagnostics (Abstract1 and Abstract2)");
        }

        // ===== Abstract class with members =====
        [TestMethod]
        public void AbstractClassWithProperties_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public abstract class MyClass {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}", true);

        // ===== Abstract class with constructors =====
        [TestMethod]
        public void AbstractClassWithConstructor_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public abstract class MyClass {
        protected MyClass(int id) { }
        public int Id { get; set; }
    }
}", true);

        // ===== Abstract class in different namespace =====
        [TestMethod]
        public void AbstractClassInDifferentNamespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Other {
    [FluentBuilder] public abstract class MyClass {}
}", true);

        // ===== Abstract class with using alias =====
        [TestMethod]
        public void AbstractClassWithUsingAlias_ReportsDiagnostic() =>
            RunTest(@"
using FB = FluentBuilder.FluentBuilderAttribute;
namespace Test {
    [FB] public abstract class MyClass {}
}", true);

        // ===== Abstract class with file-scoped namespace =====
        [TestMethod]
        public void AbstractClass_FileScopedNamespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test;
[FluentBuilder] public abstract class MyClass {}", true);

        // ===== Abstract class with no namespace =====
        [TestMethod]
        public void AbstractClass_GlobalNamespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
[FluentBuilder] public abstract class MyClass {}", true);
    }
}
