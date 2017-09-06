﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Notification controller for tag file errors and misc alarms
  /// </summary>
  public class NotificationController : Controller
  {
    private readonly IRepositoryFactory factory;
    private readonly ILogger log;

    /// <summary>
    /// Notification constructor
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="logger"></param>
    public NotificationController(IRepositoryFactory factory, ILogger<NotificationController> logger)
    {
      this.factory = factory;
      this.log = logger;
    }

    /// <summary>
    /// Writes to the log for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the asset id, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorV1Executor</executor>
    [Route("api/v1/notification/tagFileProcessingError")]
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV1Request request)
    {
      log.LogDebug("PostTagFileProcessingErrorV1: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      return RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, log)
        .Process(request) as TagFileProcessingErrorResult;
    }

    /// <summary>
    /// Writes a Kafka event for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the customerUid, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorV2Executor</executor>
    [Route("api/v2/notification/tagFileProcessingError")]
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    {
      log.LogDebug("PostTagFileProcessingErrorV2: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      return RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(factory, log)
        .Process(request) as TagFileProcessingErrorResult;
    }

    /// <summary>
    /// Posts the application alarm.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [Route("api/v1/notification/appAlarm")]
    [HttpPost]
    public ContractExecutionResult PostAppAlarm([FromBody] AppAlarmMessage request)
    {
      log.LogWarning("PostAppAlarm: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();
      return new ContractExecutionResult();
    }
  }
}