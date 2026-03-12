// <copyright file="FluentInclude.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

using System;

namespace FluentBuilder
{
    /// <summary>
    /// Explicitly includes a member, constructor, or method in the builder even if it is not public.
    /// </summary>
    /// <remarks>
    /// By default, only public instance members are included. Apply this attribute to internal members
    /// to include them as well. The member must still be accessible to the generated builder
    /// (i.e., in the same assembly).
    /// </remarks>
    /// <example>
    /// <code>
    /// [FluentInclude]
    /// internal string InternalName { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method |
                    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class FluentIncludeAttribute : Attribute { }
}