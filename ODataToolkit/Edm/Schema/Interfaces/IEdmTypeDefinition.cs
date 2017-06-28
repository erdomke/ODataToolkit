//---------------------------------------------------------------------
// <copyright file="IEdmTypeDefinition.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace ODataToolkit
{
    /// <summary>
    /// Represents a definition of an EDM type definition.
    /// </summary>
    public interface IEdmTypeDefinition : IEdmSchemaType
    {
        /// <summary>
        /// Gets the underlying type of this type definition.
        /// </summary>
        IEdmPrimitiveType UnderlyingType { get; }
    }
}
