//---------------------------------------------------------------------
// <copyright file="CyclicComplexType.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit
{
    /// <summary>
    /// Represents an EDM complex type that cannot be determined due to a cyclic reference.
    /// </summary>
    internal class CyclicComplexType : BadComplexType
    {
        public CyclicComplexType(string qualifiedName, EdmLocation location)
            : base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadCyclicComplex, Strings.Bad_CyclicComplex(qualifiedName)) })
        {
        }
    }
}