﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.DBModels;
using Repositories.ExtendedModels;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{
  /// <summary>
  /// The executor which gets a legacyAssetId and/or serviceType for the requested radioSerial and/or legacyProjectId.
  /// </summary>
  public class AssetIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get asset request and finds the id of the asset corresponding to the given tagfile radio serial number.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetAssetIdResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetAssetIdRequest request = item as GetAssetIdRequest;
      log.LogDebug("AssetIdExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      long legacyAssetId = -1;
      int serviceType = 0;
      bool result = false;

      Project project = null;
      IEnumerable<SubscriptionData> customerSubs = null;
      IEnumerable<SubscriptionData> assetSubs = null;

      // legacyProjectId can exist with and without a radioSerial so set this up early
      if (request.projectId > 0)
      {
        project = LoadProject(request.projectId);
        log.LogDebug("AssetIdExecutor: Loaded project? {0}", JsonConvert.SerializeObject(project));

        if (project != null)
        {
          customerSubs = LoadManual3DCustomerBasedSubs(project.CustomerUID, DateTime.UtcNow);
          log.LogInformation("AssetIdExecutor: Loaded projectsCustomerSubs? {0}", JsonConvert.SerializeObject(customerSubs));
        }
      }


      //Special case: Allow manual import of tag file if user has manual 3D subscription.
      //ProjectID is -1 for auto processing of tag files and non-zero for manual processing.
      //Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.
      if (string.IsNullOrEmpty(request.radioSerial) || request.deviceType == (int)DeviceTypeEnum.MANUALDEVICE)
      {
        //Check for manual 3D subscription for the projects customer, Only allowed to process tag file if legacyProjectId is > 0.
        //If ok then set asset Id to -1 so Raptor knows it's a John Doe machine and set serviceType machineLevel to 18 "Manual 3D PM"
        if (project != null)
        {          
          CheckForManual3DCustomerBasedSub(request.projectId, customerSubs, assetSubs, out legacyAssetId, out serviceType);
        }
      }
      else
      {
        //Radio serial in tag file. Use it to map to asset in VL.
        AssetDeviceIds assetDevice = LoadAssetDevice(request.radioSerial, ((DeviceTypeEnum)request.deviceType).ToString());

        // special case in CGen US36833 If fails on DT SNM940 try as again SNM941 
        if (assetDevice == null && (DeviceTypeEnum)request.deviceType == DeviceTypeEnum.SNM940)
        {
          log.LogInformation("AssetIdExecutor: Failed for SNM940 trying again as Device Type SNM941");
          assetDevice = LoadAssetDevice(request.radioSerial, DeviceTypeEnum.SNM941.ToString());
        }
        log.LogDebug("AssetIdExecutor: Loaded assetDevice? {0}", JsonConvert.SerializeObject(assetDevice));

        if (assetDevice != null)
        {
          legacyAssetId = assetDevice.LegacyAssetID;
          assetSubs = LoadAssetSubs(assetDevice.AssetUID, DateTime.UtcNow);
          log.LogDebug("AssetIdExecutor: Loaded assetSubs? {0}", JsonConvert.SerializeObject(assetSubs));

          // OwningCustomerUID should always be present, but bug in MD airlift means that most are missing.
          customerSubs = LoadManual3DCustomerBasedSubs(assetDevice.OwningCustomerUID, DateTime.UtcNow);
          log.LogDebug("AssetIdExecutor: Loaded assetsCustomerSubs? {0}", JsonConvert.SerializeObject(customerSubs));

          serviceType = GetMostSignificantServiceType(assetDevice.AssetUID, project, customerSubs, assetSubs);
          log.LogInformation("AssetIdExecutor: after GetMostSignificantServiceType(). AssetUID {0} project{1} custSubs {2} assetSubs {3}", 
            assetDevice.AssetUID, JsonConvert.SerializeObject(project), JsonConvert.SerializeObject(customerSubs), JsonConvert.SerializeObject(assetSubs));
        }
        else
        {          
          CheckForManual3DCustomerBasedSub(request.projectId, customerSubs, assetSubs, out legacyAssetId, out serviceType);
        }
      }

      result = !((legacyAssetId == -1) && (serviceType == 0));
      log.LogInformation("AssetIdExecutor: All done. result {0} legacyAssetId {1} serviceType {2}", result, legacyAssetId, serviceType);

      try
      {
        return GetAssetIdResult.CreateGetAssetIdResult(result, legacyAssetId, serviceType);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get legacy asset id"));
      }

    }


    private void CheckForManual3DCustomerBasedSub(long legacyProjectId,
              IEnumerable<SubscriptionData> customerSubs, IEnumerable<SubscriptionData> assetSubs,
              out long legacyAssetId, out int serviceType)
    {
      // these are CustomerBased and no legacyAssetID will be returned
      legacyAssetId = -1;
      serviceType = (int)ServiceTypeEnumNG.Unknown;
      log.LogDebug("AssetIdExecutor: CheckForManual3DCustomerBasedSub(). projectId {0} custSubs {1} assetSubs {2}", legacyProjectId, JsonConvert.SerializeObject(customerSubs), JsonConvert.SerializeObject(assetSubs));

      if (legacyProjectId > 0)
      {
        log.LogDebug("AssetIdExecutor: project ID non-zero so manual import for project - about to check for manual 3D subscription. legacyProjectId {0}", legacyProjectId);
      
        if (customerSubs != null && customerSubs.Count() > 0)
        {
          legacyAssetId = -1;   //Raptor needs to know it's a John Doe machine i.e. not a VL asset
          serviceType = (int)ServiceTypeEnumCG.Manual3DProjectMonitoring;
        }
      }
      log.LogInformation("AssetIdExecutor: CheckForManual3DCustomerBasedSub(). legacyAssetId {0} serviceType {1}", legacyAssetId, serviceType);
    }

    private int GetMostSignificantServiceType(string assetUID, Project project,
      IEnumerable<SubscriptionData> customerSubs, IEnumerable<SubscriptionData> assetSubs)
    {
      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() for asset UID {0} and project UID {1}", assetUID, JsonConvert.SerializeObject(project));

      ServiceTypeEnumNG serviceType = ServiceTypeEnumNG.Unknown;

      IEnumerable<SubscriptionData> subs = new List<SubscriptionData>();
      if (customerSubs != null && customerSubs.Count() > 0) subs = subs.Concat(customerSubs.Select(s => s));
      if (assetSubs != null && assetSubs.Count() > 0)
      {
        subs = subs.Concat(assetSubs.Select(s => s));
      }
      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() subs being checked {0}", JsonConvert.SerializeObject(subs));

      if (subs != null && subs.Count() > 0)
      {
        //Look for highest level machine subscription which is current
        int utcNowKeyDate = DateTime.UtcNow.KeyDate();
        foreach (SubscriptionData sub in subs)
        {
          switch ((ServiceTypeEnumNG)sub.serviceTypeId)
          {
            // Manual3d is least significant
            case ServiceTypeEnumNG.Manual3DProjectMonitoring:
              if (serviceType != ServiceTypeEnumNG.e3DProjectMonitoring)
              {
                log.LogDebug("AssetIdExecutor: GetProjectServiceType found ServiceTypeEnum.Manual3DProjectMonitoring for asset UID {0}", assetUID);
                serviceType = ServiceTypeEnumNG.Manual3DProjectMonitoring;
              }
              break;

            // 3D PM is most significant
            // if 3D asset-based, the assets customer must be the same as the Projects customer 
            case ServiceTypeEnumNG.e3DProjectMonitoring:
              if (serviceType != ServiceTypeEnumNG.e3DProjectMonitoring)
              {
                //Allow manual tag file import for customer who has the 3D subscription for the asset
                //and allow automatic tag file processing in all cases (can't tell customer for automatic)
                log.LogDebug("AssetIdExecutor: GetProjectServiceType found ServiceTypeEnum.e3DProjectMonitoring for asset UID {0}", assetUID);
                if (project == null || sub.customerUid == project.CustomerUID)
                {
                  serviceType = ServiceTypeEnumNG.e3DProjectMonitoring;
                }
              }
              break;
            default:
              break;
          }
        }
      }

      log.LogDebug("AssetIdExecutor: GetMostSignificantServiceType() for asset ID {0}, returning serviceTypeNG {1} actually serviceTypeCG {2}", assetUID, serviceType, ConvertServiceTypeNGtoCG(serviceType));
      return (int)(ConvertServiceTypeNGtoCG(serviceType));
    }

    private ServiceTypeEnumCG ConvertServiceTypeNGtoCG(ServiceTypeEnumNG serviceTypeNG)
    {
      return (ServiceTypeEnumCG)ServiceTypeEnumCG.Parse(typeof(ServiceTypeEnumCG), serviceTypeNG.ToString());
    }
  }
}
    