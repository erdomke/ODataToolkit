//---------------------------------------------------------------------
// <copyright file="CsdlSemanticsCollectionTypeExpression.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Csdl.Parsing.Ast;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class CsdlSemanticsCollectionTypeExpression : CsdlSemanticsTypeExpression, IEdmCollectionTypeReference
    {
        public CsdlSemanticsCollectionTypeExpression(CsdlExpressionTypeReference expressionUsage, CsdlSemanticsTypeDefinition type)
            : base(expressionUsage, type)
        {
        }
    }
}
