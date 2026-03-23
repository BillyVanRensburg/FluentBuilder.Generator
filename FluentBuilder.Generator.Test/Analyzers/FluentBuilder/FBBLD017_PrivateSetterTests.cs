using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD017_PrivateSetterTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new PrivateSetterAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationWarnings.Any(d => d.Id == PrivateSetterAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Properties with private setter =====
        [TestMethod]
        public void PropertyWithPrivateSetter_ReportsWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; private set; }
    }
}", true);

        [TestMethod]
        public void PropertyWithPublicSetter_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void PropertyWithInternalSetter_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; internal set; }
    }
}", false);

        [TestMethod]
        public void PropertyWithProtectedSetter_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; protected set; }
    }
}", false);

        // ===== Properties with no setter (read‑only) =====
        [TestMethod]
        public void ReadOnlyProperty_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop { get; }
    }
}", false); // Already covered by FB015? But this analyzer only checks setter existence and accessibility; no setter => no warning.

        // ===== Ignored property =====
        [TestMethod]
        public void PropertyWithPrivateSetterAndFluentIgnore_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentIgnore]
        public int Prop { get; private set; }
    }
}", false);

        // ===== Static properties =====
        [TestMethod]
        public void StaticPropertyWithPrivateSetter_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public static int Prop { get; private set; }
    }
}", false);

        // ===== Non‑public properties =====
        [TestMethod]
        public void PrivatePropertyWithPrivateSetter_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        private int Prop { get; set; }
    }
}", false);

        // ===== Multiple properties, only one with private setter =====
        [TestMethod]
        public void MixedProperties_OnlyPrivateSetterReported()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop1 { get; private set; } // warning
        public int Prop2 { get; set; }         // no warning
        public int Prop3 { get; }              // no warning
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationWarnings.Where(d => d.Id == PrivateSetterAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(1, diagnostics.Count, "Should report only Prop1");
            Assert.IsTrue(diagnostics[0].GetMessage().Contains("Prop1"));
        }

        // ===== Record with primary constructor (property generated automatically) =====
        [TestMethod]
        public void RecordWithPrimaryConstructor_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public record MyRecord(int Prop);
}", false); // Record properties have init accessors, not setters.

        // ===== Property with private setter but also [FluentInclude] – should still warn =====
        [TestMethod]
        public void PropertyWithPrivateSetterAndFluentInclude_ReportsWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentInclude]
        public int Prop { get; private set; }
    }
}", true); // FluentInclude does not override the setter accessibility issue.
    }
}
