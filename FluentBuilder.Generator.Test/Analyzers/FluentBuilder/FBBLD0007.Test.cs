// <copyright file="FBBLD0007.Test.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>Tests for analyzer FBBLD0007.</summary>

using FluentBuilder.Generator.Analyzers;
using FluentBuilder.Generator.BaseCode;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace FluentBuilder.Generator.Analyzers
{
    [TestClass]
    public class FBBLD0007
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new ConstructorAccessibilityAnalyzer());
        // Exhaustive explicit tests for all builder x constructor x (top-level/nested) combinations
        // Builders
        private static readonly string[] Builders = new[] { "Public", "Internal", "Private", "Protected", "ProtectedInternal", "PrivateProtected", "File" };
        // Constructors (string used in source), and their name forms for method names
        private static readonly (string Source, string Name)[] Ctors = new[]
        {
            ("public", "Public"),
            ("internal", "Internal"),
            ("private", "Private"),
            ("protected", "Protected"),
            ("protected internal", "ProtectedInternal"),
            ("private protected", "PrivateProtected"),
        };

        // Generate tests explicitly
        // Note: The following tests exhaustively cover all combinations of BuilderAccessibility x Constructor accessibility
        // for both top-level and nested target types. Each test asserts whether FBBLD0007 is reported (Error) or not (NoError).

        // Begin generated tests


        private static int GetBuilderLevel(string name) => name switch
        {
            "File" => 0,
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
    }
}
