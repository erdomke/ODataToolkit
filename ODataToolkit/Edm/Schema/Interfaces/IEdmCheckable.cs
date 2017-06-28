//---------------------------------------------------------------------
// <copyright file="IEdmCheckable.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using ODataToolkit.Validation;

namespace ODataToolkit
{
    /// <summary>
    /// Defines an Edm component who is invalid or whose validity is unknown at construction
    /// </summary>
    public interface IEdmCheckable
    {
        /// <summary>
        /// Gets an error if one exists with the current object.
        /// </summary>
        IEnumerable<EdmError> Errors { get; }
    }
}
