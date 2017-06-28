//---------------------------------------------------------------------
// <copyright file="BadComplexType.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using ODataToolkit.Validation;
using ODataToolkit.Vocabularies;

namespace ODataToolkit
{
    /// <summary>
    /// Represents a semantically invalid EDM complex type definition.
    /// </summary>
    internal class BadComplexType : BadNamedStructuredType, IEdmComplexType
    {
        public BadComplexType(string qualifiedName, IEnumerable<EdmError> errors)
            : base(qualifiedName, errors)
        {
        }

        public override EdmTypeKind TypeKind
        {
            get { return EdmTypeKind.Complex; }
        }
    }
}
