using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD010_NoAccessibleConstructorTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new NoAccessibleConstructorAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationWarnings.Any(d => d.Id == NoAccessibleConstructorAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== No constructors declared =====
        [TestMethod]
        public void ClassWithImplicitPublicConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { } }", false);

        // ===== Explicit constructors =====
        [TestMethod]
        public void ClassWithPublicConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { public MyClass() {} } }", false);

        [TestMethod]
        public void ClassWithInternalConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { internal MyClass() {} } }", false);

        [TestMethod]
        public void ClassWithProtectedInternalConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { protected internal MyClass() {} } }", false);

        [TestMethod]
        public void ClassWithPrivateConstructor_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { private MyClass() {} } }", true);

        [TestMethod]
        public void ClassWithProtectedConstructor_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { protected MyClass() {} } }", true);

        [TestMethod]
        public void ClassWithPrivateProtectedConstructor_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public class MyClass { private protected MyClass() {} } }", true);

        // ===== Multiple constructors, at least one accessible =====
        [TestMethod]
        public void ClassWithMixedConstructors_HasPublic_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] public class MyClass { 
        private MyClass(int x) {} 
        public MyClass() {} 
    } 
}", false);

        // ===== No accessible constructors but factory method present =====
        [TestMethod]
        public void ClassWithFactoryMethod_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] 
    [FluentBuilderFactoryMethod(""Create"")]
    public class MyClass { 
        private MyClass() {} 
        public static MyClass Create() => new MyClass(); 
    } 
}", false);

        // ===== Record types =====
        [TestMethod]
        public void RecordWithPrimaryConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public record MyRecord(int X); }", false);

        [TestMethod]
        public void RecordWithPrivateConstructor_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] public record MyRecord { 
        private MyRecord() { } 
        public int X { get; init; } 
    } 
}", true);

        // ===== Nested types =====
        [TestMethod]
        public void NestedClassWithInternalConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    public class Container { 
        [FluentBuilder] public class Nested { internal Nested() {} } 
    } 
}", false);

        [TestMethod]
        public void NestedClassWithPrivateConstructor_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    public class Container { 
        [FluentBuilder] public class Nested { private Nested() {} } 
    } 
}", true);

        // ===== Non-class/record types should be ignored =====
        [TestMethod]
        public void Struct_WithPrivateConstructor_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] public struct MyStruct { 
        private MyStruct(int x) { } 
    } 
}", false);

        [TestMethod]
        public void Enum_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public enum MyEnum { A, B } }", false);

        [TestMethod]
        public void Delegate_WithFluentBuilder_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { [FluentBuilder] public delegate void MyDelegate(); }", false);

        // ===== Factory method attribute present but method missing – analyzer trusts attribute =====
        [TestMethod]
        public void ClassWithFactoryMethodAttributeButMissingMethod_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] 
    [FluentBuilderFactoryMethod(""Create"")]
    public class MyClass { 
        private MyClass() {} 
        // No Create method
    } 
}", false); // The analyzer trusts the attribute; generator will report error later.

        // ===== Class with no accessible constructors and factory method attribute but method is private =====
        [TestMethod]
        public void ClassWithFactoryMethodAttributeButPrivateMethod_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] 
    [FluentBuilderFactoryMethod(""Create"")]
    public class MyClass { 
        private MyClass() {} 
        private static MyClass Create() => new MyClass(); 
    } 
}", false); // Still trusts attribute; generator may fail.

        // ===== Class with no accessible constructors and factory method attribute but method is not static =====
        [TestMethod]
        public void ClassWithFactoryMethodAttributeButNonStaticMethod_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] 
    [FluentBuilderFactoryMethod(""Create"")]
    public class MyClass { 
        private MyClass() {} 
        public MyClass Create() => new MyClass(); // instance method
    } 
}", false); // Analyzer trusts attribute; generator will error.

        // ===== Class with inaccessible constructors but also marked with other attributes =====
        [TestMethod]
        public void ClassWithInaccessibleConstructorsAndFluentImplicit_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] 
    [FluentImplicit]
    public class MyClass { 
        private MyClass() {} 
    } 
}", true); // Still warns because no accessible constructor.

        [TestMethod]
        public void ClassWithInaccessibleConstructorsAndAsyncSupport_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test { 
    [FluentBuilder] 
    [FluentBuilderAsyncSupport]
    public class MyClass { 
        private MyClass() {} 
    } 
}", true); // Still warns.
    }
}
