//---------------------------------------------------------------------
// <copyright file="UnresolvedEntityType.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class UnresolvedEntityType : BadEntityType, IUnresolvedElement
    {
        public UnresolvedEntityType(string qualifiedName, EdmLocation location)
            : base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEntityType, Strings.Bad_UnresolvedEntityType(qualifiedName)) })
        {
        }
    }
}
