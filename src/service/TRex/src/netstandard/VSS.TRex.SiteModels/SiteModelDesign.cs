﻿using System;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Describes a single design used in the site model. It's chief purpose is to record the name of the design 
  /// and the plan extents of the cell pass information stored within the site model for that design.
  /// </summary>
  public class SiteModelDesign : IEquatable<string>, ISiteModelDesign
  {
    public string Name { get; }

    public BoundingWorldExtent3D Extents { get; set; } = new BoundingWorldExtent3D();

    public SiteModelDesign()
    {
      Extents.Clear();
    }

    public SiteModelDesign(string name, BoundingWorldExtent3D extents) : this()
    {
      Name = name;
      Extents = extents;
    }

    public bool Equals(string other) => other != null && Name.Equals(other);
  }
}
