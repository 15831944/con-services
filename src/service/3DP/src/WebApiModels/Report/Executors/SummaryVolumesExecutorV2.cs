﻿using System;
#if RAPTOR
using ASNode.Volumes.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using VSS.Productivity3D.WebApi.Models.Report.Executors.Utilities;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Summary volumes executor for use with API v2.
  /// </summary>
  public class SummaryVolumesExecutorV2 : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutorV2()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<SummaryVolumesRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_VOLUMES"))
        {
#endif
          var summaryVolumesRequest = new SummaryVolumesDataRequest(
            request.ProjectUid,
            request.BaseFilter,
            request.TopFilter,
            request.BaseDesignDescriptor.FileUid,
            request.TopDesignDescriptor.FileUid,
            request.VolumeCalcType);

          return trexCompactionDataProxy.SendDataPostRequest<SummaryVolumesResult, SummaryVolumesDataRequest>(summaryVolumesRequest, "/volumes/summary", customHeaders).Result;
#if RAPTOR
        }

        TASNodeSimpleVolumesResult result;

        var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilter);
        var topFilter = RaptorConverters.ConvertFilter(request.TopFilter);
        var baseDesignDescriptor = RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor);
        var topDesignDescriptor = RaptorConverters.DesignDescriptor(request.TopDesignDescriptor);

        var volType = RaptorConverters.ConvertVolumesType(request.VolumeCalcType);

        // #68799 - Temporarily revert v2 executor behaviour to match that of v1 by adjusting filter dates on Filter to Filter calculations.
        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters && !request.ExplicitFilters)
        {
          RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
        }

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref baseFilter, ref topFilter,
          request.VolumeCalcType);
        // End #68799 fix.

        TASNodeErrorStatus raptorResult;

        if (request.CutTolerance != null && request.FillTolerance != null)
        {
          raptorResult = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilter), (double) request.CutTolerance,
            (double) request.FillTolerance,
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }
        else
        {
          raptorResult = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilter),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ResultConverter.SimpleVolumesResultToSummaryVolumesResult(result);

        throw CreateServiceException<SummaryVolumesExecutorV2>((int)raptorResult);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }
  }
}
