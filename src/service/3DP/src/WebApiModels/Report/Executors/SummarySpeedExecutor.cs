﻿using System;
using System.Collections.Generic;
#if RAPTOR
using ASNode.SpeedSummary.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
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
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class SummarySpeedExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummarySpeedExecutor()
    {
      ProcessErrorCodes();
    }
#if RAPTOR
    private SpeedSummaryResult ConvertResult(TASNodeSpeedSummaryResult result)
    {
      return new SpeedSummaryResult
      (
        Math.Round(result.AboveTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
        Math.Round(result.BelowTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
        Math.Round(result.MatchTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
        Math.Round(result.CovegareArea, 5)
      );
    }
#endif
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<SummarySpeedRequest>(item);
#if RAPTOR
        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_SPEED"), out var useTrexGateway);

        if (useTrexGateway)
        {
#endif
          var speedSummaryRequest = new SpeedSummaryRequest(
            request.ProjectUid,
            request.Filter,
            request.LiftBuildSettings.MachineSpeedTarget);

          return trexCompactionDataProxy.SendDataPostRequest<SpeedSummaryResult, SpeedSummaryRequest>(speedSummaryRequest, "/speed/summary", customHeaders).Result;
#if RAPTOR
        }

        var raptorResult = raptorClient.GetSummarySpeed(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((request.CallId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          RaptorConverters.ConvertFilter(request.Filter, overrideAssetIds: new List<long>()),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
          out var result);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(result);

        throw CreateServiceException<SummarySpeedExecutor>((int)raptorResult);
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
