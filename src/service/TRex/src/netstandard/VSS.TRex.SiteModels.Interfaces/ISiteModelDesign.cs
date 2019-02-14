﻿using VSS.TRex.Geometry;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelDesign
  {
    string Name { get; }
    BoundingWorldExtent3D Extents { get; set; }
    bool MatchesDesignName(string other);
  }
}
