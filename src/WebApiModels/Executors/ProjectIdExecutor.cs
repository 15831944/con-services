﻿using Repositories;
using Repositories.DBModels;
using Repositories.ExtendedModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project id request and finds the id of the project corresponding to the given location and asset/tccorgID and relavant subscriptions.
    /// 
    ///  A device asset reports at a certain location, at a point in time -
    ///        which project should its data be accumulating into?
    ///    assumption1: A customers projects cannot overlap spatially at the same point-in-time
    ///                 this applies to construction and Landfill types
    ///                 therefore this should legitimately retrieve max of ONE match
    ///    assumption2: tag files are data type-generic at this level, so this function does not need to
    ///                 differentiate between the 3 subscription types.
    ///    assumption3: the customer must be identifiable by EITHER the AssetID, or TCCOrgID being supplied
    ///                 only projects for that customer are fair game.
    ///   
    ///    A construction project is only fair game if an assetID is provided
    ///    A landfill project is fair game for an aasetID or a TCCOrgID
    ///
    ///    determine the union (ONE) of the following:
    ///    1) which projects were valid at this time?
    ///    2) which customers have a machineControl-type subscription for at this time? (for construction type projects)
    ///        a) for the asset provided OR
    ///        b) any assets if -1 is provided
    ///    3) which project.sites are these points are in?
    ///    
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectIdResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectIdRequest request = item as GetProjectIdRequest;
      log.LogDebug("AssetIdExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      long projectId = -1; // none found
      IEnumerable<Project> potentialProjects = null;

      Asset asset = null;
      IEnumerable<SubscriptionData> assetSubs = null;
      CustomerTccOrg customerTCCOrg = null;
      CustomerTccOrg customerAssetOwner = null;

      // must be able to find one or other customer for a) tccOrgUid b) legacyAssetID, whichever is provided
      if (!string.IsNullOrEmpty(request.tccOrgUid))
      {
        var g = LoadCustomerByTccOrgId(request.tccOrgUid);
        customerTCCOrg = g.Result != null ? g.Result : null;
      }

      // assetId could be valid (>0) or -1 (john doe i.e. landfill) or -2 (imported tagfile)
      if (request.assetId > 0)
      {
        asset = LoadAsset(request.assetId);
        if (asset != null && !string.IsNullOrEmpty(asset.OwningCustomerUID))
        {
          customerAssetOwner = LoadCustomerByCustomerUID(asset.OwningCustomerUID);
          assetSubs = LoadAssetSubs(asset.AssetUID, request.timeOfPosition);
        }
      }

      if (customerTCCOrg != null || customerAssetOwner != null)
      {
        //Look for projects with (request.latitude, request.longitude) inside their boundary
        //and belonging to customers who have a Project Monitoring subscription
        //for asset with id request.assetId at time request.timeOfPosition 
        //and the customer owns the asset. (In VL multiple customers can have subscriptions
        //for an asset but only the owner gets the tag file data).

        ProjectRepository projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;

        //  standard 2d / 3d project aka construction project
        //    IGNORE any tccOrgID
        //    must have valid assetID, which must have a 3d sub.
        if (customerAssetOwner != null && assetSubs != null && assetSubs.Count() > 0)
        {
          var p = projectRepo.GetStandardProject(customerAssetOwner.CustomerUID, request.latitude, request.longitude, request.timeOfPosition);
          if (p.Result != null && p.Result.Count() > 0)
            potentialProjects = potentialProjects == null ? p.Result : potentialProjects.Concat(p.Result);
        }

        // ProjectMonitoring project
        //  MUST have a TCCOrgID
        //  customersOrgID must have a PM sub
        //  allow johnDoe assets(-1) and valid assetIDs(no manually imported tagfile(assetid = -2))
        if (customerTCCOrg != null && request.assetId != -2)
        {
          var p = projectRepo.GetProjectMonitoringProject(customerTCCOrg.CustomerUID,
                      request.latitude, request.longitude, request.timeOfPosition,
                      2 /*  ProjectMonitoring project type */, (int)ServiceTypeEnumNG.ProjectMonitoring);
          if (p.Result != null && p.Result.Count() > 0)
            potentialProjects = potentialProjects == null ? p.Result : potentialProjects.Concat(p.Result);
        }

        // Landfill project
        //   MUST have a TCCOrgID
        //   customersOrgID must have a PM sub
        //   allow manual assets(-2) and johnDoe assets(-1) and valid assetIDs
        if (customerTCCOrg != null)
        {
          var p = projectRepo.GetProjectMonitoringProject(customerTCCOrg.CustomerUID,
          request.latitude, request.longitude, request.timeOfPosition,
          1 /*  Landfill project type */, (int)ServiceTypeEnumNG.Landfill);
          if (p.Result != null && p.Result.Count() > 0)
            potentialProjects = potentialProjects == null ? p.Result : potentialProjects.Concat(p.Result);
        }

        //projectId
        //If zero found then returns -1
        //If one found then returns its id
        //If > 1 found then returns -2
        if (potentialProjects == null || potentialProjects.Count() == 0)
          projectId = -1;
        else
          if (potentialProjects.Distinct().Count() > 1)
          projectId = -2;
        else
          projectId = potentialProjects.ToList()[0].LegacyProjectID;
      }

      var result = projectId > 1;

      try
      {
        return GetProjectIdResult.CreateGetProjectIdResult(result, projectId);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project id"));
      }

    }
  }
}
