//---------------------------------------------------------------------
// <copyright file="IEdmSpatialTypeReference.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace ODataToolkit
{
    /// <summary>
    /// Represents a reference to an EDM spatial type.
    /// </summary>
    public interface IEdmSpatialTypeReference : IEdmPrimitiveTypeReference
    {
        /// <summary>
        /// Gets the Spatial Reference Identifier of this spatial type.
        /// </summary>
        int? SpatialReferenceIdentifier { get; }
    }
}
