﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApi.Models.Interfaces
{
  public interface IBoundingBoxService
  {
    Task<MapBoundingBox> GetBoundingBox(ProjectData project, FilterResult filter, TileOverlayType[] overlays,
      FilterResult baseFilter, FilterResult topFilter, DesignDescriptor designDescriptor,
      string userId, IHeaderDictionary customHeaders);

    Task<List<List<WGSPoint>>> GetFilterBoundaries(ProjectData project, FilterResult filter,
      FilterResult baseFilter, FilterResult topFilter, FilterBoundaryType boundaryType,
      IHeaderDictionary customHeaders);

    Task<List<List<WGSPoint>>> GetFilterBoundaries(ProjectData project, FilterResult filter,
      FilterBoundaryType boundaryType, IHeaderDictionary customHeaders);

    Task<IEnumerable<WGSPoint>> GetAlignmentPoints(ProjectData project, DesignDescriptor alignDescriptor,
      double startStation = 0, double endStation = 0, double leftOffset = 0, double rightOffset = 0, IHeaderDictionary customHeaders = null);

    Task<List<List<WGSPoint>>> GetDesignBoundaryPolygons(ProjectData project, DesignDescriptor designDescriptor,
      IHeaderDictionary customHeaders);

    Task<AlignmentStationRangeResult> GetAlignmentStationRange(ProjectData project, DesignDescriptor alignDescriptor, IHeaderDictionary customHeaders);

    Task<CoordinateConversionResult> GetProductionDataExtents(Guid projectUid, long projectId, IEnumerable<long> excludedIds, IEnumerable<Guid> excludedUids,
      string userId, IHeaderDictionary customHeaders);
  }
}
