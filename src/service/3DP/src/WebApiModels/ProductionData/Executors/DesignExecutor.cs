﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from Raptor's Project/DataModel.
  /// </summary>
  /// 
  public class DesignExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        log.LogDebug("Getting GeoJson design boundaries from Raptor");

        var request = CastRequestObjectTo<DesignBoundariesRequest>(item);
        JObject[] geoJsons;

        if (fileList != null && fileList.Count > 0)
        {
          List<JObject> geoJsonList = new List<JObject>();

          for (var i = 0; i < fileList.Count; i++)
          {
            log.LogDebug($"Getting GeoJson design boundary from Raptor for file: {fileList[i].Name}");

            string fileSpaceId = FileDescriptorExtensions.GetFileSpaceId(configStore, log);
            FileDescriptor fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, fileList[i].Path, fileList[i].Name);

            bool result = raptorClient.GetDesignBoundary(
              DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
                request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
                fileDescriptor.DesignDescriptor(configStore, log, 0, 0),
                DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson,
                request.tolerance,
                TVLPDDistanceUnits.vduMeters,
                0),
              out var memoryStream,
              out var designProfilerResult);

            if (result)
            {
              if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null && memoryStream.Length > 0)
              {
                // TODO (Aaron) Re do this.
                memoryStream.Position = 0;
                var sr = new StreamReader(memoryStream);
                string geoJSONStr = sr.ReadToEnd();
                JObject jo = JObject.Parse(geoJSONStr);
                geoJsonList.Add(jo);
              }
            }
            else
            {
              throw new ServiceException(HttpStatusCode.InternalServerError,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  $"Failed to get design boundary for file: {fileList[i].Name}"));
            }
          }

          geoJsons = geoJsonList.ToArray();
        }
        else
        {
          geoJsons = new JObject[0];
        }

        return DesignResult.CreateDesignResult(geoJsons);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
