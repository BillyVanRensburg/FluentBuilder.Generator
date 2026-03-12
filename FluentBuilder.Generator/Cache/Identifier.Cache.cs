// <copyright file="Identifier.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Implementations;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// Cache for C# identifier validation.
    /// </summary>
    internal static class IdentifierCache
    {
        private static readonly FrozenSet<string> _csharpKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal",
            "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly",
            "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
            "static", "string", "struct", "switch", "this", "throw", "true", "try",
            "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while", "init", "record", "with",
            "add", "alias", "and", "ascending", "async", "await", "by", "descending",
            "dynamic", "equals", "from", "get", "global", "group", "into", "join",
            "let", "managed", "nameof", "nint", "not", "notnull", "nuint", "on",
            "or", "orderby", "partial", "remove", "select", "set", "unmanaged",
            "value", "var", "when", "where", "yield"
        }.ToFrozenSet();

        private static readonly StrongCache<string, bool> _identifierCache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length == 0)
                return false;

            return _identifierCache.GetOrAdd(name, static n =>
            {
                if (!char.IsLetter(n[0]) && n[0] != '_')
                    return false;

                for (int i = 1; i < n.Length; i++)
                {
                    if (!char.IsLetterOrDigit(n[i]) && n[i] != '_')
                        return false;
                }

                return !_csharpKeywords.Contains(n);
            });
        }

        public static void Clear() => _identifierCache.Clear();
    }
}