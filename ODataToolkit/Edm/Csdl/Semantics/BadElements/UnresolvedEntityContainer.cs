//---------------------------------------------------------------------
// <copyright file="UnresolvedEntityContainer.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class UnresolvedEntityContainer : BadEntityContainer, IUnresolvedElement
    {
        public UnresolvedEntityContainer(string name, EdmLocation location)
            : base(name, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEntityContainer, Strings.Bad_UnresolvedEntityContainer(name)) })
        {
        }
    }
}
