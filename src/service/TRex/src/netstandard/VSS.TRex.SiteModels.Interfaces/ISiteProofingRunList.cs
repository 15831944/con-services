﻿using System;
using System.Collections.Generic;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteProofingRunList : IList<ISiteProofingRun>, IBinaryReaderWriter
  {
    /// <summary>
    /// The identifier of the site model owning this list of proofing runs.
    /// </summary>
    Guid DataModelID { get; set; }

    bool CreateAndAddProofingRun(string name, short machineID, DateTime startTime, DateTime endTime, BoundingWorldExtent3D extents);

    void SaveToPersistentStore(IStorageProxy storageProxy);

    void RemoveFromPersistentStore(IStorageProxy storageProxy);

    void LoadFromPersistentStore(IStorageProxy storageProxy);
  }
}
