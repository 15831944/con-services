﻿using System;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels.Interfaces.Events
{
  public interface ISiteModelAttributesChangedEventSender
  {
    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    /// <param name="siteModelID"></param>
    void ModelAttributesChanged(StorageMutability targetGrid, Guid siteModelID, bool existenceMapChanged = false,
      bool designsChanged = false, bool surveyedSurfacesChanged = false, bool CsibChanged = false,
      bool machinesChanged = false, bool machineTargetValuesChanged = false);
  }
}
