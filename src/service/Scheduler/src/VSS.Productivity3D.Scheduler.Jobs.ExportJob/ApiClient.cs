﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for requesting data from a web api.
  /// </summary>
  public class ApiClient : IApiClient
  {
    private readonly ILogger _log;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApiClient(ILoggerFactory logger, IServiceScopeFactory serviceScope)
    {
      _log = logger.CreateLogger<ApiClient>();
      _scopeFactory = serviceScope;
    }

    /// <summary>
    /// Send an HTTP request to the requested URL
    /// </summary>
    /// <param name="jobRequest">Details of the job request</param>
    /// <param name="customHeaders">Custom HTTP headers for the HTTP request</param>
    /// <returns>The result of the HTTP request as an instance of T</returns>
    public async Task<T> SendRequest<T>(ScheduleJobRequest jobRequest, IHeaderDictionary customHeaders)
    {
      var result = default(T);
      var method = new HttpMethod(jobRequest.Method ?? "GET");
      _log.LogDebug($"Job request is {JsonConvert.SerializeObject(jobRequest)}");
      try
      {
        // Merge the Custom headers passed in with the http request, and the headers requested by the Schedule Job
        foreach (var header in jobRequest.Headers)
        {
          if (!customHeaders.ContainsKey(header.Key))
          {
            customHeaders[header.Key] = header.Value;
          }
          else
          {
            _log.LogDebug($"HTTP Header '{header.Key}' exists in both the web requests and job request headers, using web request value. Web Request Value: '${customHeaders[header.Key]}', Job Request Value: '${header.Value}'");
          }
        }

        using var serviceScope = _scopeFactory.CreateScope();
        var webRequest = serviceScope.ServiceProvider.GetService<IWebRequest>();

        // The Schedule job request may contain encoded binary data, or a standard string,
        // We need to handle both cases differently, as we could lose data if converting binary information to a string
        if (jobRequest.IsBinaryData)
        {
          using var ms = new MemoryStream(jobRequest.PayloadBytes);
          result = await webRequest.ExecuteRequest<T>(jobRequest.Url, ms, customHeaders, method, jobRequest.Timeout, 0);
        }
        else if (!string.IsNullOrEmpty(jobRequest.Payload))
        {
          using var ms = new MemoryStream(Encoding.UTF8.GetBytes(jobRequest.Payload));
          result = await webRequest.ExecuteRequest<T>(jobRequest.Url, ms, customHeaders, method, jobRequest.Timeout, 0);
        }
        else
        {
          // Null payload (which is ok), so we don't need a stream
          result = await webRequest.ExecuteRequest<T>(jobRequest.Url, null, customHeaders, method, jobRequest.Timeout, 0);
        }

        _log.LogDebug($"Result of send request: {JsonConvert.SerializeObject(result)}");
      }
      catch (Exception ex)
      {
        var message = ex.Message;
        var stacktrace = ex.StackTrace;
        //Check for 400 and 500 errors which come through as an inner exception
        if (ex.InnerException != null)
        {
          message = ex.InnerException.Message;
          stacktrace = ex.InnerException.StackTrace;
        }
        _log.LogWarning("Error sending data: ", message);
        _log.LogWarning("Stacktrace: ", stacktrace);
        throw;
      }
      return result;
    }

    /// <summary>
    /// Send an HTTP request to the requested URL
    /// </summary>
    /// <param name="jobRequest">Details of the job request</param>
    /// <param name="customHeaders">Custom HTTP headers for the HTTP request</param>
    /// <returns>The result of the HTTP request as a stream</returns>
    public async Task<HttpContent> SendRequestStream(ScheduleJobRequest jobRequest, IHeaderDictionary customHeaders)
    {
      HttpContent result = null;
      var method = new HttpMethod(jobRequest.Method ?? "GET");
      _log.LogDebug($"Job request is {JsonConvert.SerializeObject(jobRequest)}");
      try
      {
        // Merge the Custom headers passed in with the http request, and the headers requested by the Schedule Job
        foreach (var header in jobRequest.Headers)
        {
          if (!customHeaders.ContainsKey(header.Key))
          {
            customHeaders[header.Key] = header.Value;
          }
          else
          {
            _log.LogDebug($"HTTP Header '{header.Key}' exists in both the web requests and job request headers, using web request value. Web Request Value: '${customHeaders[header.Key]}', Job Request Value: '${header.Value}'");
          }
        }

        using var serviceScope = _scopeFactory.CreateScope();
        var webRequest = serviceScope.ServiceProvider.GetService<IWebRequest>();

        // The Schedule job request may contain encoded binary data, or a standard string,
        // We need to handle both cases differently, as we could lose data if converting binary information to a string
        if (jobRequest.IsBinaryData)
        {
          using var ms = new MemoryStream(jobRequest.PayloadBytes);
          result = await webRequest.ExecuteRequestAsStreamContent(jobRequest.Url, method, customHeaders, ms, jobRequest.Timeout, 0);
        }
        else if (!string.IsNullOrEmpty(jobRequest.Payload))
        {
          using var ms = new MemoryStream(Encoding.UTF8.GetBytes(jobRequest.Payload));
          result = await webRequest.ExecuteRequestAsStreamContent(jobRequest.Url, method, customHeaders, ms, jobRequest.Timeout, 0);
        }
        else
        {
          // Null payload (which is ok), so we don't need a stream
          result = await webRequest.ExecuteRequestAsStreamContent(jobRequest.Url, method, customHeaders, null, jobRequest.Timeout, 0);
        }

        _log.LogDebug("Result of send request: Stream Content={0}", result);
      }
      catch (Exception ex)
      {
        var message = ex.Message;
        var stacktrace = ex.StackTrace;
        //Check for 400 and 500 errors which come through as an inner exception
        if (ex.InnerException != null)
        {
          message = ex.InnerException.Message;
          stacktrace = ex.InnerException.StackTrace;
        }
        _log.LogWarning("Error sending data: ", message);
        _log.LogWarning("Stacktrace: ", stacktrace);
        throw;
      }
      return result;
    }
  }
}
