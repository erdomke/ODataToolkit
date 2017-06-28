//---------------------------------------------------------------------
// <copyright file="UnresolvedNavigationPropertyPath.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace ODataToolkit.Csdl.CsdlSemantics
{
    using ODataToolkit.Validation;

    /// <summary>
    /// Represents a navigation property path that could not be resolved.
    /// </summary>
    internal class UnresolvedNavigationPropertyPath : BadNavigationProperty, IUnresolvedElement
    {
        public UnresolvedNavigationPropertyPath(IEdmStructuredType startingType, string path, EdmLocation location)
            : base(startingType, path, new[] { new EdmError(location, EdmErrorCode.BadUnresolvedNavigationPropertyPath, Strings.Bad_UnresolvedNavigationPropertyPath(path, startingType.FullTypeName())) })
        {
        }
    }
}