﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IFileImportProxy fileImportProxy,
      RepositoryBase repository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, fileImportProxy, repository, null /*, null, null */)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetFilterExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetFilters Request
    /// </summary>
    /// <returns>If successful returns a <see cref="FilterDescriptorSingleResult"/> object containing the filter.</returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<FilterRequestFull>(item, 5);
      if (request == null) return null;

      MasterData.Repositories.DBModels.Filter filter = null;
      // get FilterUid where !deleted 
      //   must be ok for 
      //      customer /project
      //      and UserUid: If the calling context is == Application, then get all 
      //                     else get only those for the calling UserUid
      try
      {
        filter = await ((IFilterRepository)Repository).GetFilter(request.FilterUid);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 6, e.Message);
      }

      if (filter == null
          || !string.Equals(filter.CustomerUid, request.CustomerUid, StringComparison.OrdinalIgnoreCase)
          || !string.Equals(filter.ProjectUid, request.ProjectUid, StringComparison.OrdinalIgnoreCase)
          || !string.Equals(filter.UserId, request.UserId, StringComparison.OrdinalIgnoreCase) && !request.IsApplicationContext
      )
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 36);
      }


      await FilterJsonHelper.ParseFilterJson(request.ProjectData, filter, Productivity3dV2ProxyCompaction, request.CustomHeaders);

      return new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));
    }
  }
}
