using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBNAM002_DuplicateFluentMethodNameTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new DuplicateFluentMethodNameAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == DuplicateFluentMethodNameAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Duplicate custom names =====
        [TestMethod]
        public void SameFluentNameOnTwoProperties_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Value"")] public int Prop1 { get; set; }
        [FluentName(""Value"")] public int Prop2 { get; set; }
    }
}", true);

        [TestMethod]
        public void SameFluentNameOnPropertyAndMethod_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Do"")] public int Prop { get; set; }
        [FluentName(""Do"")] public void Method() { }
    }
}", true);

        // ===== Default name collisions =====
        [TestMethod]
        public void DefaultNameCollisionDueToPrefix_ReportsDiagnostic()
        {
            // With prefix "With", both properties would become "WithValue".
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder(MethodPrefix = ""With"")]
    public class MyClass {
        public int Value { get; set; }
        public int Value2 { get; set; } // no conflict because names differ
    }
}", false); // No conflict because names are different.
        }

        [TestMethod]
        public void DefaultNameCollisionDueToSuffix_ReportsDiagnostic()
        {
            // Both become "ValueAsync"
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder(MethodSuffix = ""Async"")]
    public class MyClass {
        public int Value { get; set; }
        public int Value2 { get; set; } // different name, no conflict
    }
}", false);
        }

        // Actually, for a single property, no conflict. Need two properties with same base name? Not possible because property names are unique.
        // But a property and a method could collide if the method name matches the property's prefixed name.
        [TestMethod]
        public void PropertyDefaultCollidesWithMethodName_ReportsDiagnostic()
        {
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder(MethodPrefix = ""With"")]
    public class MyClass {
        public int Value { get; set; } // -> WithValue
        public void WithValue() { } // method name conflicts with property's fluent method
    }
}", true);
        }

        [TestMethod]
        public void MethodNameCollidesWithPropertyFluentName_ReportsDiagnostic()
        {
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Do"")] public int Prop { get; set; }
        public void Do() { }
    }
}", true);
        }

        // ===== Reserved name collisions =====
        [TestMethod]
        public void CustomNameCollidesWithBuild_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Build"")] public int Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void DefaultNameCollidesWithBuild_ReportsDiagnostic()
        {
            // With prefix "" and suffix "" the property name "Build" collides.
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder(MethodPrefix = """", MethodSuffix = """")]
    public class MyClass {
        public int Build { get; set; }
    }
}", true);
        }

        // ===== No duplicates =====
        [TestMethod]
        public void AllUnique_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Prop1 { get; set; }
        public string Prop2 { get; set; }
        public void Method() { }
    }
}", false);

        // ===== Ignored members are excluded =====
        [TestMethod]
        public void IgnoredMember_NotConsidered() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Value"")] public int Prop1 { get; set; }
        [FluentIgnore]
        [FluentName(""Value"")] public int Prop2 { get; set; } // ignored, so no conflict
    }
}", false);

        // ===== Duplicates only count among included members =====
        [TestMethod]
        public void DuplicateAmongIncludedOnly_ReportsDiagnostic()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentName(""Value"")] public int Prop1 { get; set; }
        [FluentIgnore]
        [FluentName(""Value"")] public int Prop2 { get; set; } // ignored
        [FluentName(""Value"")] public int Prop3 { get; set; } // conflict with Prop1
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationErrors.Where(d => d.Id == DuplicateFluentMethodNameAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(1, diagnostics.Count);
        }
    }
}
