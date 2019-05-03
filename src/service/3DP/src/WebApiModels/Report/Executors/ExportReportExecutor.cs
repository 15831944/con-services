﻿using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary pass counts request to Raptor
  /// </summary>
  public class ExportReportExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ExportReportExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary pass counts request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ExportReport>(item);
      var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);

      bool success = raptorClient.GetProductionDataExport(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataExport),
        RaptorConverters.convertToRaptorUserPreferences(request.UserPrefs), (int)request.ExportType, request.CallerId, raptorFilter,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
        request.TimeStampRequired, request.CellSizeRequired, request.RawData, request.RestrictSize, true,
        request.Tolerance, request.IncludeSurveydSurface,
        request.Precheckonly, request.Filename, RaptorConverters.convertToRaptorMachines(request.MachineList), (int)request.CoordType, (int)request.OutputType,
        request.DateFromUTC, request.DateToUTC,
        RaptorConverters.convertToRaptorTranslations(request.Translations), 
        RaptorConverters.convertToRaptorProjectExtents(request.ProjectExtents), out var dataexport);

      if (success)
      {
        try
        {
          return ExportResult.Create(
            File.ReadAllBytes(BuildFilePath(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.CallerId, request.Filename, true)),
            dataexport.ReturnCode);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received export data: " + ex.Message));
        }
      }

      throw CreateServiceException<ExportReportExecutor>(dataexport.ReturnCode);
    }
    
    private string BuildFilePath(long projectid, string callerid, string filename, bool zipped)
    {
      string prodFolder = configStore.GetValueString("RaptorProductionDataFolder");
      return
        $"{prodFolder}\\DataModels\\{projectid}\\Exports\\{callerid}\\{Path.GetFileNameWithoutExtension(filename) + (zipped ? ".zip" : ".csv")}";
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddExportErrorMessages(ContractExecutionStates);
    }
  }
}
