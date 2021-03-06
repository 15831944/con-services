﻿using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary CCA request to Raptor
  /// </summary>
  public class SummaryCCAExecutor : TbcExecutorHelper
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCCAExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary CCA request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CCARequest>(item);
#if RAPTOR
        if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_CCA") ?? false)
        {
#endif
        await PairUpAssetIdentifiers(request.ProjectUid.Value, request.Filter);
        await PairUpImportedFileIdentifiers(request.ProjectUid.Value, filter1: request.Filter);

        var ccaSummaryRequest = new CCASummaryRequest(
            request.ProjectUid.Value, 
            request.Filter,
            AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
            AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings));

          return await trexCompactionDataProxy.SendDataPostRequest<CCASummaryResult, CCASummaryRequest>(ccaSummaryRequest, "/cca/summary", customHeaders);
#if RAPTOR
        }

        var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);

        bool success = raptorClient.GetCCASummary(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtCCASummary),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
                            out var ccaSummary);

        if (success)
          return ConvertResult(ccaSummary);

        throw CreateServiceException<SummaryCCAExecutor>(ccaSummary.ReturnCode);
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
      RaptorResult.AddMissingTargetDataResultMessages(ContractExecutionStates);
#endif
    }
#if RAPTOR
    private CCASummaryResult ConvertResult(TCCASummary summary)
    {
      return CCASummaryResult.Create(
                summary.CompactedPercent,
                summary.OverCompactedPercent,
                summary.ReturnCode,
                summary.TotalAreaCoveredSqMeters,
                summary.UnderCompactedPercent);
    }
#endif

  }
}
