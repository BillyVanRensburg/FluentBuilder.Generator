using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD019_CollectionWithoutAddMethodTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new CollectionWithoutAddMethodAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationWarnings.Any(d => d.Id == CollectionWithoutAddMethodAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Types with Add method – no diagnostic =====
        [TestMethod]
        public void List_NoDiagnostic() =>
            RunTest(@"
using System.Collections.Generic;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public List<int> Items { get; set; }
    }
}", false);

        [TestMethod]
        public void ObservableCollection_NoDiagnostic() =>
            RunTest(@"
using System.Collections.ObjectModel;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public ObservableCollection<int> Items { get; set; }
    }
}", false);

        // ===== Collection-like types without Add – diagnostic =====
        [TestMethod]
        public void ReadOnlyCollection_ReportsDiagnostic() =>
            RunTest(@"
using System.Collections.ObjectModel;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public ReadOnlyCollection<int> Items { get; set; }
    }
}", true);

        [TestMethod]
        public void Array_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int[] Items { get; set; }
    }
}", true);

        [TestMethod]
        public void ImmutableArray_ReportsDiagnostic() =>
            RunTest(@"
using System.Collections.Immutable;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public ImmutableArray<int> Items { get; set; }
    }
}", true);

        [TestMethod]
        public void IReadOnlyList_ReportsDiagnostic() =>
            RunTest(@"
using System.Collections.Generic;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public IReadOnlyList<int> Items { get; set; }
    }
}", true);

        [TestMethod]
        public void ICollectionWithoutAdd_ReportsDiagnostic()
        {
            var source = @"
using System.Collections;
using System.Collections.Generic;
using FluentBuilder;
namespace Test {
    public class CustomCollection<T> : ICollection<T> {
        public int Count => 0;
        public bool IsReadOnly => true;
        public void Clear() {}
        public bool Contains(T item) => false;
        public void CopyTo(T[] array, int arrayIndex) {}
        public IEnumerator<T> GetEnumerator() => null;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Remove(T item) => false;
        // No Add method
    }
    [FluentBuilder]
    public class MyClass {
        public CustomCollection<int> Items { get; set; }
    }
}";
            RunTest(source, true);
        }

        // ===== Non‑collection types – no diagnostic =====
        [TestMethod]
        public void Primitive_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public int Number { get; set; }
    }
}", false);

        [TestMethod]
        public void String_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public string Text { get; set; }
    }
}", false);

        [TestMethod]
        public void CustomClass_NoDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    public class MyCustom { }
    [FluentBuilder]
    public class MyClass {
        public MyCustom Prop { get; set; }
    }
}", false);

        // ===== Field instead of property =====
        [TestMethod]
        public void ReadOnlyCollectionField_ReportsDiagnostic() =>
            RunTest(@"
using System.Collections.ObjectModel;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public ReadOnlyCollection<int> Items;
    }
}", true);

        // ===== Ignored member – no diagnostic =====
        [TestMethod]
        public void IgnoredCollection_NoDiagnostic() =>
            RunTest(@"
using System.Collections.ObjectModel;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentIgnore]
        public ReadOnlyCollection<int> Items { get; set; }
    }
}", false);
    }
}
