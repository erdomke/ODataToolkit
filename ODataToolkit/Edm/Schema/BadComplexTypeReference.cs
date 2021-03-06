﻿//---------------------------------------------------------------------
// <copyright file="BadComplexTypeReference.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ODataToolkit.Validation;

namespace ODataToolkit
{
    internal class BadComplexTypeReference : EdmComplexTypeReference, IEdmCheckable
    {
        private readonly IEnumerable<EdmError> errors;

        public BadComplexTypeReference(string qualifiedName, bool isNullable, IEnumerable<EdmError> errors)
            : base(new BadComplexType(qualifiedName, errors), isNullable)
        {
            this.errors = errors;
        }

        public IEnumerable<EdmError> Errors
        {
            get { return this.errors; }
        }

        public override string ToString()
        {
            EdmError error = this.Errors.FirstOrDefault();
            Debug.Assert(error != null, "error != null");
            string prefix = error != null ? error.ErrorCode.ToString() + ":" : "";
            return prefix + this.ToTraceString();
        }
    }
}
