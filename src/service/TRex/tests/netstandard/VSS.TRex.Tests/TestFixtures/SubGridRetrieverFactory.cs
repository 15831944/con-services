﻿using System;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  /// <summary>
  /// Factory class that constructs specialist sub grid retrievers according to the grid data type being requested
  /// </summary>
  public class SubGridRetrieverFactory : ISubGridRetrieverFactory
  {
    public ISubGridRetriever Instance(ISubGridsRequestArgument subGridsRequestArgument,
      ISiteModel siteModel,
      GridDataType gridDataType,
      IStorageProxy storageProxy,
      ICombinedFilter filter,
      ICellPassAttributeFilterProcessingAnnex filterAnnex,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      int maxNumberOfPassesToReturn,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask pdExistenceMap,
      ITRexSpatialMemoryCacheContext[] subGridCacheContexts,
      IOverrideParameters overrides,
      ILiftParameters liftParams,
      ISurveyedSurfaces filteredSurveyedSurfaces)
    {
      if (gridDataType == GridDataType.ProgressiveVolumes)
      {
        var retriever = new ProgressiveVolumesSubGridRetriever(siteModel,
          gridDataType,
          storageProxy,
          filter,
          filterAnnex,
          hasOverrideSpatialCellRestriction,
          overrideSpatialCellRestriction,
          subGridCacheContexts != null,
          maxNumberOfPassesToReturn,
          areaControlSet,
          populationControl,
          pdExistenceMap,
          overrides,
          liftParams,
          filteredSurveyedSurfaces);

        if (subGridsRequestArgument is IProgressiveVolumesSubGridsRequestArgument argument)
        {
          retriever.StartDate = argument.StartDate;
          retriever.EndDate = argument.EndDate;
          retriever.Interval = argument.Interval;
        }
        else
        {
          throw new ArgumentException($"Argument passed to sub grid retriever factory for progressive volumes retriever construction is not an expected type: {subGridsRequestArgument.GetType()}");
        }

        return retriever;
      }
      else
      {
        var retriever = new SubGridRetriever(siteModel,
          gridDataType,
          storageProxy,
          filter,
          filterAnnex,
          hasOverrideSpatialCellRestriction,
          overrideSpatialCellRestriction,
          subGridCacheContexts != null,
          maxNumberOfPassesToReturn,
          areaControlSet,
          populationControl,
          pdExistenceMap,
          overrides,
          liftParams);

        return retriever;
      }
    }
  }
}
