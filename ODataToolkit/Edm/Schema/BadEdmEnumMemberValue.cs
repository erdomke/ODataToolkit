//---------------------------------------------------------------------
// <copyright file="BadEdmEnumMemberValue.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using ODataToolkit.Validation;

namespace ODataToolkit
{
    internal class BadEdmEnumMemberValue : BadElement, IEdmEnumMemberValue
    {
        public BadEdmEnumMemberValue(IEnumerable<EdmError> errors)
            : base(errors)
        {
        }

        public long Value
        {
            get { return 0; }
        }
    }
}
