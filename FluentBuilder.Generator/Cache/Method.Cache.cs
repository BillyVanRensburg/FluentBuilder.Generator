// <copyright file="Method.Cache.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FluentBuilder.Generator.Caching
{
    /// <summary>
    /// Cache for method-related information.
    /// </summary>
    internal static class MethodCache
    {
        private class MethodInfo
        {
            public bool? IsAsync { get; set; }
            public string? AsyncMethodName { get; set; }
            public ConcurrentDictionary<string, bool> ParameterCheckCache { get; } = new();
            public bool? HasCancellationToken { get; set; }
        }

        private static readonly WeakCache<IMethodSymbol, MethodInfo> _cache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsyncMethod(IMethodSymbol method)
        {
            var info = _cache.GetOrCreateValue(method);
            return info.IsAsync ??= CheckIsAsync(method);
        }

        private static bool CheckIsAsync(IMethodSymbol method)
        {
            if (method == null)
                return false;

            if (method.IsAsync)
                return true;

            if (method.ReturnType is INamedTypeSymbol returnType)
            {
                var returnTypeName = returnType.ToDisplayString();

                if (returnTypeName.StartsWith("System.Threading.Tasks.Task"))
                    return true;

                if (returnTypeName.StartsWith("System.Threading.Tasks.ValueTask"))
                    return true;

                if (returnType.OriginalDefinition != null)
                {
                    var originalDefName = returnType.OriginalDefinition.ToDisplayString();
                    return originalDefName == "System.Threading.Tasks.Task<TResult>" ||
                           originalDefName == "System.Threading.Tasks.ValueTask<TResult>" ||
                           originalDefName == "System.Threading.Tasks.Task" ||
                           originalDefName == "System.Threading.Tasks.ValueTask";
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAsyncMethodName(IMethodSymbol method)
        {
            if (method == null) return string.Empty;

            var info = _cache.GetOrCreateValue(method);
            return info.AsyncMethodName ??= BuildAsyncMethodName(method);
        }

        private static string BuildAsyncMethodName(IMethodSymbol method)
        {
            var attr = AttributeCache.GetAttribute(method, "FluentAsyncMethod");

            if (attr != null)
            {
                var syncName = AttributeCache.GetNamedArgument<string>(attr, "SyncMethodName");
                if (!string.IsNullOrEmpty(syncName))
                    return syncName!;
            }

            if (method.Name.EndsWith("Async"))
                return method.Name.Substring(0, method.Name.Length - 5);

            return method.Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasParameter(IMethodSymbol method, string parameterName)
        {
            var info = _cache.GetOrCreateValue(method);
            return info.ParameterCheckCache.GetOrAdd(parameterName, name =>
            {
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    if (method.Parameters[i].Name == name)
                        return true;
                }
                return false;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCancellationTokenParameter(IMethodSymbol method)
        {
            var info = _cache.GetOrCreateValue(method);
            return info.HasCancellationToken ??= CheckHasCancellationToken(method);
        }

        private static bool CheckHasCancellationToken(IMethodSymbol method)
        {
            foreach (var param in method.Parameters)
            {
                if (param.Type.ToDisplayString() == "System.Threading.CancellationToken")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if any method with the given name on the type has a CancellationToken parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasMethodWithCancellationToken(ITypeSymbol type, string methodName)
        {
            if (type == null) return false;
            var members = type.GetMembers(methodName).OfType<IMethodSymbol>();
            foreach (var method in members)
            {
                if (HasCancellationTokenParameter(method))
                    return true;
            }
            return false;
        }

        public static void Clear() => _cache.Clear();
    }
}