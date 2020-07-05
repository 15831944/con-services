﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
#if RAPTOR
using ASNodeDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// The executor which passes the summary pass counts request to Raptor V2.
  /// This is the same as ExportReportExecutor V1 but with different error handling.
  /// </summary>
  public class CompactionExportExecutor : RequestExecutorContainer
  {
#if RAPTOR
    private static readonly Dictionary<ExportTypes, string> configKeys = new Dictionary<ExportTypes, string>
    {
      {ExportTypes.SurfaceExport, "ENABLE_TREX_GATEWAY_SURFACE"},
      {ExportTypes.VedaExport, "ENABLE_TREX_GATEWAY_VETA"},
      {ExportTypes.PassCountExport, "ENABLE_TREX_GATEWAY_EXPORT_PASSCOUNT"},
    };
#endif
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionExportExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the exports request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<ExportReport>(item);
#if RAPTOR
        if (UseTRexGateway(configKeys[request.ExportType]))
        {
#endif
          var overrides = AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings);
          var liftSettings = AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings);
          switch (request.ExportType)
          {
            case ExportTypes.SurfaceExport:
              var compactionSurfaceExportRequest =
                new CompactionSurfaceExportRequest(request.ProjectUid.Value, request.Filter, request.Filename, request.Tolerance,
                  overrides, liftSettings);

              log.LogInformation($"Calling TRex SendSurfaceExportRequest for projectUid: {request.ProjectUid}");
              return await trexCompactionDataProxy.SendDataPostRequest<CompactionExportResult, CompactionSurfaceExportRequest>(compactionSurfaceExportRequest, "/export/surface/ttm", customHeaders);

            case ExportTypes.VedaExport:             
            default://to satisfy the compiler
              var compactionVetaExportRequest =
                new CompactionVetaExportRequest(request.ProjectUid.Value, request.Filter, request.Filename, request.CoordType, 
                  request.OutputType, request.UserPrefs, request.MachineList.Select(m => m.MachineName).ToArray(), overrides, liftSettings);

              log.LogInformation($"Calling TRex SendVetaExportRequest for projectUid: {request.ProjectUid}");
              return await trexCompactionDataProxy.SendDataPostRequest<CompactionExportResult, CompactionVetaExportRequest>(compactionVetaExportRequest, "/export/veta", customHeaders);

            case ExportTypes.PassCountExport:
              var compactionPassCountExportRequest =
                new CompactionPassCountExportRequest(request.ProjectUid.Value, request.Filter, request.Filename, request.CoordType, 
                  request.OutputType, request.UserPrefs, request.RestrictSize, request.RawData, overrides, liftSettings);

              log.LogInformation($"Calling TRex SendPassCountExportRequest for projectUid: {request.ProjectUid}");
              return await trexCompactionDataProxy.SendDataPostRequest<CompactionExportResult, CompactionPassCountExportRequest>(compactionPassCountExportRequest, "/export/passcount", customHeaders);
          }
#if RAPTOR
        }
        log.LogInformation($"Calling Raptor ProcessWithRaptor for projectUid: {request.ProjectUid}");
        return ProcessWithRaptor(request);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(ExportReport request)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);

      bool success = raptorClient.GetProductionDataExport(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataExport),
        RaptorConverters.convertToRaptorUserPreferences(request.UserPrefs), (int)request.ExportType, request.CallerId, raptorFilter,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
        request.TimeStampRequired, request.CellSizeRequired, request.RawData, request.RestrictSize, true,
        request.Tolerance, request.IncludeSurveydSurface,
        request.Precheckonly, request.Filename, RaptorConverters.convertToRaptorMachines(request.MachineList), (int)request.CoordType,
        (int)request.OutputType,
        request.DateFromUTC, request.DateToUTC,
        RaptorConverters.convertToRaptorTranslations(request.Translations),
        RaptorConverters.convertToRaptorProjectExtents(request.ProjectExtents), out var dataexport);

      if (success)
      {
        try
        {
          return new CompactionExportResult(BuildFilePath(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.CallerId, request.Filename, true));
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received export data: " + ex.Message));
        }
      }

      throw CreateServiceException<CompactionExportExecutor>(dataexport.ReturnCode);
    }
#endif
    private string BuildFilePath(long projectid, string callerid, string filename, bool zipped)
    {
      string prodFolder = configStore.GetValueString("RaptorProductionDataFolder");
      return
        $"{prodFolder}\\DataModels\\{projectid}\\Exports\\{callerid}\\{Path.GetFileNameWithoutExtension(filename) + (zipped ? ".zip" : ".csv")}";
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddExportErrorMessages(ContractExecutionStates);
#endif
    }
  }
}
