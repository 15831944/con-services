﻿using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurfaceElevationPatchArgument
  {
    /// <summary>
    /// The ID of the SiteModel to execute the request against
    /// </summary>
    Guid SiteModelID { get; }

    /// <summary>
    /// The bottom left on-the-ground cell origin X location for the patch of elevations to be computed from
    /// </summary>
    int OTGCellBottomLeftX { get; }

    /// <summary>
    /// The bottom left on-the-ground cell origin Y location for the patch of elevations to be computed from
    /// </summary>
    int OTGCellBottomLeftY { get; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    double CellSize { get; }

    /// <summary>
    /// Determines which surface information should be extracted: Earliest, Latest or Composite
    /// </summary>
    SurveyedSurfacePatchType SurveyedSurfacePatchType { get; }

    /// <summary>
    /// A map of the cells within the sub grid patch to be computed
    /// </summary>
    SubGridTreeBitmapSubGridBits ProcessingMap { get; set; }

    /// <summary>
    /// The list of surveyed surface UIDs to be included in the calculation
    /// Note: This list should be ordered in the preferred time order for the calculation.
    /// IE: If latest single elevations are needed then this list should be sorted in decreasing time,
    /// similarly if earliest single elevations are needed then this list should be sorted in increasing time.
    /// </summary>
    Guid[] IncludedSurveyedSurfaces { get; set; }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    string ToString();

    /// <summary>
    /// Computes a Fingerprint for use in caching surveyed surface height + time responses
    /// Note: This fingerprint used the SurveyedSurfaceHeightAndTime grid data type in the cache fingerprint,
    /// even though the core engine returns HeightAndTime results. This allows HeightAndTime and
    /// SurveyedSurfaceHeightAndTime results to cohabit in the same cache
    /// </summary>
    string CacheFingerprint();

    void SetOTGBottomLeftLocation(int otgCellBottomLeftX, int otgCellBottomLeftY);
  }
}
