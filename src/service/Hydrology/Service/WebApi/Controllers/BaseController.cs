﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Serilog.Extensions;
using ILoggerFactory = log4net.Repository.Hierarchy.ILoggerFactory;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// Common base class for Raptor service controllers.
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {
    private ILogger<T> logger;
    private ILoggerFactory loggerFactory;
    private IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IServiceExceptionHandler ServiceExceptionHandler => serviceExceptionHandler ?? (serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    protected ILogger<T> Log => logger ?? (logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    /// <summary>
    /// Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders.
    /// </summary>
    protected ILoggerFactory LoggerFactory => loggerFactory ?? (loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore ConfigStore;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly MemoryCacheEntryOptions filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    /// <summary>
    /// Indicates whether to use the TRex Gateway instead of calling to the Raptor client.
    /// </summary>
    protected bool UseTRexGateway(string key) => bool.TryParse(ConfigStore.GetValueString(key), out var useTrexGateway) && useTrexGateway;

    /// <summary>
    /// 
    /// </summary>
    protected BaseController(IConfigurationStore configStore)
    {
      ConfigStore = configStore;
    }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = action.Invoke();
        if (Log.IsTraceEnabled())
          Log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, errorMessage1: ex.Message, innerException: ex);
      }
      finally
      {
        Log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }

      return result;
    }

    /// <summary>
    /// With the service exception try execute async.
    /// </summary>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = await action.Invoke();
        if (Log.IsTraceEnabled())
          Log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");

      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, ex.Message, innerException: ex);
      }
      finally
      {
        Log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }
  }
}
