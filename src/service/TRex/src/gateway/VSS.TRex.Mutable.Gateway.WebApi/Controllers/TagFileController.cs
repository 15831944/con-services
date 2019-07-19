﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Mutable.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Process TAG files Controller
  /// </summary>
  public class TagFileController : BaseController
  {
    /// <summary>
    /// Controller for tag file processing
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    public TagFileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore)
        : base(loggerFactory, loggerFactory.CreateLogger<TagFileController>(), exceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Test endpoint for tag files
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpGet]
    public String Get()
    {
      // Test endpoint connection
      return "You have reached api/v2/tagfiles";
    }

    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagNonDirectFile([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = ConvertObjectForLogging.SerializeObjectIgnoringProperties(request, "Data");
      Log.LogInformation("PostTagFile: " + serializedRequest);
      return await ExecuteRequest(request);
    }

    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
   // [PostRequestVerifier]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public Task<ContractExecutionResult> PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {

      var serializedRequest = ConvertObjectForLogging.SerializeObjectIgnoringProperties(request, "Data");
      Log.LogInformation("PostTagFile (Direct): " + serializedRequest);
      return ExecuteRequest(request);
    }

    private Task<ContractExecutionResult> ExecuteRequest(CompactionTagFileRequest tfRequest)
    {

      return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
                                                     .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
                                                     .ProcessAsync(tfRequest));
    }
  }
}
