﻿using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VSS.WebApi.Common
{
  public class RequestTraceMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTraceMiddleware> log;



    public RequestTraceMiddleware(RequestDelegate next, ILoggerFactory logger)
    {
      log = logger.CreateLogger<RequestTraceMiddleware>();
      _next = next;
    }

    /// <summary>
    /// Invokes the specified context.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
      log.LogInformation($"Request {context.Request.Method} {context.Request.Path} {context.Request.QueryString.Value}");
      var watch = Stopwatch.StartNew();
      await _next.Invoke(context);
      watch.Stop();
      log.LogInformation($"Response {context.Response.StatusCode} {watch.ElapsedMilliseconds}ms");
    }
  }

}
