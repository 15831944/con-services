﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Filters
{
  public static class FilterUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(FilterUtilities));

    /*
    private IExistenceMaps existenceMaps = null;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ?? (existenceMaps = DIContext.Obtain<IExistenceMaps>());
    */

    /// <summary>
    /// Prepare a filter for use by performing any necessary coordinate conversions and requesting any
    /// supplemental information such as alignment design boundary calculations.
    /// </summary>
    /// <param name="Filter"></param>
    /// <param name="DataModelID"></param>
    /// <returns></returns>
    public static RequestErrorStatus PrepareFilterForUse(ICombinedFilter Filter, Guid DataModelID)
    {
      // Fence DesignBoundary = null;
      RequestErrorStatus Result = RequestErrorStatus.OK;

      //RequestResult: TDesignProfilerRequestResult;

      if (Filter == null)
        return Result;

      if (Filter.SpatialFilter != null)
      {
        if (!Filter.SpatialFilter.CoordsAreGrid)
        {
          ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);

          XYZ[] LLHCoords;
          // If the filter has a spatial or positional context, then convert the LLH values in the
          // spatial context into the NEE values consistent with the data model.
          if (Filter.SpatialFilter.IsSpatial)
          {
            LLHCoords = new XYZ[Filter.SpatialFilter.Fence.NumVertices];

            // Note: Lat/Lons in filter fence boundaries are supplied to us in decimal degrees, not radians
            for (int FencePointIdx = 0; FencePointIdx < Filter.SpatialFilter.Fence.NumVertices; FencePointIdx++)
            {
              LLHCoords[FencePointIdx] = new XYZ(MathUtilities.DegreesToRadians(Filter.SpatialFilter.Fence[FencePointIdx].X), MathUtilities.DegreesToRadians(Filter.SpatialFilter.Fence[FencePointIdx].Y));
            }

            (var errorCode, XYZ[] NEECoords) = ConvertCoordinates.LLHToNEE(SiteModel.CSIB(), LLHCoords);

            if (errorCode != RequestErrorStatus.OK)
            {
              Log.LogInformation("Summary volume failure, could not convert coordinates from WGS to grid coordinates");

              return RequestErrorStatus.FailedToConvertClientWGSCoords;
            }

            for (int fencePointIdx = 0; fencePointIdx < Filter.SpatialFilter.Fence.NumVertices; fencePointIdx++)
            {
              Filter.SpatialFilter.Fence[fencePointIdx].X = NEECoords[fencePointIdx].X;
              Filter.SpatialFilter.Fence[fencePointIdx].Y = NEECoords[fencePointIdx].Y;
            }

            // Ensure that the bounding rectangle for the filter fence correctly encloses the newly calculated grid coordinates
            Filter.SpatialFilter.Fence.UpdateExtents();
          }

          if (Filter.SpatialFilter.IsPositional)
          {
            // Note: Lat/Lons in positions are supplied to us in decimal degrees, not radians
            LLHCoords = new[] { new XYZ(MathUtilities.DegreesToRadians(Filter.SpatialFilter.PositionX), MathUtilities.DegreesToRadians(Filter.SpatialFilter.PositionY)) };

            (var errorCode, XYZ[] NEECoords) = ConvertCoordinates.LLHToNEE(SiteModel.CSIB(), LLHCoords);

            if (errorCode != RequestErrorStatus.OK)
            {
              Log.LogInformation("Filter mutation failure, could not convert coordinates from WGS to grid coordinates");

              return RequestErrorStatus.FailedToConvertClientWGSCoords;
            }
            
            Filter.SpatialFilter.PositionX = NEECoords[0].X;
            Filter.SpatialFilter.PositionY = NEECoords[0].Y;
          }

          Filter.SpatialFilter.CoordsAreGrid = true;
        }

        // Ensure that the bounding rectangle for the filter fence correctly encloses the newly calculated grid coordinates
        Filter.SpatialFilter?.Fence.UpdateExtents();

        // Is there an alignment file to look up? If so, only do it if there not an already existing alignment fence boundary
        if (Filter.SpatialFilter.HasAlignmentDesignMask() && !(Filter.SpatialFilter.AlignmentFence?.HasVertices ?? true))
        {
          var BoundaryResult = AlignmentDesignFilterBoundaryRequest.Execute
            (new DesignOffset(Filter.SpatialFilter.AlignmentDesignMaskDesignUID, 0),
             Filter.SpatialFilter.StartStation ?? Common.Consts.NullDouble,
             Filter.SpatialFilter.EndStation ?? Common.Consts.NullDouble,
             Filter.SpatialFilter.LeftOffset ?? Common.Consts.NullDouble,
             Filter.SpatialFilter.RightOffset ?? Common.Consts.NullDouble);

          if (BoundaryResult.RequestResult != DesignProfilerRequestResult.OK)
          {
            Log.LogError($"{nameof(PrepareFilterForUse)}: Failed to get boundary for alignment design ID:{Filter.SpatialFilter.AlignmentDesignMaskDesignUID}");

            return RequestErrorStatus.NoResultReturned;
          }

          Filter.SpatialFilter.AlignmentFence = BoundaryResult.Boundary;
        }

        // Is there a surface design to look up
        if (Filter.SpatialFilter.HasSurfaceDesignMask())
        {
          // Todo: Not yet supported (or demonstrated that it's needed as this should be taken care of in the larger scale determination of the subgrids that need to be queried).

          /* If the filter needs to retain a reference to the existence map, then do this...
          Filter.SpatialFilter.DesignMaskExistenceMap = GetExistenceMaps().GetSingleExistenceMap(DataModelID, EXISTENCE_MAP_DESIGN_DESCRIPTOR, Filter.SpatialFilter.SurfaceDesignMaskDesignUid);

          if (Filter.SpatialFilter.DesignMaskExistenceMap == null)
          {
            Log.LogError($"PrepareFilterForUse: Failed to get existence map for surface design ID:{Filter.SpatialFilter.SurfaceDesignMaskDesignUid}");
          }
          */
        }
      }

      return Result;
    }

    /// <summary>
    /// Prepare a set of filter for use by performing any necessary coordinate conversions and requesting any
    /// supplemental information such as alignment design boundary calculations.
    /// </summary>
    /// <param name="Filters"></param>
    /// <param name="DataModelID"></param>
    /// <returns></returns>
    public static RequestErrorStatus PrepareFiltersForUse(ICombinedFilter[] Filters, Guid DataModelID)
    {
      var status = RequestErrorStatus.Unknown;

      foreach (ICombinedFilter filter in Filters)
      {
        if (filter != null)
        {
          status = PrepareFilterForUse(filter, DataModelID);

          if (status != RequestErrorStatus.OK)
            break;
        }
      }

      return status;
    }
  }
}
