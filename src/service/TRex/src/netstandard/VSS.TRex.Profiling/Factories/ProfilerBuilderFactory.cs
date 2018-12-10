﻿using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Factories
{
  /// <summary>
  /// Factory responsible for creating builder elements use in construction of profile information based on cell production data
  /// </summary>
    public class ProfilerBuilderFactory<T> : IProfilerBuilderFactory<T> where T: class, IProfileCellBase, new()
  {
    /// <summary>
    /// Creates a new builder responsible for processing layer and other information for single cells in a profile
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="gridDataType"></param>
    /// <param name="populationControl"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellPassFastEventLookerUpper"></param>
    /// <returns></returns>
      public ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel, 
        GridDataType gridDataType, 
        IFilteredValuePopulationControl populationControl, 
        ICellPassAttributeFilter passFilter, 
        ICellPassFastEventLookerUpper cellPassFastEventLookerUpper)
      {
         return new CellLiftBuilder(siteModel, gridDataType, populationControl, passFilter, cellPassFastEventLookerUpper);
      }

    /// <summary>
    /// Creates a new builder responsible for determining a vector of cells that are cross by a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="slicerToolUsed"></param>
    /// <returns></returns>
    public ICellProfileBuilder<T> NewCellProfileBuilder(ISiteModel siteModel, 
        ICellSpatialFilter cellFilter, 
        IDesign cutFillDesign,
        bool slicerToolUsed)
      {
        return new CellProfileBuilder<T>(siteModel, cellFilter, cutFillDesign, slicerToolUsed);
      }

    /// <summary>
    /// Creates a new builder responsible for analyzing profile information for a cell of cells identified along a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    /// <returns></returns>
      public IProfileLiftBuilder<T> NewProfileLiftBuilder(ISiteModel siteModel,
        ISubGridTreeBitMask pDExistenceMap,
        ICellPassAttributeFilter passFilter,
        ICellSpatialFilter cellFilter,
        IDesign cellPassFilter_ElevationRangeDesign,
        ICellLiftBuilder cellLiftBuilder)
      {
        return new ProfileLiftBuilder<T>(siteModel, pDExistenceMap, passFilter, cellFilter, cellPassFilter_ElevationRangeDesign, cellLiftBuilder);
      }
  }
}
