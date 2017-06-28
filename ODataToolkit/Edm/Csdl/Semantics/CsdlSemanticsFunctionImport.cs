//---------------------------------------------------------------------
// <copyright file="CsdlSemanticsFunctionImport.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Csdl.Parsing.Ast;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    internal class CsdlSemanticsFunctionImport : CsdlSemanticsOperationImport, IEdmFunctionImport
    {
        private readonly CsdlFunctionImport functionImport;
        private readonly CsdlSemanticsSchema csdlSchema;

        public CsdlSemanticsFunctionImport(CsdlSemanticsEntityContainer container, CsdlFunctionImport functionImport, IEdmFunction backingfunction)
            : base(container, functionImport, backingfunction)
        {
            this.csdlSchema = container.Context;
            this.functionImport = functionImport;
        }

        public IEdmFunction Function
        {
            get { return (IEdmFunction)this.Operation; }
        }

        public bool IncludeInServiceDocument
        {
            get { return this.functionImport.IncludeInServiceDocument; }
        }

        public override EdmContainerElementKind ContainerElementKind
        {
            get { return EdmContainerElementKind.FunctionImport; }
        }
    }
}
