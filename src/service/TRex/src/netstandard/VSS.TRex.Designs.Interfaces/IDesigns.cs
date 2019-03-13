﻿using System;
using System.Collections.Generic;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesigns : IList<IDesign>, IBinaryReaderWriter
  {
  /// <summary>
    /// Create a new design in the list based on the provided details
    /// </summary>
    /// <param name="ADesignID"></param>
    /// <param name="ADesignDescriptor"></param>
    /// <param name="AExtents"></param>
    /// <returns></returns>
    IDesign AddDesignDetails(Guid ADesignID,
      DesignDescriptor ADesignDescriptor,
      BoundingWorldExtent3D AExtents);

    /// <summary>
    /// Remove a given design from the list of designs for a site model
    /// </summary>
    /// <param name="ADesignID"></param>
    /// <returns></returns>
    bool RemoveDesign(Guid ADesignID);

    IDesign Locate(Guid AID);
  }
}
