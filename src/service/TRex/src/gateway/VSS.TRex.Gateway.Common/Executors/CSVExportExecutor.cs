﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class CSVExportExecutor : BaseExecutor
  {
    public CSVExportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CSVExportExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionCSVExportRequest>(item);

      var siteModel = GetSiteModel(request.ProjectUid);
      if (request.CoordType == CoordType.LatLon && siteModel.CSIBLoaded == false)
      {
        log.LogError($"#Out# CSVExportExecutor. CoordinateType of LatLong requested, but CSIB not found : Project: {request.ProjectUid} Filename: {request.FileName}");
        throw CreateServiceException<CSVExportExecutor>
        (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
          RequestErrorStatus.ExportInvalidCSIB);
      }

      var filter = ConvertFilter(request.Filter, siteModel);
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, request.Filter);
      filter.AttributeFilter.StartTime = startEndDate.Item1;
      filter.AttributeFilter.EndTime = startEndDate.Item2;

      var tRexRequest = new CSVExportRequest();
      var csvExportRequestArgument = AutoMapperUtility.Automapper.Map<CSVExportRequestArgument>(request);
      csvExportRequestArgument.MappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, request.MachineNames);
      csvExportRequestArgument.Filters = new FilterSet(filter);
      var response = await tRexRequest.ExecuteAsync(csvExportRequestArgument);

      if (response == null || response.ResultStatus != RequestErrorStatus.OK)
      {
        log.LogError($"CSVExportExecutor unable to process request. Project: {request.ProjectUid} Filename: {request.FileName} Response: {response?.ResultStatus.ToString()}");
        throw CreateServiceException<CSVExportExecutor>
        (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
          response?.ResultStatus ?? RequestErrorStatus.FailedToConfigureInternalPipeline);
      }
      var s3FileTransfer = new S3FileTransfer(TransferProxyType.Temporary);
      return new CompactionExportResult(s3FileTransfer.GeneratePreSignedUrl(response.fileName));
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
