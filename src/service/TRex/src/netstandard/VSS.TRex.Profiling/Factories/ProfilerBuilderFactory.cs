﻿using System;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Factories
{
  /// <summary>
  /// Factory responsible for creating builder elements used in construction of profile information based on cell production data
  /// </summary>
  public class ProfilerBuilderFactory<T> : IProfilerBuilderFactory<T> where T : class, IProfileCellBase, new()
  {
    /// <summary>
    /// Creates a new builder responsible for determining a vector of cells that are crossed by a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filterSet"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="cutFillDesignOffset"></param>
    /// <param name="slicerToolUsed"></param>
    /// <returns></returns>
    public ICellProfileBuilder<T> NewCellProfileBuilder(ISiteModel siteModel,
      IFilterSet filterSet,
      IDesign cutFillDesign,
      double cutFillDesignOffset,
      bool slicerToolUsed)
    {
      return new CellProfileBuilder<T>(siteModel, filterSet, cutFillDesign, cutFillDesignOffset, slicerToolUsed);
    }

    /// <summary>
    /// Creates a new builder responsible for analyzing profile information for a cell or cells identified along a profile line
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellPassFilter_ElevationRangeDesignOffset"></param>
    /// <param name="referenceDesign"></param>
    /// <param name="referenceDesignOffset"></param>
    /// <param name="cellLiftBuilder"></param>
    /// <param name="profileStyle"></param>
    /// <returns></returns>
    public ICellProfileAnalyzer<T> NewCellProfileAnalyzer(ProfileStyle profileStyle,
      ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesign cellPassFilter_ElevationRangeDesign,
      double cellPassFilter_ElevationRangeDesignOffset,
      IDesign referenceDesign,
      double referenceDesignOffset,
      ICellLiftBuilder cellLiftBuilder)
    {
      switch (profileStyle)
      {
        case ProfileStyle.CellPasses:
          return DIContext.Obtain<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, double, ICellLiftBuilder, ICellProfileAnalyzer<T>>>()
            (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellPassFilter_ElevationRangeDesignOffset, cellLiftBuilder);

        case ProfileStyle.SummaryVolume:
          return DIContext.Obtain<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, double, IDesign, double, ICellLiftBuilder, ICellProfileAnalyzer<T>>>()
            (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellPassFilter_ElevationRangeDesignOffset, referenceDesign, referenceDesignOffset, cellLiftBuilder);

        default:
          throw new ArgumentOutOfRangeException(nameof(profileStyle), profileStyle, null);
      }
    }

    /// <summary>
    /// Creates a new builder responsible for processing cell pass, layer and other information for single cells in a profile
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="gridDataType"></param>
    /// <param name="populationControl"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFastEventLookerUpper"></param>
    /// <returns></returns>
    public ICellLiftBuilder NewCellLiftBuilder(ISiteModel siteModel,
      GridDataType gridDataType,
      IFilteredValuePopulationControl populationControl,
      IFilterSet filterSet,
      ICellPassFastEventLookerUpper cellPassFastEventLookerUpper)
    {
      return new CellLiftBuilder(siteModel, gridDataType, populationControl, filterSet, cellPassFastEventLookerUpper);
    }
  }
}
