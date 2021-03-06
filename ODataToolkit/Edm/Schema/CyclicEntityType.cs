//---------------------------------------------------------------------
// <copyright file="CyclicEntityType.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using ODataToolkit.Validation;

namespace ODataToolkit
{
    /// <summary>
    /// Represents an EDM entity type that cannot be determined due to a cyclic reference.
    /// </summary>
    internal class CyclicEntityType : BadEntityType
    {
        public CyclicEntityType(string qualifiedName, EdmLocation location)
            : base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadCyclicEntity, Strings.Bad_CyclicEntity(qualifiedName)) })
        {
        }
    }
}
