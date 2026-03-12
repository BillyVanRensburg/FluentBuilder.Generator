using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentBuilder;
using FluentBuilder.Generator;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace FluentBuilder.Generator.BaseCode
{
    public static class Action
    {
        public static CompilationWithGeneratedCodeResult RunGeneratorAndCompile(
            string inputSource,
            ImmutableArray<DiagnosticAnalyzer>? analyzers = null,
            string langVersion = "latest",
            bool combineIntoSingleFile = false)
        {
            Console.WriteLine("\n=== ORIGINAL CODE");
            Console.WriteLine(inputSource);

            // Parse the requested language version
            var languageVersion = ParseLanguageVersion(langVersion);
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);

            var syntaxTree = CSharpSyntaxTree.ParseText(inputSource, parseOptions);
            var compilation = CreateTestCompilation(syntaxTree);
            var generator = new FluentBuilderGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(
                generators: new[] { generator.AsSourceGenerator() },
                parseOptions: parseOptions,
                driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

            driver = driver.RunGenerators(compilation);
            var runResult = driver.GetRunResult();

            var generatedSources = runResult.Results
                .SelectMany(r => r.GeneratedSources)
                .ToList();

            // List of syntax trees for final compilation
            List<SyntaxTree> allSources = new List<SyntaxTree>();

            if (combineIntoSingleFile)
            {
                // Combine original and all generated sources into one big string
                var combinedCode = new StringBuilder();
                combinedCode.AppendLine(inputSource.TrimEnd());
                foreach (var source in generatedSources)
                {
                    combinedCode.AppendLine();
                    combinedCode.AppendLine($"// Generated: {source.HintName}");
                    combinedCode.AppendLine(source.SourceText.ToString().TrimEnd());
                }
                var combinedTree = CSharpSyntaxTree.ParseText(
                    combinedCode.ToString(),
                    parseOptions,
                    path: "Combined.cs");
                allSources.Add(combinedTree);
            }
            else
            {
                // Original behavior: separate trees
                allSources.Add(syntaxTree);
                foreach (var source in generatedSources)
                {
                    var generatedTree = CSharpSyntaxTree.ParseText(
                        source.SourceText.ToString(),
                        parseOptions,
                        path: source.HintName);
                    allSources.Add(generatedTree);
                }
            }

            // Create new compilation with the chosen sources
            var finalCompilation = CSharpCompilation.Create(
                assemblyName: "TestAssemblyWithGenerated",
                syntaxTrees: allSources,
                references: GetMetadataReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable)
                    .WithOptimizationLevel(OptimizationLevel.Debug));

            // Collect diagnostics from compilation
            var diagnostics = finalCompilation.GetDiagnostics().ToList();

            // Run analyzers if provided
            if (analyzers.HasValue && !analyzers.Value.IsEmpty)
            {
                var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
                var compilationWithAnalyzers = finalCompilation.WithAnalyzers(analyzers.Value, analyzerOptions);
                var analyzerDiagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
                diagnostics.AddRange(analyzerDiagnostics);
            }

            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            var warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();

            // === PRINT GENERATED SOURCES FIRST ===
            if (generatedSources.Any())
            {
                Console.WriteLine("\n=== GENERATED SOURCES ===");
                foreach (var source in generatedSources)
                {
                    Console.WriteLine($"--- {source.HintName} ---");
                    Console.WriteLine(source.SourceText.ToString());
                }
            }

            // === THEN PRINT ERRORS AND WARNINGS ===
            foreach (var error in errors)
            {
                Console.WriteLine($"❌ Compilation Error {error.Id}: {error.GetMessage()}");
            }
            foreach (var warning in warnings)
            {
                Console.WriteLine($"⚠ Compilation Warning {warning.Id}: {warning.GetMessage()}");
            }

            return new CompilationWithGeneratedCodeResult
            {
                GeneratedSources = generatedSources,
                FinalCompilation = finalCompilation,
                CompilationDiagnostics = diagnostics,
                CompilationErrors = errors,
                CompilationWarnings = warnings
            };
        }

        private static LanguageVersion ParseLanguageVersion(string version)
        {
            // Guard against null or empty input – treat as default compiler version.
            if (string.IsNullOrEmpty(version))
                return LanguageVersion.Default;

            version = version.Trim().ToLowerInvariant();

            return version switch
            {
                "latest" => LanguageVersion.Latest,
                "preview" => LanguageVersion.Preview,
                "default" => LanguageVersion.Default,
                "csharp1" or "1" or "1.0" => LanguageVersion.CSharp1,
                "csharp2" or "2" or "2.0" => LanguageVersion.CSharp2,
                "csharp3" or "3" or "3.0" => LanguageVersion.CSharp3,
                "csharp4" or "4" or "4.0" => LanguageVersion.CSharp4,
                "csharp5" or "5" or "5.0" => LanguageVersion.CSharp5,
                "csharp6" or "6" or "6.0" => LanguageVersion.CSharp6,
                "csharp7" or "7" or "7.0" => LanguageVersion.CSharp7,
                "csharp7_1" or "7.1" => LanguageVersion.CSharp7_1,
                "csharp7_2" or "7.2" => LanguageVersion.CSharp7_2,
                "csharp7_3" or "7.3" => LanguageVersion.CSharp7_3,
                "csharp8" or "8" or "8.0" => LanguageVersion.CSharp8,
                "csharp9" or "9" or "9.0" => LanguageVersion.CSharp9,
                "csharp10" or "10" or "10.0" => LanguageVersion.CSharp10,
                "csharp11" or "11" or "11.0" => LanguageVersion.CSharp11,
                "csharp12" or "12" or "12.0" => LanguageVersion.CSharp12,
                "csharp13" or "13" or "13.0" => LanguageVersion.CSharp13,
                _ => TryParseEnum<LanguageVersion>(version) ?? throw new ArgumentException($"Unsupported language version: {version}")
            };
        }

        private static T? TryParseEnum<T>(string value) where T : struct
        {
            if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
                return result;
            return null;
        }

        private static List<MetadataReference> GetMetadataReferences()
        {
            var references = new List<MetadataReference>();

            var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p));

            references.AddRange(trustedAssemblies);

            var additionalAssemblies = new[]
            {
                typeof(FluentBuilderAttribute).Assembly,
                typeof(FluentBuilderGenerator).Assembly,
                typeof(FluentBuilder.Generator.Analyzers.FluentBuilderAnalyzer).Assembly,
                typeof(System.ComponentModel.ArrayConverter).Assembly,
                typeof(System.Linq.Enumerable).Assembly,
                typeof(System.Text.RegularExpressions.Regex).Assembly,
                typeof(System.Collections.Generic.List<>).Assembly,
                typeof(System.Threading.Tasks.Task).Assembly
            };

            foreach (var assembly in additionalAssemblies)
            {
                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            return references;
        }

        private static Compilation CreateTestCompilation(SyntaxTree syntaxTree)
        {
            return CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: GetMetadataReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable));
        }
    }

    public class CompilationWithGeneratedCodeResult
    {
        public List<GeneratedSourceResult> GeneratedSources { get; set; }
        public Compilation FinalCompilation { get; set; }
        public List<Diagnostic> CompilationDiagnostics { get; set; }
        public List<Diagnostic> CompilationErrors { get; set; }
        public List<Diagnostic> CompilationWarnings { get; set; }

        public bool CompilationSuccess => !CompilationErrors.Any();

        public string GetGeneratedSource(string hintName = null)
        {
            if (hintName == null)
                return string.Join("\n", GeneratedSources.Select(s => s.SourceText.ToString()));

            var source = GeneratedSources.FirstOrDefault(s => s.HintName == hintName);
            if (source.Equals(default(GeneratedSourceResult)))
                return string.Empty;

            return source.SourceText.ToString();
        }
    }
}
