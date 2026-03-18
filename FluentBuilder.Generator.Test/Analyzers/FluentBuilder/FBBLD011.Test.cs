using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD011_MissingNestedBuilderTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new MissingNestedBuilderAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationWarnings.Any(d => d.Id == MissingNestedBuilderAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Basic scenarios =====
        [TestMethod]
        public void PropertyTypeWithBuilder_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Inner { }
    [FluentBuilder] public class Outer { public Inner Prop { get; set; } }
}", false);

        [TestMethod]
        public void PropertyTypeWithoutBuilder_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Inner { }
    [FluentBuilder] public class Outer { public Inner Prop { get; set; } }
}", true);

        // ===== Multiple properties =====
        [TestMethod]
        public void MultipleProperties_MixedWarnings()
        {
            var source = @"
using FluentBuilder;
namespace Test {
    public class InnerA { }
    [FluentBuilder] public class InnerB { }
    [FluentBuilder] public class Outer {
        public InnerA PropA { get; set; } // should warn
        public InnerB PropB { get; set; } // no warning
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationWarnings.Where(d => d.Id == MissingNestedBuilderAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(1, diagnostics.Count, "Should warn only on PropA (InnerA)");
            Assert.IsTrue(diagnostics[0].GetMessage().Contains("InnerA"), "Message should mention InnerA");
        }

        // ===== Ignored property kinds =====
        [TestMethod]
        public void PrimitiveType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Outer { public int Prop { get; set; } }
}", false);

        [TestMethod]
        public void StringType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Outer { public string Prop { get; set; } }
}", false);

        [TestMethod]
        public void EnumType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public enum Color { Red, Green }
    [FluentBuilder] public class Outer { public Color Prop { get; set; } }
}", false);

        [TestMethod]
        public void ArrayType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Inner { }
    [FluentBuilder] public class Outer { public Inner[] Prop { get; set; } }
}", false); // array itself is not a named type eligible for builder

        [TestMethod]
        public void CollectionType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
using System.Collections.Generic;
namespace Test {
    [FluentBuilder] public class Inner { }
    [FluentBuilder] public class Outer { public List<Inner> Prop { get; set; } }
}", false); // List<T> is not a user-defined class with builder

        // ===== Non-public properties =====
        [TestMethod]
        public void NonPublicProperty_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Inner { }
    [FluentBuilder] public class Outer { private Inner Prop { get; set; } }
}", false);

        // ===== Static property =====
        [TestMethod]
        public void StaticProperty_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Inner { }
    [FluentBuilder] public class Outer { public static Inner Prop { get; set; } }
}", false);

        // ===== Property of external (framework) type =====
        [TestMethod]
        public void ExternalType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
using System;
namespace Test {
    [FluentBuilder] public class Outer { public DateTime Prop { get; set; } }
}", false); // DateTime is from System, not user code

        // ===== Nested types =====
        [TestMethod]
        public void NestedPropertyTypeWithoutBuilder_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        public class Inner { }
    }
    [FluentBuilder] public class Outer { public Container.Inner Prop { get; set; } }
}", true);

        [TestMethod]
        public void NestedPropertyTypeWithBuilder_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder] public class Inner { }
    }
    [FluentBuilder] public class Outer { public Container.Inner Prop { get; set; } }
}", false);

        // ===== Generic property type =====
        [TestMethod]
        public void GenericTypeWithoutBuilder_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class Generic<T> { }
    [FluentBuilder] public class Outer { public Generic<int> Prop { get; set; } }
}", true);

        [TestMethod]
        public void GenericTypeWithBuilder_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Generic<T> { }
    [FluentBuilder] public class Outer { public Generic<int> Prop { get; set; } }
}", false);

        // ===== Interface type =====
        [TestMethod]
        public void InterfaceType_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public interface IInner { }
    [FluentBuilder] public class Outer { public IInner Prop { get; set; } }
}", false); // interfaces are not classes/records

        // ===== Abstract class without builder =====
        [TestMethod]
        public void AbstractClassWithoutBuilder_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public abstract class Inner { }
    [FluentBuilder] public class Outer { public Inner Prop { get; set; } }
}", true); 

        // ===== Static class without builder =====
        [TestMethod]
        public void StaticClassWithoutBuilder_Warning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public static class Inner { }
    [FluentBuilder] public class Outer { public Inner Prop { get; set; } }
}", true);

        // ===== Property of same type as containing type (recursive) =====
        [TestMethod]
        public void RecursiveProperty_NoWarning() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder] public class Node { public Node Next { get; set; } }
}", false); // property type is same, which has builder, so no warning
    }
}
