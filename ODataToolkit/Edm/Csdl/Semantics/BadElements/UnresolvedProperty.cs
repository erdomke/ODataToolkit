//---------------------------------------------------------------------
// <copyright file="UnresolvedProperty.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class UnresolvedProperty : BadProperty, IUnresolvedElement
    {
        public UnresolvedProperty(IEdmStructuredType declaringType, string name, EdmLocation location)
            : base(declaringType, name, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedProperty, Strings.Bad_UnresolvedProperty(name)) })
        {
        }
    }
}
