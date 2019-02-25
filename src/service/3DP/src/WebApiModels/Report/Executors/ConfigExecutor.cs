﻿using System;
using System.Net;
using System.Xml;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Log4NetExtensions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ConfigExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        raptorClient.RequestConfig(out var config);
        if (log.IsTraceEnabled())
          log.LogTrace("Received config {0}", config);
        var doc = new XmlDocument();
        doc.LoadXml(config);
        return ConfigResult.Create(config);
      }
      catch (Exception e)
      {
        log.LogError(e, "Exception loading config");
        throw new ServiceException(HttpStatusCode.InternalServerError,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message));
      }
    }
  }
}
