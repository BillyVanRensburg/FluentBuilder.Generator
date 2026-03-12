// <copyright file="UsingDirectives.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Managers;
using System.Text;

namespace FluentBuilder.Generator.Generator
{
    internal static class UsingDirectivesGenerator
    {
        public static void GenerateUsings(StringBuilder sb, UsingDirectiveManager usingManager)
        {
            usingManager.EmitUsings(sb);
            sb.AppendLine();
        }
    }
}