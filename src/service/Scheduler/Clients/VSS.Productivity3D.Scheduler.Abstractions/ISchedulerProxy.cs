﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface ISchedulerProxy : ICacheProxy
  {
    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    Task<ScheduleJobResult> ScheduleExportJob(ScheduleJobRequest request, IHeaderDictionary customHeaders);

    /// <summary>
    /// Schedule a background job via Scheduler, behind TPAAS
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="customHeaders">Any custom headers to be passed to the Scheduler</param>
    /// <returns>A Scheduled Job Result, with a Job ID to check status</returns>
    Task<ScheduleJobResult> ScheduleBackgroundJob(ScheduleJobRequest request, IHeaderDictionary customHeaders);

    /// <summary>
    /// Get the job status for a previously started job
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <param name="customHeaders">Any custom headers to be passed with the request</param>
    /// <returns>A Job Status, if it is completed, a Download URL will be included</returns>
    Task<JobStatusResult> GetBackgroundJobStatus(string jobId, IHeaderDictionary customHeaders);

    /// <summary>
    /// Get a Background Job Result if the Job is completed
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <param name="customHeaders">Any custom headers to be passed with the request</param>
    /// <returns>Stream Content matching the Result of the original job call</returns>
    Task<Stream> GetBackgroundJobResults(string jobId, IHeaderDictionary customHeaders);

    /// <summary>
    /// Schedule a VSS job via Scheduler, behind TPAAS.
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="customHeaders">Any custom headers to be passed to the Scheduler</param>
    /// <returns>A Scheduled Job Result, with a Job ID to check status</returns>
    Task<ScheduleJobResult> ScheduleVSSJob(JobRequest request, IHeaderDictionary customHeaders);

  }
}
