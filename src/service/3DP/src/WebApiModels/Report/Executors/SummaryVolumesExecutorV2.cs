﻿using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNode.Volumes.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using VSS.Productivity3D.WebApi.Models.Report.Executors.Utilities;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
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
            request.BaseDesignDescriptor.Offset,
            request.TopDesignDescriptor.FileUid,
            request.TopDesignDescriptor.Offset,
            request.VolumeCalcType);

          return await trexCompactionDataProxy.SendDataPostRequest<SummaryVolumesResult, SummaryVolumesDataRequest>(summaryVolumesRequest, "/volumes/summary", customHeaders);
#if RAPTOR
        }

        TASNodeSimpleVolumesResult result;

        var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilter, request.ProjectId, raptorClient);
        var topFilter = RaptorConverters.ConvertFilter(request.TopFilter, request.ProjectId, raptorClient);
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
          raptorResult = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilter, request.ProjectId, raptorClient), (double)request.CutTolerance,
            (double)request.FillTolerance,
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }
        else
        {
          raptorResult = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilter, request.ProjectId, raptorClient),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }

        switch (raptorResult)
        {
          case TASNodeErrorStatus.asneOK:
            return ResultConverter.SimpleVolumesResultToSummaryVolumesResult(result);
          case TASNodeErrorStatus.asneNoProductionDataFound:
            return null;
          default:
            throw CreateServiceException<SummaryVolumesExecutorV2>((int)raptorResult);
        }
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
