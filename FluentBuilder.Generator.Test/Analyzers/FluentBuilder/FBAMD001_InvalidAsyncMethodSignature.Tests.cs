using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBAMD001_InvalidAsyncMethodSignatureTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidAsyncMethodSignatureAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == InvalidAsyncMethodSignatureAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // ===== Valid async methods =====
        [TestMethod]
        public void AsyncMethodReturnsTask_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async Task DoAsync() => await Task.CompletedTask;
    }
}", false);

        [TestMethod]
        public void AsyncMethodReturnsTaskOfInt_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async Task<int> GetAsync() => 42;
    }
}", false);

        [TestMethod]
        public void MethodWithFluentAsyncAttributeReturnsTask_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentAsyncMethod]
        public Task DoAsync() => Task.CompletedTask;
    }
}", false);

        [TestMethod]
        public void MethodWithFluentAsyncMethodReturnsTaskOfInt_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentAsyncMethod]
        public Task<int> GetIntAsync() => Task.FromResult(42);
    }
}", false);

        // ===== Invalid async methods =====
        [TestMethod]
        public void AsyncMethodReturnsVoid_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async void DoAsync() { }
    }
}", true);

        [TestMethod]
        public void AsyncMethodReturnsInt_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async int GetIntAsync() => 42;
    }
}", true);

        [TestMethod]
        public void MethodWithFluentAsyncAttributeReturnsVoid_ReportsDiagnostic() =>
            RunTest(@"
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentAsyncMethod]
        public void DoAsync() { }
    }
}", true);

        [TestMethod]
        public void AsyncMethodReturnsValueTask_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async ValueTask DoAsync() => await ValueTask.CompletedTask;
    }
}", true);

        [TestMethod]
        public void AsyncMethodReturnsValueTaskOfInt_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async ValueTask<int> GetAsync() => 42;
    }
}", true);

        // ===== Ignored methods =====
        [TestMethod]
        public void AsyncMethodWithFluentIgnore_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        [FluentIgnore]
        public async void IgnoredMethod() { }
    }
}", false);

        // ===== Mixed methods (only invalid reported) =====
        [TestMethod]
        public void MixedMethods_OnlyInvalidReported()
        {
            var source = @"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        public async Task Valid1() => await Task.CompletedTask;       // valid
        public async void Invalid1() { }                             // invalid
        [FluentAsyncMethod] public Task Valid2() => Task.CompletedTask; // valid
        [FluentAsyncMethod] public void Invalid2() { }               // invalid
        [FluentIgnore] public async void Ignored() { }               // ignored
    }
}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: "latest");
            var diagnostics = result.CompilationErrors.Where(d => d.Id == InvalidAsyncMethodSignatureAnalyzer.DiagnosticId).ToList();
            Assert.AreEqual(2, diagnostics.Count, "Should report only Invalid1 and Invalid2");
        }

        // ===== Non‑public methods are ignored =====
        [TestMethod]
        public void PrivateAsyncMethod_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass {
        private async void DoAsync() { }
    }
}", false);

        // ===== Types without [FluentBuilder] are ignored =====
        [TestMethod]
        public void AsyncMethodInClassWithoutBuilder_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class MyClass {
        public async void DoAsync() { }
    }
}", false);

        // ===== Nested builder types =====
        [TestMethod]
        public void AsyncMethodInNestedBuilderType_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Container {
        [FluentBuilder]
        public class Nested {
            public async void DoAsync() { }
        }
    }
}", true);

        // ===== Generic builder types =====
        [TestMethod]
        public void AsyncMethodInGenericBuilderType_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public class MyClass<T> {
        public async void DoAsync() { }
    }
}", true);

        // ===== Record builder types =====
        [TestMethod]
        public void AsyncMethodInRecordBuilder_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    [FluentBuilder]
    public record MyRecord {
        public async void DoAsync() { }
    }
}", true);
    }
}
