//---------------------------------------------------------------------
// <copyright file="UnresolvedLabeledElement.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class UnresolvedLabeledElement : BadLabeledExpression, IUnresolvedElement
    {
        public UnresolvedLabeledElement(string label, EdmLocation location)
            : base(label, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedLabeledElement, Strings.Bad_UnresolvedLabeledElement(label)) })
        {
        }
    }
}
