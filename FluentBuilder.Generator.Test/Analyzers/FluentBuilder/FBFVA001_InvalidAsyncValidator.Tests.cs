using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBFVA001_InvalidAsyncValidatorTests
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidAsyncValidatorAnalyzer());

        private static void RunTest(string source, bool expectDiagnostic, string languageVersion = "latest")
        {
            var result = BaseCode.Action.RunGeneratorAndCompile(source, Analyzers, langVersion: languageVersion);
            var hasDiagnostic = result.CompilationErrors.Any(d => d.Id == InvalidAsyncValidatorAnalyzer.DiagnosticId);
            Assert.AreEqual(expectDiagnostic, hasDiagnostic, source);
        }

        // Valid cases
        [TestMethod]
        public void ValidValidator_NoDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        public Validator() { }
        public Task<bool> Validate(string value) => Task.FromResult(true);
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), nameof(Validator.Validate))]
        public string Prop { get; set; }
    }
}", false);

        [TestMethod]
        public void ValidValidatorWithCancellationToken_NoDiagnostic() =>
            RunTest(@"
using System.Threading;
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        public Validator() { }
        public Task<bool> Validate(string value, CancellationToken ct) => Task.FromResult(true);
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), nameof(Validator.Validate))]
        public string Prop { get; set; }
    }
}", false);

        // Invalid cases
        [TestMethod]
        public void ValidatorWithoutPublicConstructor_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        private Validator() { }
        public Task<bool> Validate(string value) => Task.FromResult(true);
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), nameof(Validator.Validate))]
        public string Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void MethodNotFound_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        public Validator() { }
        public Task<bool> Validate(string value) => Task.FromResult(true);
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), ""WrongMethod"")]
        public string Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void WrongReturnType_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        public Validator() { }
        public async void Validate(string value) { }
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), nameof(Validator.Validate))]
        public string Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void WrongParameterType_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        public Validator() { }
        public Task<bool> Validate(int value) => Task.FromResult(true);
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), nameof(Validator.Validate))]
        public string Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void StaticMethod_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator {
        public Validator() { }
        public static Task<bool> Validate(string value) => Task.FromResult(true);
    }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync(typeof(Validator), nameof(Validator.Validate))]
        public string Prop { get; set; }
    }
}", true);

        [TestMethod]
        public void MissingAttributeArguments_ReportsDiagnostic() =>
            RunTest(@"
using System.Threading.Tasks;
using FluentBuilder;
namespace Test {
    public class Validator { public Validator() { } }
    [FluentBuilder]
    public class MyClass {
        [FluentValidateAsync] // missing arguments
        public string Prop { get; set; }
    }
}", true);
    }
}
