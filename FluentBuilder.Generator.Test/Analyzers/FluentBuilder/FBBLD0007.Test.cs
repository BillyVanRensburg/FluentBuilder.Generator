// Auto-generated exhaustive tests for FBBLD0007
using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers.Tests
{
    [TestClass]
    public class FBBLD0007
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new ConstructorAccessibilityAnalyzer());

        // Helper levels to compute expected outcome (match analyzer logic)
        private static int GetBuilderLevel(string name) => name switch
        {
            // File-scoped builders are not supported by the generator; tests should expect a warning for File.
            "File" => 6,
            "Private" => 1,
            "Protected" => 2,
            "Internal" => 3,
            "PrivateProtected" => 3,
            "ProtectedInternal" => 4,
            "Public" => 5,
            _ => 5
        };

        private static int GetCtorLevel(string name) => name switch
        {
            "private" => 1,
            "protected" => 2,
            "internal" => 3,
            "private protected" => 3,
            "protected internal" => 4,
            "public" => 5,
            _ => 5
        };

        // Builders
        // Public, Internal, Private, Protected, ProtectedInternal, PrivateProtected, File
        // Ctors: public, internal, private, protected, protected internal, private protected

        // Top-level and nested tests follow

        // PUBLIC builder
        [TestMethod] public void PublicBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Public","public", "latest",false); }
        [TestMethod] public void PublicBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Public","internal", "latest", true); }
        [TestMethod] public void PublicBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("Public","private", "latest", true); }
        [TestMethod] public void PublicBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Public","protected", "latest", true); }
        [TestMethod] public void PublicBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Public","protected internal", "latest", true); }
        [TestMethod] public void PublicBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Public","private protected", "latest", true); }

        [TestMethod] public void PublicBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Public","public", "latest", false); }
        [TestMethod] public void PublicBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("Public","internal", "latest", true); }
        [TestMethod] public void PublicBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("Public","private", "latest", true); }
        [TestMethod] public void PublicBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("Public","protected", "latest", true); }
        [TestMethod] public void PublicBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("Public","protected internal", "latest", true); }
        [TestMethod] public void PublicBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("Public","private protected", "latest", true); }

        // INTERNAL builder
        [TestMethod] public void InternalBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Internal","public", "latest", false); }
        [TestMethod] public void InternalBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","internal", "latest", true); }
        [TestMethod] public void InternalBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","private", "latest", true); }
        [TestMethod] public void InternalBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","protected", "latest", true); }
        [TestMethod] public void InternalBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","protected internal", "latest", true); }
        [TestMethod] public void InternalBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","private protected", "latest", true); }

        [TestMethod] public void InternalBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Internal","public", "latest", false); }
        [TestMethod] public void InternalBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("Internal","internal", "latest", true); }
        [TestMethod] public void InternalBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("Internal","private", "latest", true); }
        [TestMethod] public void InternalBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("Internal","protected", "latest", true); }
        [TestMethod] public void InternalBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("Internal","protected internal", "latest", true); }
        [TestMethod] public void InternalBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("Internal","private protected", "latest", true); }

        // PRIVATE builder
        [TestMethod] public void PrivateBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","public", "latest", false); }
        [TestMethod] public void PrivateBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Private","internal", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("Private","private", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Private","protected", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Private","protected internal", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Private","private protected", "latest", true); }

        [TestMethod] public void PrivateBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Private","public", "latest", false); }
        [TestMethod] public void PrivateBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("Private","internal", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("Private","private", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("Private","protected", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("Private","protected internal", "latest", true); }
        [TestMethod] public void PrivateBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("Private","private protected", "latest", true); }

        // PROTECTED builder
        [TestMethod] public void ProtectedBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Protected","public", "latest", false); }
        [TestMethod] public void ProtectedBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Protected","internal", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("Protected","private", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Protected","protected", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Protected","protected internal", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Protected","private protected", "latest", true); }

        [TestMethod] public void ProtectedBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Protected","public", "latest", false); }
        [TestMethod] public void ProtectedBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("Protected","internal", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("Protected","private", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("Protected","protected", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("Protected","protected internal", "latest", true); }
        [TestMethod] public void ProtectedBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("Protected","private protected", "latest", true); }

        // PROTECTEDINTERNAL builder
        [TestMethod] public void ProtectedInternalBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("ProtectedInternal","public", "latest", false); }
        [TestMethod] public void ProtectedInternalBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","internal", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","private", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","protected", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","protected internal", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","private protected", "latest", true); }

        [TestMethod] public void ProtectedInternalBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("ProtectedInternal","public", "latest", false); }
        [TestMethod] public void ProtectedInternalBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","internal", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","private", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","protected", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","protected internal", "latest", true); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","private protected", "latest", true); }

        // PRIVATEPROTECTED builder
        [TestMethod] public void PrivateProtectedBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","public", "latest", false); }
        [TestMethod] public void PrivateProtectedBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("PrivateProtected","internal", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("PrivateProtected","private", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("PrivateProtected","protected", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("PrivateProtected","protected internal", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("PrivateProtected","private protected", "latest", true); }

        [TestMethod] public void PrivateProtectedBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","public", "latest", false); }
        [TestMethod] public void PrivateProtectedBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("PrivateProtected","internal", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("PrivateProtected","private", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("PrivateProtected","protected", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("PrivateProtected","protected internal", "latest", true); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("PrivateProtected","private protected", "latest", true); }

        // FILE builder (requires latest language)
        [TestMethod] public void FileBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("File","public","latest", false); }
        [TestMethod] public void FileBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("File","internal","latest", true); }
        [TestMethod] public void FileBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("File","private","latest", true); }
        [TestMethod] public void FileBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("File","protected","latest", true); }
        [TestMethod] public void FileBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("File","protected internal","latest", true); }
        [TestMethod] public void FileBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("File","private protected","latest", true); }

        [TestMethod] public void FileBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("File","public","latest", false); }
        [TestMethod] public void FileBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("File","internal","latest", true); }
        [TestMethod] public void FileBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("File","private","latest", true); }
        [TestMethod] public void FileBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("File","protected","latest", true); }
        [TestMethod] public void FileBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("File","protected internal","latest", true); }
        [TestMethod] public void FileBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("File","private protected","latest", true); }

        // Helper invokers
        private static void RunTestTopLevel(string builder, string ctor, string lang, Boolean expected)
        {
            var source = 
$@"using FluentBuilder;

namespace Demo
{{
[FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builder})]
public partial class Order {{ {ctor} Order() {{ }} }}
}}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, ImmutableArray.Create<DiagnosticAnalyzer>(new ConstructorAccessibilityAnalyzer()), langVersion: lang);
            var matched = result.CompilationWarnings.ToList();
            var actual = result.CompilationWarnings.Any(d => d.Id == "FBBLD0007");
            Assert.AreEqual(expected, actual, source);
            if (!expected)
            {
                Assert.AreEqual(0, matched.Count, source);
            }
            else
            {
                Assert.IsTrue(matched.Count > 1, $"Expected more than 1 warning, found {matched.Count}.\n{source}");
            }
        }

        private static void RunTestNested(string builder, string ctor, string lang, Boolean expected)
        {
            var source = 
$@"using FluentBuilder;

namespace Demo
{{
public partial class Container {{
[FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builder})]
public partial class Order {{ {ctor} Order() {{ }} }}
}}
}}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, ImmutableArray.Create<DiagnosticAnalyzer>(new ConstructorAccessibilityAnalyzer()), langVersion: lang);
            var matched = result.CompilationWarnings.ToList();
            var actual = result.CompilationWarnings.Any(d => d.Id == "FBBLD0007");
            Assert.AreEqual(expected, actual, source);
            if (!expected)
            {
                Assert.AreEqual(0, matched.Count, source);
            }
            else
            {
                Assert.IsTrue(matched.Count > 1, $"Expected more than 1 warning, found {matched.Count}.\n{source}");
            }
        }
    }
}
