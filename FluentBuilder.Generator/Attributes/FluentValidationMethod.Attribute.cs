// <copyright file="FluentValidationMethod.Attribute.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
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
    /// Marks a method as a custom validation method to be automatically called during <c>Build()</c> or <c>BuildAsync()</c>.
    /// </summary>
    /// <remarks>
    /// <para>The method must be public, instance, and parameterless. It can return:</para>
    /// <list type="bullet">
    /// <item><description><c>void</c> – simply executed.</description></item>
    /// <item><description><c>bool</c> – if <c>false</c>, an exception is thrown.</description></item>
    /// <item><description><c>Task</c> – awaited during async build.</description></item>
    /// <item><description><c>Task&lt;bool&gt;</c> – if the result is <c>false</c>, an exception is thrown.</description></item>
    /// </list>
    /// <para>In synchronous builds, async methods are ignored; in async builds, both sync and async methods are called.</para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// [FluentValidationMethod]
    /// public bool ValidateTotal() => Items.Sum(i => i.Price) > 0;
    /// 
    /// [FluentValidationMethod]
    /// public async Task<bool> CheckInventoryAsync() => await inventoryService.CheckAsync(Items);
    /// ]]></code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FluentValidationMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the execution order (lower numbers first). Not yet implemented.
        /// </summary>
        public int Order { get; set; } = 0;
    }
}