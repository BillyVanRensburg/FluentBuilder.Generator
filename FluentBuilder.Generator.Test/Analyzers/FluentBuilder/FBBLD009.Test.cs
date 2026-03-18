using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD009_StaticClassBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new StaticClassBuilderAnalyzer());

        // Helper to run a test and assert diagnostic presence.
        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == StaticClassBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Basic tests =====
        [TestMethod]
        public void StaticClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public static class MyClass {} }", true);

        [TestMethod]
        public void NonStaticClass_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass {} }", false);

        [TestMethod]
        public void StaticClass_WithoutFluentBuilder_NoDiagnostic() =>
            RunTest(@"
namespace Test { public static class MyClass {} }", false);

        [TestMethod]
        public void Interface_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public interface IMyInterface {} }", false);

        // ===== Accessibility variations =====
        [TestMethod]
        public void InternalStaticClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] internal static class MyClass {} }", true);

        // Private top-level static class is invalid, but nested private static class is valid
        [TestMethod]
        public void NestedPrivateStaticClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] private static class NestedStatic {}
    }
}", true);

        // ===== Nested types =====
        [TestMethod]
        public void NestedStaticClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public static class NestedStatic {}
    }
}", true);

        [TestMethod]
        public void NestedNonStaticClass_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public class NestedNonStatic {}
    }
}", false);

        // ===== Generic static class =====
        [TestMethod]
        public void GenericStaticClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public static class GenericStatic<T> { }
}", true);

        [TestMethod]
        public void GenericStaticClassWithConstraints_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public static class GenericStatic<T> where T : class { }
}", true);

        // ===== Partial static class =====
        [TestMethod]
        public void PartialStaticClass_WithFluentBuilder_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public static partial class MyPartialClass {}
}", true);

        // ===== Attribute with parameters =====
        [TestMethod]
        public void StaticClass_WithFluentBuilderAndAccessibility_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder(BuilderAccessibility = BuilderAccessibility.Internal)]
    public static class MyClass {}
}", true);

        // ===== Multiple classes in one file =====
        [TestMethod]
        public void MultipleClasses_MixedStaticAndNonStatic_OnlyStaticReported()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public static class Static1 {}
    [FluentBuilder] public class NonStatic1 {}
    [FluentBuilder] public static class Static2 {}
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationErrors.Where(d => d.Id == StaticClassBuilderAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(2, diagnostics.Count, "Should report two diagnostics (Static1 and Static2)");
        }

        // ===== Static class with members =====
        [TestMethod]
        public void StaticClassWithMembers_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public static class MyClass {
        public static int Id { get; set; }
        public static string Name { get; set; }
    }
}", true);

        // ===== Static class with ignored members =====
        [TestMethod]
        public void StaticClassWithIgnoredMembers_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public static class MyClass {
        [FluentIgnore]
        public static int Id { get; set; }
    }
}", true);

        // ===== Static class with FluentName attribute =====
        [TestMethod]
        public void StaticClassWithFluentName_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentName(""CustomName"")]
    [FluentBuilder]
    public static class MyClass { }
}", true);

        // ===== Static class in different namespace =====
        [TestMethod]
        public void StaticClassInDifferentNamespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Other {
    [FluentBuilder] public static class MyClass {}
}", true);

        // ===== Static class with using alias =====
        [TestMethod]
        public void StaticClassWithUsingAlias_ReportsDiagnostic() =>
            RunTest(@"
using FB = FluentBuilder.FluentBuilderAttribute;
namespace Test {
    [FB] public static class MyClass {}
}", true);

        // ===== Static class with file-scoped namespace =====
        [TestMethod]
        public void StaticClass_FileScopedNamespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test;
[FluentBuilder] public static class MyClass {}", true);

        // ===== Static class with no namespace =====
        [TestMethod]
        public void StaticClass_GlobalNamespace_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
[FluentBuilder] public static class MyClass {}", true);

        // ===== Static class nested inside a generic container =====
        [TestMethod]
        public void StaticClassInsideGenericContainer_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container<T> {
        [FluentBuilder] public static class NestedStatic { }
    }
}", true);
    }
}
