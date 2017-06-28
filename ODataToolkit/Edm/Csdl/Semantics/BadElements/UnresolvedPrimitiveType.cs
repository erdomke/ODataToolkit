//---------------------------------------------------------------------
// <copyright file="UnresolvedPrimitiveType.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class UnresolvedPrimitiveType : BadPrimitiveType, IUnresolvedElement
    {
        public UnresolvedPrimitiveType(string qualifiedName, EdmLocation location)
            : base(qualifiedName, EdmPrimitiveTypeKind.None, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedPrimitiveType, Strings.Bad_UnresolvedPrimitiveType(qualifiedName)) })
        {
        }
    }
}
