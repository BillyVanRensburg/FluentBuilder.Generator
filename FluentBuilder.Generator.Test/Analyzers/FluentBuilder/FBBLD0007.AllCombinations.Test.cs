// Auto-generated exhaustive tests for FBBLD0007
using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [TestClass]
    public class FBBLD0007_AllCombinations
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
        [TestMethod] public void PublicBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Public","public"); }
        [TestMethod] public void PublicBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Public","internal"); }
        [TestMethod] public void PublicBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("Public","private"); }
        [TestMethod] public void PublicBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Public","protected"); }
        [TestMethod] public void PublicBuilder_WithProtectedInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("Public","protected internal"); }
        [TestMethod] public void PublicBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Public","private protected"); }

        [TestMethod] public void PublicBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Public","public"); }
        [TestMethod] public void PublicBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("Public","internal"); }
        [TestMethod] public void PublicBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("Public","private"); }
        [TestMethod] public void PublicBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("Public","protected"); }
        [TestMethod] public void PublicBuilder_WithProtectedInternalConstructor_Nested_NoWarning() { RunTestNested("Public","protected internal"); }
        [TestMethod] public void PublicBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("Public","private protected"); }

        // INTERNAL builder
        [TestMethod] public void InternalBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Internal","public"); }
        [TestMethod] public void InternalBuilder_WithInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("Internal","internal"); }
        [TestMethod] public void InternalBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","private"); }
        [TestMethod] public void InternalBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","protected"); }
        [TestMethod] public void InternalBuilder_WithProtectedInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("Internal","protected internal"); }
        [TestMethod] public void InternalBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("Internal","private protected"); }

        [TestMethod] public void InternalBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Internal","public"); }
        [TestMethod] public void InternalBuilder_WithInternalConstructor_Nested_NoWarning() { RunTestNested("Internal","internal"); }
        [TestMethod] public void InternalBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("Internal","private"); }
        [TestMethod] public void InternalBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("Internal","protected"); }
        [TestMethod] public void InternalBuilder_WithProtectedInternalConstructor_Nested_NoWarning() { RunTestNested("Internal","protected internal"); }
        [TestMethod] public void InternalBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("Internal","private protected"); }

        // PRIVATE builder
        [TestMethod] public void PrivateBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","public"); }
        [TestMethod] public void PrivateBuilder_WithInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","internal"); }
        [TestMethod] public void PrivateBuilder_WithPrivateConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","private"); }
        [TestMethod] public void PrivateBuilder_WithProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","protected"); }
        [TestMethod] public void PrivateBuilder_WithProtectedInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","protected internal"); }
        [TestMethod] public void PrivateBuilder_WithPrivateProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("Private","private protected"); }

        [TestMethod] public void PrivateBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Private","public"); }
        [TestMethod] public void PrivateBuilder_WithInternalConstructor_Nested_NoWarning() { RunTestNested("Private","internal"); }
        [TestMethod] public void PrivateBuilder_WithPrivateConstructor_Nested_NoWarning() { RunTestNested("Private","private"); }
        [TestMethod] public void PrivateBuilder_WithProtectedConstructor_Nested_NoWarning() { RunTestNested("Private","protected"); }
        [TestMethod] public void PrivateBuilder_WithProtectedInternalConstructor_Nested_NoWarning() { RunTestNested("Private","protected internal"); }
        [TestMethod] public void PrivateBuilder_WithPrivateProtectedConstructor_Nested_NoWarning() { RunTestNested("Private","private protected"); }

        // PROTECTED builder
        [TestMethod] public void ProtectedBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("Protected","public"); }
        [TestMethod] public void ProtectedBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("Protected","internal"); }
        [TestMethod] public void ProtectedBuilder_WithPrivateConstructor_TopLevel_NoWarning() { RunTestTopLevel("Protected","private"); }
        [TestMethod] public void ProtectedBuilder_WithProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("Protected","protected"); }
        [TestMethod] public void ProtectedBuilder_WithProtectedInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("Protected","protected internal"); }
        [TestMethod] public void ProtectedBuilder_WithPrivateProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("Protected","private protected"); }

        [TestMethod] public void ProtectedBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("Protected","public"); }
        [TestMethod] public void ProtectedBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("Protected","internal"); }
        [TestMethod] public void ProtectedBuilder_WithPrivateConstructor_Nested_NoWarning() { RunTestNested("Protected","private"); }
        [TestMethod] public void ProtectedBuilder_WithProtectedConstructor_Nested_NoWarning() { RunTestNested("Protected","protected"); }
        [TestMethod] public void ProtectedBuilder_WithProtectedInternalConstructor_Nested_NoWarning() { RunTestNested("Protected","protected internal"); }
        [TestMethod] public void ProtectedBuilder_WithPrivateProtectedConstructor_Nested_NoWarning() { RunTestNested("Protected","private protected"); }

        // PROTECTEDINTERNAL builder
        [TestMethod] public void ProtectedInternalBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("ProtectedInternal","public"); }
        [TestMethod] public void ProtectedInternalBuilder_WithInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("ProtectedInternal","internal"); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","private"); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("ProtectedInternal","protected"); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("ProtectedInternal","protected internal"); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("ProtectedInternal","private protected"); }

        [TestMethod] public void ProtectedInternalBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("ProtectedInternal","public"); }
        [TestMethod] public void ProtectedInternalBuilder_WithInternalConstructor_Nested_NoWarning() { RunTestNested("ProtectedInternal","internal"); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","private"); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedConstructor_Nested_NoWarning() { RunTestNested("ProtectedInternal","protected"); }
        [TestMethod] public void ProtectedInternalBuilder_WithProtectedInternalConstructor_Nested_NoWarning() { RunTestNested("ProtectedInternal","protected internal"); }
        [TestMethod] public void ProtectedInternalBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("ProtectedInternal","private protected"); }

        // PRIVATEPROTECTED builder
        [TestMethod] public void PrivateProtectedBuilder_WithPublicConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","public"); }
        [TestMethod] public void PrivateProtectedBuilder_WithInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","internal"); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","private"); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","protected"); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedInternalConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","protected internal"); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateProtectedConstructor_TopLevel_NoWarning() { RunTestTopLevel("PrivateProtected","private protected"); }

        [TestMethod] public void PrivateProtectedBuilder_WithPublicConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","public"); }
        [TestMethod] public void PrivateProtectedBuilder_WithInternalConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","internal"); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","private"); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","protected"); }
        [TestMethod] public void PrivateProtectedBuilder_WithProtectedInternalConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","protected internal"); }
        [TestMethod] public void PrivateProtectedBuilder_WithPrivateProtectedConstructor_Nested_NoWarning() { RunTestNested("PrivateProtected","private protected"); }

        // FILE builder (requires latest language)
        [TestMethod] public void FileBuilder_WithPublicConstructor_TopLevel_Warning() { RunTestTopLevel("File","public","latest"); }
        [TestMethod] public void FileBuilder_WithInternalConstructor_TopLevel_Warning() { RunTestTopLevel("File","internal","latest"); }
        [TestMethod] public void FileBuilder_WithPrivateConstructor_TopLevel_Warning() { RunTestTopLevel("File","private","latest"); }
        [TestMethod] public void FileBuilder_WithProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("File","protected","latest"); }
        [TestMethod] public void FileBuilder_WithProtectedInternalConstructor_TopLevel_Warning() { RunTestTopLevel("File","protected internal","latest"); }
        [TestMethod] public void FileBuilder_WithPrivateProtectedConstructor_TopLevel_Warning() { RunTestTopLevel("File","private protected","latest"); }

        [TestMethod] public void FileBuilder_WithPublicConstructor_Nested_Warning() { RunTestNested("File","public","latest"); }
        [TestMethod] public void FileBuilder_WithInternalConstructor_Nested_Warning() { RunTestNested("File","internal","latest"); }
        [TestMethod] public void FileBuilder_WithPrivateConstructor_Nested_Warning() { RunTestNested("File","private","latest"); }
        [TestMethod] public void FileBuilder_WithProtectedConstructor_Nested_Warning() { RunTestNested("File","protected","latest"); }
        [TestMethod] public void FileBuilder_WithProtectedInternalConstructor_Nested_Warning() { RunTestNested("File","protected internal","latest"); }
        [TestMethod] public void FileBuilder_WithPrivateProtectedConstructor_Nested_Warning() { RunTestNested("File","private protected","latest"); }

        // Helper invokers
        private static void RunTestTopLevel(string builder, string ctor, string lang = null)
        {
            var source = 
$@"using FluentBuilder;

namespace Demo
{{
[FluentBuilder(BuilderAccessibility = BuilderAccessibility.{builder})]
public partial class Order {{ {ctor} Order() {{ }} }}
}}";
            var result = BaseCode.Action.RunGeneratorAndCompile(source, ImmutableArray.Create<DiagnosticAnalyzer>(new ConstructorAccessibilityAnalyzer()), langVersion: lang);
            var actual = result.CompilationWarnings.Any(d => d.Id == "FBBLD0007");
            var expected = GetCtorLevel(ctor) < GetBuilderLevel(builder);
            Assert.AreEqual(expected, actual, source);
        }

        private static void RunTestNested(string builder, string ctor, string lang = null)
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
            var actual = result.CompilationWarnings.Any(d => d.Id == "FBBLD0007");
            var expected = GetCtorLevel(ctor) < GetBuilderLevel(builder);
            Assert.AreEqual(expected, actual, source);
        }
    }
}
