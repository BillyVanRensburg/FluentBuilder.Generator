using FluentBuilder.Generator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD012_MissingAttributeReferenceTests
    {
        private static readonly DiagnosticAnalyzer Analyzer = new MissingAttributeReferenceAnalyzer();

        /// <summary>
        /// Creates a compilation with the specified source code and optional references.
        /// If includeFluentBuilderAssembly is true, adds a reference to the assembly containing FluentBuilderAttribute.
        /// </summary>
        private static CompilationWithAnalyzers CreateCompilation(string source, bool includeFluentBuilderAssembly)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            // Core references needed for basic C# compilation.
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib / System.Private.CoreLib
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Linq
                MetadataReference.CreateFromFile(typeof(DiagnosticDescriptor).Assembly.Location), // Microsoft.CodeAnalysis
            };

            // If we want the FluentBuilder attributes to be resolvable, add the assembly that contains them.
            if (includeFluentBuilderAssembly)
            {
                // This assumes the test project references the FluentBuilder.Generator assembly.
                // We locate the assembly by finding the FluentBuilderAttribute type.
                var attributeAssembly = typeof(FluentBuilder.FluentBuilderAttribute).Assembly;
                references.Add(MetadataReference.CreateFromFile(attributeAssembly.Location));
            }

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
            return compilation.WithAnalyzers(ImmutableArray.Create(Analyzer), analyzerOptions);
        }

        private static async Task AssertDiagnosticCount(string source, bool includeReference, int expectedCount)
        {
            var compilationWithAnalyzers = CreateCompilation(source, includeReference);
            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            var actualCount = diagnostics.Count(d => d.Id == MissingAttributeReferenceAnalyzer.DiagnosticId);
            Assert.AreEqual(expectedCount, actualCount, $"Expected {expectedCount} diagnostics, found {actualCount}.");
        }

        // ===== Tests with missing assembly reference =====
        [TestMethod]
        public async Task MissingReference_WithFluentBuilderAttribute_ReportsDiagnostic()
        {
            var source = @"
namespace Test
{
    [FluentBuilder]
    public class MyClass { }
}";
            await AssertDiagnosticCount(source, includeReference: false, expectedCount: 1);
        }

        [TestMethod]
        public async Task MissingReference_WithFluentNameAttribute_ReportsDiagnostic()
        {
            var source = @"
namespace Test
{
    [FluentName(""Custom"")]
    public class MyClass { }
}";
            await AssertDiagnosticCount(source, includeReference: false, expectedCount: 1);
        }

        [TestMethod]
        public async Task MissingReference_WithFluentIgnoreAttribute_ReportsDiagnostic()
        {
            var source = @"
namespace Test
{
    public class MyClass
    {
        [FluentIgnore]
        public int Id { get; set; }
    }
}";
            await AssertDiagnosticCount(source, includeReference: false, expectedCount: 1);
        }

        [TestMethod]
        public async Task MissingReference_WithMultipleAttributes_ReportsEach()
        {
            var source = @"
using System;
namespace Test
{
    [FluentBuilder]
    [FluentName(""Custom"")]
    public class MyClass
    {
        [FluentIgnore]
        public int Id { get; set; }

        [FluentValidate(Required = true)]
        public string Name { get; set; }
    }
}";
            // Should report on each of the four attribute usages.
            await AssertDiagnosticCount(source, includeReference: false, expectedCount: 4);
        }

        [TestMethod]
        public async Task MissingReference_NoAttributes_NoDiagnostic()
        {
            var source = @"
namespace Test
{
    public class MyClass { }
}";
            await AssertDiagnosticCount(source, includeReference: false, expectedCount: 0);
        }

        // ===== Tests with assembly reference present =====
        [TestMethod]
        public async Task WithReference_WithFluentBuilderAttribute_NoDiagnostic()
        {
            var source = @"
namespace Test
{
    [FluentBuilder]
    public class MyClass { }
}";
            await AssertDiagnosticCount(source, includeReference: true, expectedCount: 0);
        }

        [TestMethod]
        public async Task WithReference_WithMultipleAttributes_NoDiagnostic()
        {
            var source = @"
using System;
namespace Test
{
    [FluentBuilder]
    [FluentName(""Custom"")]
    public class MyClass
    {
        [FluentIgnore]
        public int Id { get; set; }
    }
}";
            await AssertDiagnosticCount(source, includeReference: true, expectedCount: 0);
        }

        // ===== Test for attribute that is not from FluentBuilder =====
        [TestMethod]
        public async Task MissingReference_WithNonFluentAttribute_NoDiagnostic()
        {
            var source = @"
using System;
namespace Test
{
    [Obsolete]
    public class MyClass { }
}";
            await AssertDiagnosticCount(source, includeReference: false, expectedCount: 0);
        }
    }
}
