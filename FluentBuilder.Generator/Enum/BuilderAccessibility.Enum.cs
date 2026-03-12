// <copyright file="BuilderAccessibility.Enum.cs" company="Adriaan Jacobus van Rensburg a.k.a Billy">
//     Copyright (c) Adriaan Jacobus van Rensburg a.k.a Billy. All rights reserved.
// </copyright>
// <license>
//     This source code is licensed under the MIT license found in the
//     LICENSE file in the root directory of this source tree.
// </license>
// <author>Adriaan Jacobus van Rensburg a.k.a Billy</author>
// <summary>FluentBuilder source generator implementation.</summary>

namespace FluentBuilder
{
    /// <summary>
    /// Defines the allowed accessibility modifiers for a generated builder class.
    /// </summary>
    /// <remarks>
    /// These values correspond to C# access modifiers. Not all values are valid for every context;
    /// for example, top‑level types may only use <see cref="Public"/> or <see cref="Internal"/>.
    /// The generator uses this enumeration to validate the <see cref="FluentBuilderAttribute.BuilderAccessibility"/> property.
    /// </remarks>
    public enum BuilderAccessibility
    {
        /// <summary>
        /// The builder class is public – accessible from any other code.
        /// </summary>
        Public,

        /// <summary>
        /// The builder class is internal – accessible only within the same assembly.
        /// </summary>
        Internal,

        /// <summary>
        /// The builder class is private – accessible only within the containing type.
        /// Valid only for nested builder classes.
        /// </summary>
        Private,

        /// <summary>
        /// The builder class is protected – accessible within the containing class and derived types.
        /// Valid only for nested builder classes.
        /// </summary>
        Protected,

        /// <summary>
        /// The builder class is protected internal – accessible within the same assembly and from derived classes.
        /// Valid only for nested builder classes.
        /// </summary>
        ProtectedInternal,

        /// <summary>
        /// The builder class is private protected – accessible only within the same class or derived types in the same assembly.
        /// Valid only for nested builder classes.
        /// </summary>
        PrivateProtected,

        /// <summary>
        /// The builder class is file‑scoped – accessible only within the same source file.
        /// Introduced in C# 11. Valid only for top‑level types.
        /// </summary>
        File
    }
}