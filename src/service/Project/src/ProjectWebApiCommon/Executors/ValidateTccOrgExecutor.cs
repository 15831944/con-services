﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which validates the tcc orgShortName received
  ///   Validates against TCC and the ProjectService database.
  /// </summary>
  public class ValidateTccOrgExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Processes the ValidateTcc orgShortName against customerUid
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var validateTccAuthorizationRequest = CastRequestObjectTo<ValidateTccAuthorizationRequest>(item, errorCode: 86);

      try
      {
        var organizations = await fileRepo.ListOrganizations();
        var tccOrganization
          = organizations.FirstOrDefault(o => o.shortName == validateTccAuthorizationRequest.OrgShortName);
        if (tccOrganization == null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 88);
        }
        else if (string.IsNullOrEmpty(tccOrganization.filespaceId) || string.IsNullOrEmpty(tccOrganization.orgId))
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 89,
            JsonConvert.SerializeObject(tccOrganization));
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 87, e.Message);
      }

      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}