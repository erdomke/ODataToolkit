//---------------------------------------------------------------------
// <copyright file="CsdlSemanticsFunction.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Csdl.Parsing.Ast;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    /// <summary>
    /// Provides semantics for a CsdlAction
    /// </summary>
    internal class CsdlSemanticsFunction : CsdlSemanticsOperation, IEdmFunction
    {
        private readonly CsdlFunction function;

        public CsdlSemanticsFunction(CsdlSemanticsSchema context, CsdlFunction function)
            : base(context, function)
        {
            this.function = function;
        }

        public bool IsComposable
        {
            get { return this.function.IsComposable; }
        }

        public override EdmSchemaElementKind SchemaElementKind
        {
            get { return EdmSchemaElementKind.Function; }
        }
    }
}
