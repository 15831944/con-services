﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.Jobs.ExportJob
{
  /// <summary>
  /// Class for managing an export job.
  /// </summary>
  public class ExportJob : IExportJob, IJob
  {
    public static Guid VSSJOB_UID = Guid.Parse("c3cbb048-05c1-4961-a799-70434cb2f162");
    public Guid VSSJobUid => VSSJOB_UID;

    /// <summary>
    /// Used to store the final download link for export jobs
    /// </summary>
    public const string DOWNLOAD_LINK_STATE_KEY = "downloadLink";

    /// <summary>
    /// Used to store the s3 key for the export jobs
    /// </summary>
    public const string S3_KEY_STATE_KEY = "s3Key";

    /// <summary>
    /// Location to save incoming Scheduled Job Requests
    /// </summary>
    private const string S3_SCHEDULE_SAVE_LOCATION = "background";

    private readonly IApiClient _apiClient;
    private readonly ITransferProxy _transferProxy;
    private readonly ITransferProxyFactory _transferProxyFactory;
    private readonly ILogger _log;
    private Guid _savedRequestId;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public ExportJob(IApiClient apiClient, ITransferProxyFactory transferProxyfactory, ILoggerFactory logger)
    {
      _log = logger.CreateLogger<ExportJob>();
      _apiClient = apiClient;
      _transferProxyFactory = transferProxyfactory;
      _transferProxy = _transferProxyFactory.NewProxy(TransferProxyType.Temporary);
    }

    /// <summary>
    /// Save the request in S3 for use in the background task, rather than in the Database
    /// </summary>
    /// <param name="request">Request to be saved</param>
    /// <returns>A Guid to be passed in to the background task</returns>
    private Guid SaveRequest(ScheduleJobRequest request)
    {
      var guid = Guid.NewGuid();
      var data = JsonConvert.SerializeObject(request, Formatting.None);
      var bytes = Encoding.UTF8.GetBytes(data);
      using (var ms = new MemoryStream(bytes))
      {
        _transferProxy.Upload(ms, $"{S3_SCHEDULE_SAVE_LOCATION}/{guid}");
      }

      return guid;
    }

    /// <summary>
    /// Fetch the Schedule Job Request for a given Request ID
    /// </summary>
    /// <param name="requestId">Request ID returned from the SaveRequest Method</param>
    /// <returns>The original Scheduled Task class</returns>
    private async Task<ScheduleJobRequest> DownloadRequest(Guid requestId)
    {
      ScheduleJobRequest request = null;
      var fileStreamResult = await _transferProxy.Download($"{S3_SCHEDULE_SAVE_LOCATION}/{requestId}");
      using (var ms = new MemoryStream())
      {
        fileStreamResult.FileStream.CopyTo(ms);
        var bytes = ms.ToArray();
        var data = Encoding.UTF8.GetString(bytes);
        request = JsonConvert.DeserializeObject<ScheduleJobRequest>(data);
      }

      return request;
    }

    /// <summary>
    /// Queue a Scheduled Job to be run in the background
    /// </summary>
    /// <param name="request">Scheduled Job Details</param>
    /// <param name="customHeaders">Any Customer headers to be passed with the Scheduled Job Request</param>
    /// <returns>A Job ID for the Background Job</returns>
    public string QueueJob(ScheduleJobRequest request, IHeaderDictionary customHeaders)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the export data from a web api and saves it to AWS S3.
    /// </summary>
    /// <param name="requestId">Http request details of how to get the export data</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="context">Hangfire context</param>
    [ExportFailureFilter]
    [AutomaticRetry(Attempts = 0)]
    public async Task GetExportData(Guid requestId, IHeaderDictionary customHeaders,
      PerformContext context)
    {
      // Refetch the Request Model from S3
      var request = await DownloadRequest(requestId);
      await ExecuteExportProc(request, customHeaders, context);
    }

    public async Task<string> ExecuteExportProc(ScheduleJobRequest request, IHeaderDictionary customHeaders,
      PerformContext context)
    {
      var result = await _apiClient.SendRequest<CompactionExportResult>(request, customHeaders);
      try
      {
        // Set the results so the results can access the final url easily
        if (context != null)
        {
          _log.LogInformation($"Setting export job {context.BackgroundJob.Id} downloadLink={result.DownloadLink}");
          JobStorage.Current.GetConnection().SetJobParameter(context.BackgroundJob.Id, S3_KEY_STATE_KEY, GetS3Key(context.BackgroundJob.Id, request.Filename));
          JobStorage.Current.GetConnection().SetJobParameter(context.BackgroundJob.Id, DOWNLOAD_LINK_STATE_KEY, result.DownloadLink);
        }
        return result.DownloadLink;
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Exception in ApiClient delegate.");
        throw;
      }
    }

    /// <summary>
    /// Gets the download link for the completed job.
    /// </summary>
    [Obsolete("Use the JobStorage to store download links, as the requested filename could change")]
    public string GetDownloadLink(string jobId, string filename) => _transferProxy.GeneratePreSignedUrl(GetS3Key(jobId, filename));

    /// <summary>
    /// Gets the S3 key for a job
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <param name="filename">The name of the file</param>
    /// <returns>The S3 key where the file is stored. This is the full path and file name in AWS.</returns>
    public static string GetS3Key(string jobId, string filename) => $"3dpm/{jobId}/{filename}";

    public Task Setup(object o, object context)
    {
      var request = o.GetConvertedObject<ScheduleJobRequest>();
      _savedRequestId = SaveRequest(request);

      return Task.FromResult(true);
    }

    public Task Run(object o, object context)
    {
      if (!(context is PerformContext))
        throw new ArgumentException($"Wrong context object has been passed {nameof(context)}");

      var headers = o.GetConvertedObject<HeaderDictionary>();

      return GetExportData(_savedRequestId, headers, context as PerformContext);
    }

    public Task TearDown(object o, object context) => Task.FromResult(true);
  }
}
