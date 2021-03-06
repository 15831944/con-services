﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class SiteModelStatisticsExecutor : BaseExecutor
  {
    public SiteModelStatisticsExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SiteModelStatisticsExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectStatisticsTRexRequest>(item);
      var siteModel = GetSiteModel(request.ProjectUid);

      var result = new ProjectStatisticsResult();

      if (siteModel != null)
      {
        var extents = siteModel.GetAdjustedDataModelSpatialExtents(request.ExcludedSurveyedSurfaceUids);
        result.extents = new BoundingBox3DGrid(
          extents.MinX, extents.MinY, extents.MinZ,
          extents.MaxX, extents.MaxY, extents.MaxZ
        );

        var startEndDates = siteModel.GetDateRange();
        var format = "yyyy-MM-ddTHH-mm-ss.fffffff";
        result.startTime = DateTime.ParseExact(startEndDates.startUtc.ToString(format, CultureInfo.InvariantCulture), format, CultureInfo.InvariantCulture);
        result.endTime = DateTime.ParseExact(startEndDates.endUtc.ToString(format, CultureInfo.InvariantCulture), format, CultureInfo.InvariantCulture);

        result.cellSize = siteModel.Grid.CellSize;
        result.indexOriginOffset = siteModel.Grid.IndexOriginOffset;
      }

      return result;
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
