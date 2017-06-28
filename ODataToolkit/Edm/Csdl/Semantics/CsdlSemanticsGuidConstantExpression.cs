//---------------------------------------------------------------------
// <copyright file="CsdlSemanticsGuidConstantExpression.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using ODataToolkit.Csdl.Parsing.Ast;
using ODataToolkit.Validation;
using ODataToolkit.Vocabularies;

namespace ODataToolkit.Csdl.CsdlSemantics
{
    /// <summary>
    /// Provides semantics for a Csdl guid constant expression.
    /// </summary>
    internal class CsdlSemanticsGuidConstantExpression : CsdlSemanticsExpression, IEdmGuidConstantExpression, IEdmCheckable
    {
        private readonly CsdlConstantExpression expression;

        private readonly Cache<CsdlSemanticsGuidConstantExpression, Guid> valueCache = new Cache<CsdlSemanticsGuidConstantExpression, Guid>();
        private static readonly Func<CsdlSemanticsGuidConstantExpression, Guid> ComputeValueFunc = (me) => me.ComputeValue();

        private readonly Cache<CsdlSemanticsGuidConstantExpression, IEnumerable<EdmError>> errorsCache = new Cache<CsdlSemanticsGuidConstantExpression, IEnumerable<EdmError>>();
        private static readonly Func<CsdlSemanticsGuidConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc = (me) => me.ComputeErrors();

        public CsdlSemanticsGuidConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema)
            : base(schema, expression)
        {
            this.expression = expression;
        }

        public override CsdlElement Element
        {
            get { return this.expression; }
        }

        public Guid Value
        {
            get { return this.valueCache.GetValue(this, ComputeValueFunc, null); }
        }

        public IEdmTypeReference Type
        {
            get { return null; }
        }

        public override EdmExpressionKind ExpressionKind
        {
            get { return EdmExpressionKind.GuidConstant; }
        }

        public EdmValueKind ValueKind
        {
            get { return this.expression.ValueKind; }
        }

        public IEnumerable<EdmError> Errors
        {
            get { return this.errorsCache.GetValue(this, ComputeErrorsFunc, null); }
        }

        private Guid ComputeValue()
        {
            Guid? value;
            return EdmValueParser.TryParseGuid(this.expression.Value, out value) ? value.Value : Guid.Empty;
        }

        private IEnumerable<EdmError> ComputeErrors()
        {
            Guid? value;
            if (!EdmValueParser.TryParseGuid(this.expression.Value, out value))
            {
                return new EdmError[] { new EdmError(this.Location, EdmErrorCode.InvalidGuid, Strings.ValueParser_InvalidGuid(this.expression.Value)) };
            }
            else
            {
                return Enumerable.Empty<EdmError>();
            }
        }
    }
}
