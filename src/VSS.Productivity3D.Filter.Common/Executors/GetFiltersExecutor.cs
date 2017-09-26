﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFiltersExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFiltersExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler, 
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy,
      IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) 
      : base(configStore, logger, serviceExceptionHandler, 
          projectListProxy, raptorProxy,
          filterRepo, producer, kafkaTopicName)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetFiltersExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetFilters request for a project
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;

      var filterRequest = item as FilterRequestFull;
      if (filterRequest != null)
      {
        IEnumerable<MasterData.Repositories.DBModels.Filter> filters = null;

        // get all for projectUid where !deleted 
        //   must be ok for 
        //      customer /project
        //      and UserUid: If the calling context is == Application, then get all 
        //                     else get only those for the calling UserUid
        try
        {
          filters = (await filterRepo
            .GetFiltersForProjectUser(filterRequest.CustomerUid, filterRequest.ProjectUid, filterRequest.UserId)
            .ConfigureAwait(false));

        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10, e.Message);
        }

        // may be none, return success and empty list
        result = new FilterDescriptorListResult
        {
          filterDescriptors = filters.Select(filter =>
              AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
            .ToImmutableList()
        };
      }
      else
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 9);
      }

      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }

  }
}