//---------------------------------------------------------------------
// <copyright file="IEdmSingleton.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace ODataToolkit
{
    /// <summary>
    /// Represents an EDM singleton.
    /// </summary>
    public interface IEdmSingleton : IEdmEntityContainerElement, IEdmNavigationSource
    {
    }
}
