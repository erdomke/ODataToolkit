//---------------------------------------------------------------------
// <copyright file="UnresolvedEntitySet.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class UnresolvedEntitySet : BadEntitySet, IUnresolvedElement
    {
        public UnresolvedEntitySet(string name, IEdmEntityContainer container, EdmLocation location)
            : base(name, container, new[] { new EdmError(location, EdmErrorCode.BadUnresolvedEntitySet, Strings.Bad_UnresolvedEntitySet(name)) })
        {
        }
    }
}
