// <copyright file="DebugLogger.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using FluentBuilder.Generator.Pools;
using FluentBuilder.Generator.Services; // For LoggerCore
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace FluentBuilder.Generator.Generator
{
    internal static class DebugLogger
    {
        [Conditional("DEBUG")]
        public static void LogDebug(LogLevel level, DiagnosticDescriptor descriptor, object[] messageArgs, string? additionalInfo = null)
        {
            if (!LoggerCore.IsLoggingEnabled) return;

            using var pooled = StringBuilderPool.Rent();
            var sb = pooled.Builder;

            sb.Append(LoggerCore.LogPrefix);
            sb.Append(' ');
            sb.Append(LogLevelFormatter.GetPrefix(level));
            sb.Append(" [");
            sb.Append(descriptor.Id);
            sb.Append("] ");
            sb.Append(descriptor.Title);
            sb.Append(" - ");
            sb.AppendFormat(descriptor.MessageFormat.ToString(), messageArgs);

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                sb.Append(" at ");
                sb.Append(additionalInfo);
            }

            LoggerCore.WriteToDebug(sb.ToString());
        }
    }
}