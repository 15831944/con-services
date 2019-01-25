﻿using System;
using System.IO;
using System.Net;
#if RAPTOR
using ASNodeDecls;
using SVOICVolumeCalculationsDecls;
#endif
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// V2 Tile executor. Same as V1 but without the reconcileTopFilterAndVolumeComputationMode as this is done externally.
  /// </summary>
  public class CompactionTileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionTileExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the request for type of T.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<TileRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_TILES"))
        {
#endif
          var fileResult = trexCompactionDataProxy.SendProductionDataTileRequest(request, customHeaders).Result;

          using (var ms = new MemoryStream())
          {
            fileResult.CopyTo(ms);
            return new TileResult(ms.ToArray());
          }
#if RAPTOR
      }

        return ProcessWithRaptor(request);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(TileRequest request)
    {
      RaptorConverters.convertGridOrLLBoundingBox(request.BoundBoxGrid, request.BoundBoxLatLon, out var bottomLeftPoint, out var topRightPoint,
        out bool coordsAreGrid);

      var baseFilter = RaptorConverters.ConvertFilter(request.Filter1);
      var topFilter = RaptorConverters.ConvertFilter(request.Filter2);
      var designDescriptor = RaptorConverters.DesignDescriptor(request.DesignDescriptor);

      var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolumesType);
      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
      {
        RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
      }

      if ((baseFilter == null || topFilter == null) && designDescriptor.IsNull() ||
           baseFilter == null && topFilter == null)
      {
        throw new ServiceException(
          HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Invalid surface configuration."));
      }

      var raptorResult = raptorClient.GetRenderedMapTileWithRepresentColor(
        request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtWMSTile),
        RaptorConverters.convertDisplayMode(request.Mode),
        RaptorConverters.convertColorPalettes(request.Palettes, request.Mode),
        bottomLeftPoint, topRightPoint,
        coordsAreGrid,
        request.Width,
        request.Height,
        baseFilter,
        topFilter,
        RaptorConverters.convertOptions(null, request.LiftBuildSettings, request.ComputeVolNoChangeTolerance,
          request.FilterLayerMethod, request.Mode, request.SetSummaryDataLayersVisibility),
        designDescriptor,
        volType,
        request.RepresentationalDisplayColor,
        out MemoryStream tile);

      log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

      if (raptorResult == TASNodeErrorStatus.asneOK ||
          raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange)
      {
        if (tile != null)
          return ConvertResult(tile, raptorResult);
        else
          return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null tile returned");
      }

      log.LogTrace(
        $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}.");

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
        ContractExecutionStatesEnum.InternalProcessingError,
        $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}."));
    }

    private TileResult ConvertResult(MemoryStream tile, TASNodeErrorStatus raptorResult)
    {
      log.LogDebug("Raptor result for Tile: " + raptorResult);
      return new TileResult(tile.ToArray(), raptorResult != TASNodeErrorStatus.asneOK);
    }
#endif

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }
  }
}
