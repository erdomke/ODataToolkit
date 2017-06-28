//---------------------------------------------------------------------
// <copyright file="EdmUntypedTypeReference.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;

namespace ODataToolkit
{
    /// <summary>
    /// Represents a reference to an EDM Untyped type.
    /// </summary>
    public class EdmUntypedTypeReference : EdmTypeReference, IEdmUntypedTypeReference
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definition">IEdmUntypedType definition.</param>
        public EdmUntypedTypeReference(IEdmUntypedType definition)
            : base(definition, true)
        {
        }
    }
}
