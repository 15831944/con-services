﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Filters
{
  public class ExceptionsTrap
  {
    private readonly ILogger log;
    private readonly RequestDelegate _next;

    public ExceptionsTrap(RequestDelegate next, ILoggerFactory logger)
    {
      log = logger.CreateLogger<ExceptionsTrap>();
      _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
      try
      {
        await _next.Invoke(context);
      }
      catch (ServiceException ex)
      {
        log.LogInformation("SERVICEEXCEPTION: {0}, {1}", ex.GetContent, ex.GetContent);
        context.Response.StatusCode = (int)ex.Code;
        //Don't write any body for 204
        if (ex.Code != HttpStatusCode.NoContent)
        {
          await context.Response.WriteAsync(ex.GetContent);
        }
      }
      catch (Exception ex)
      {
        try
        {
          context.Response.StatusCode = 500;
          await context.Response.WriteAsync(ex.Message);
        }
        finally
        {
          log.LogCritical("EXCEPTION: {0}, {1}, {2}", ex.Message, ex.Source, ex.StackTrace);
          if (ex is AggregateException)
          {
            var exception = ex as AggregateException;
            log.LogCritical("EXCEPTION AGGREGATED: {0}, {1}, {2}",
                exception.InnerExceptions.Select(i => i.Message).Aggregate((i, j) => i + j),
                exception.InnerExceptions.Select(i => i.Source).Aggregate((i, j) => i + j),
                exception.InnerExceptions.Select(i => i.StackTrace).Aggregate((i, j) => i + j));

          }
        }
      }
    }
  }

  public static class ExceptionsTrapExtensions
  {
    public static IApplicationBuilder UseExceptionTrap(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<ExceptionsTrap>();
    }
  }
}
