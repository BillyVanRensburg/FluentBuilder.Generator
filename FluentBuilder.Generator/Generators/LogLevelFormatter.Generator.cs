// <copyright file="LogLevelFormatter.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Generator
{
    internal static class LogLevelFormatter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPrefix(LogLevel level) => level switch
        {
            LogLevel.Error => nameof(LogLevel.Error),
            LogLevel.Warning => nameof(LogLevel.Warning),
            LogLevel.Info => nameof(LogLevel.Info),
            LogLevel.Debug => nameof(LogLevel.Debug),
            _ => "LOG"
        };
    }
}