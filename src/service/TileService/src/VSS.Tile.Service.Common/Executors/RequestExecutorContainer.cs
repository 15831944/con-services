﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Tile.Service.Common.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.Common.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from controller logic for testability and possible executor versioning.%
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    private const string ERROR_MESSAGE = "Failed to get/update data requested by {0}";
    private const string ERROR_MESSAGE_EX = "{0} with error: {1}";
    private const int ERROR_STATUS_OK = 0;

    /// <summary>
    /// Logger for logging
    /// </summary>
    protected ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore configStore;

    protected IHeaderDictionary customHeaders;

    protected IDataOceanClient dataOceanClient;

    protected ITPaaSApplicationAuthentication authn;

    protected IProductivity3dV2ProxyCompactionTile productivity3DProxyCompactionTile;

    protected IBoundingBoxHelper bboxHelper;

    /// <summary>
    /// Gets the available contract execution error states.
    /// </summary>
    /// <value>
    /// The contract execution states.
    /// </value>
    protected ContractExecutionStatesEnum ContractExecutionStates { get; }

    /// <summary>
    /// Default constructor which creates all structures necessary for error handling.
    /// </summary>
    protected RequestExecutorContainer()
    {
      ContractExecutionStates = new ContractExecutionStatesEnum();
    }

    /// <summary>
    /// Generates the dynamic errorlist for instanciated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      List<Tuple<int, string>> result = new List<Tuple<int, string>>();
      for (int i = 0; i < ContractExecutionStates.Count; i++)
      {
        result.Add(new Tuple<int, string>(ContractExecutionStates.ValueAt(i),
               ContractExecutionStates.NameAt(i)));
      }
      ContractExecutionStates.ClearDynamic();
      return result;
    }

    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    protected virtual Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Missing asynchronous executor process method override"));
    }

    public ContractExecutionResult Process<T>(T item)
    {
      ValidateTItem(item);
      return ProcessEx(item);
    }

    public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      ValidateTItem(item);
      return await ProcessAsyncEx(item);
    }

    private static void ValidateTItem<T>(T item)
    {
      if (item == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      }
    }

    /// <summary>
    /// Dynamically defines new error codes for the executor instance. Don't forget to clean them up after exit.
    /// </summary>
    protected virtual void ProcessErrorCodes()
    { }

    public void Initialise(ILogger logger, IConfigurationStore configStore,
      IHeaderDictionary customHeaders, IDataOceanClient dataOceanClient,
      ITPaaSApplicationAuthentication authn, IProductivity3dV2ProxyCompactionTile productivity3DProxyCompactionTile, IBoundingBoxHelper bboxHelper)
    {
      log = logger;
      this.configStore = configStore;
      this.customHeaders = customHeaders;
      this.dataOceanClient = dataOceanClient;
      this.authn = authn;
      this.productivity3DProxyCompactionTile = productivity3DProxyCompactionTile;
      this.bboxHelper = bboxHelper;
    }

    protected void ThrowRequestTypeCastException<T>(string errorMessage = null)
    {
      if (errorMessage == null)
        errorMessage = $"{typeof(T).Name} cast failed.";

      throw new ServiceException(
        HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, errorMessage));
    }

    protected ServiceException CreateServiceException<T>(int errorStatus = ERROR_STATUS_OK)
    {
      var errorMessage = string.Format(ERROR_MESSAGE, typeof(T).Name);

      if (errorStatus > ERROR_STATUS_OK)
        errorMessage = string.Format(ERROR_MESSAGE_EX, errorMessage, ContractExecutionStates.FirstNameWithOffset(errorStatus));

      return new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, errorMessage));
    }

    /// <summary>
    /// Default destructor which destroys all structures necessary for error handling.
    /// </summary>
    ~RequestExecutorContainer()
    {
      ContractExecutionStates?.ClearDynamic();
    }
  }
}
