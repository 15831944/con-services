﻿using System;
using VSS.TRex.Geometry;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMetadata
  {
    Guid ID { get; }
    DateTime CreationDate { get; set; }
    DateTime LastModifiedDate { get; set; }
    BoundingWorldExtent3D SiteModelExtent { get; set; }
    int MachineCount { get; set; }
    int DesignCount { get; set; }
    int SurveyedSurfaceCount { get; set; }
    int AlignmentCount { get; set; }
  }
}
