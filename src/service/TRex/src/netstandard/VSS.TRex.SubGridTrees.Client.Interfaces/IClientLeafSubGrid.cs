﻿using System;
using System.Collections;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
  /// <summary>
  /// Interface for ClientLeafSubGrid derivations based on ClientLeafSubGrid
  /// </summary>
  public interface IClientLeafSubGrid : ILeafSubGrid, ITRexMemoryCacheItem
  {
    double CellSize { set; get; }

    int IndexOriginOffset { get; set; }

    GridDataType GridDataType { get; }

    PopulationControlFlags EventPopulationFlags { get; set; }

    bool WantsLiftProcessingResults();

    void Assign(IClientLeafSubGrid source);

    bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue);

    void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context);

    bool TopLayerOnly { get; set; }

    /// <summary>
    /// Fills the contents of the client leaf sub grid with a known, non-null test pattern of values
    /// </summary>
    void FillWithTestPattern();

    /// <summary>
    /// Determines if the leaf content of this sub grid is equal to 'other'
    /// </summary>

    bool LeafContentEquals(IClientLeafSubGrid other);

    bool SupportsAssignationFrom(GridDataType gridDataType);

    void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source);

    void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source, SubGridTreeBitmapSubGridBits map);

    SubGridTreeBitmapSubGridBits ProdDataMap { get; }

    /// <summary>
    /// Existence map of cells matching current filter settings
    /// </summary>
    SubGridTreeBitmapSubGridBits FilterMap { get; }

    /// <summary>
    /// World extent of the client leaf sub grid map of cells
    /// </summary>
    BoundingWorldExtent3D WorldExtents();

    void DumpToLog();

    bool UpdateProcessingMapForSurveyedSurfaces(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass);

    bool PerformHeightAnnotation(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass,
      IClientLeafSubGrid surfaceElevationsSource, Func<int, int, float, bool> elevationRangeFilter);
  }
}
