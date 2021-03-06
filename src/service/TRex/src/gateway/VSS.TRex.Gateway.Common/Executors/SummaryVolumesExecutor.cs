﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Geometry;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Summary Volumes statistics.
  /// </summary>
  public class SummaryVolumesExecutor : BaseExecutor
  {
    public SummaryVolumesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as SummaryVolumesDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<SummaryVolumesDataRequest>();

      // ReSharper disable once PossibleNullReferenceException
      var siteModel = GetSiteModel(request.ProjectUid);

      var baseFilter = ConvertFilter(request.BaseFilter, siteModel);
      var topFilter = ConvertFilter(request.TopFilter, siteModel);
      var additionalSpatialFilter = ConvertFilter(request.AdditionalSpatialFilter, siteModel);

      var summaryVolumesRequest = new SimpleVolumesRequest_ApplicationService();

      var simpleVolumesResponse = await summaryVolumesRequest.ExecuteAsync(new SimpleVolumesRequestArgument
      {
        ProjectID = siteModel.ID,
        BaseFilter = baseFilter,
        TopFilter = topFilter,
        AdditionalSpatialFilter = additionalSpatialFilter,
        BaseDesign = new DesignOffset(request.BaseDesignUid ?? Guid.Empty, request.BaseDesignOffset ?? 0),
        TopDesign = new DesignOffset(request.TopDesignUid ?? Guid.Empty, request.TopDesignOffset ?? 0),
        VolumeType = ConvertVolumesHelper.ConvertVolumesType(request.VolumeCalcType),
        CutTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE,
        FillTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE
      });

      if (simpleVolumesResponse != null)
      {
        log.LogInformation($"Volume response is {JsonConvert.SerializeObject(simpleVolumesResponse)}");
        return ConvertResult(simpleVolumesResponse);
      }

      log.LogWarning("Volume response is null");
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Summary Volumes data"));
    }

    /// <summary>
    /// Converts SimpleVolumesResponse data into SummaryVolumesResult data.
    /// </summary>
    private SummaryVolumesResult ConvertResult(SimpleVolumesResponse result)
    {
      return SummaryVolumesResult.Create(
        BoundingBox3DGridHelper.ConvertExtents(result.BoundingExtentGrid),
        result.Cut ?? 0.0,
        result.Fill ?? 0.0,
        result.TotalCoverageArea ?? 0.0,
        result.CutArea ?? 0.0,
        result.FillArea ?? 0.0);
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
