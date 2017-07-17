﻿
using VSS.Productivity3D.FileAccess.Service.Common.ResultHandling;

namespace VSS.Productivity3D.FileAccess.Service.Common.Interfaces
{
    /// <summary>
    /// Defines if a domain object can have business validation
    /// </summary>
    public interface IValidatable 
    {
        /// <summary>
        /// Validate domain object. If validation is not successful throw <see cref="ServiceException" />
        /// </summary>
        void Validate();
    }
}