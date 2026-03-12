// <copyright file="ExceptionLogger.Generator.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Generator
{
    internal static class ExceptionLogger
    {
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(Exception ex, string context = "")
        {
            if (!LoggerCore.IsLoggingEnabled) return;

            using var pooled = StringBuilderPool.Rent();
            var sb = pooled.Builder;

            sb.Append(LoggerCore.LogPrefix);
            sb.Append(" EXCEPTION: ");
            sb.Append(DateTime.Now.ToString("HH:mm:ss.fff"));
            sb.Append(" - ");
            sb.AppendLine(context);
            sb.Append("  Type: ");
            sb.AppendLine(ex.GetType().Name);
            sb.Append("  Message: ");
            sb.AppendLine(ex.Message);
            sb.Append("  StackTrace: ");
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.Append("  Inner Exception: ");
                sb.AppendLine(ex.InnerException.Message);
            }

            LoggerCore.WriteToDebug(sb.ToString());
        }
    }
}