//---------------------------------------------------------------------
// <copyright file="CsdlSemanticsBinaryTypeReference.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Csdl.Parsing.Ast;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    /// <summary>
    /// Provides the semantics of a reference to an EDM Binary type.
    /// </summary>
    internal class CsdlSemanticsBinaryTypeReference : CsdlSemanticsPrimitiveTypeReference, IEdmBinaryTypeReference
    {
        public CsdlSemanticsBinaryTypeReference(CsdlSemanticsSchema schema, CsdlBinaryTypeReference reference)
            : base(schema, reference)
        {
        }

        public bool IsUnbounded
        {
            get { return ((CsdlBinaryTypeReference)this.Reference).IsUnbounded; }
        }

        public int? MaxLength
        {
            get { return ((CsdlBinaryTypeReference)this.Reference).MaxLength; }
        }
    }
}
