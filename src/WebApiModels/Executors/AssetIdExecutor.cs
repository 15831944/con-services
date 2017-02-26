﻿using System;
using System.Linq;
using System.Net;
using VSS.Device.Data;
using VSS.Project.Data;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{
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

      long legacyAssetId = -1;
      int serviceType = 0;
      bool result = false;

      //Special case: Allow manual import of tag file if user has manual 3D subscription.
      //ProjectID is -1 for auto processing of tag files and non-zero for manual processing.
      //Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.

      if (string.IsNullOrEmpty(request.radioSerial) || request.deviceType == (int)DeviceTypeEnum.MANUALDEVICE)
      {
        //Check for manual 3D subscription for customer, Only allowed to process tag file if project Id is > 0.
        //If ok then set asset Id to -1 so Raptor knows it's a John Doe machine and set machineLevel to 18 

        // todo cache project and project subs
        // todo use repo factory properly once interface available
        // todo in validate check that if !radioSerial and manual device type that there IS a projectID
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProjectAndSubscriptions(request.projectId, DateTime.UtcNow.Date);
        var projectSubs = p.Result.ToList();
        if (projectSubs.Count() > 0)
        {
          // todo
          //CheckForManual3D(projectID, out assetID, out machineLevel);
        }
      }
      else
      {
        //Radio serial in tag file. Use it to map to asset in VL.
        // todo cache asset
        // todo use repo factory properly once interface available
        DeviceTypeEnum whatever = (DeviceTypeEnum)request.deviceType;

        var assetRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = assetRepo.GetAssociatedAsset(request.radioSerial, whatever.ToString()); //  (DeviceTypeEnum)request.deviceType);
        var assetDevice = a.Result;
        if (assetDevice != null)
        {
          legacyAssetId = assetDevice.LegacyAssetID;

          // todo check subs
          //  LoadServiceViewCache(assetID);
          //  machineLevel = (int)GetProjectServiceType(assetID, projectID);
        }
        else
        {
          // todo check subs
          //  CheckForManual3D(projectID, out assetID, out machineLevel);
        }
      }

      result = !((legacyAssetId == -1) && (serviceType == 0));

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
  }
}

/***** stuff
 *    // todo raptor only handles currently assigned devices, not historical

    //AssetData asset = null;
    //if (string.IsNullOrEmpty(radioSerial)
    //var haveGotAsset = AssetIDCache.GetAssetID(radioSerial, (DeviceTypeEnum)deviceType) ?? -1;

    //if (haveGotAsset)
    //    {
    //  // proceed as got assetInfo haveGotAsset.Result()
    //  asset = new AssetData() { haveGotAsset.Result };

    //  // todo Manu3d is customer, not asset based. Why dos CG check against the asset?
    //  LoadAssetBasedSubscriptions(asset.AssetUID, new List<int> {(int) ServiceTypeEnumNG.e3DProjectMonitoring });

    //  LoadCustomerBasedSubscriptions(asset.AssetUID, new List<int> {(int) ServiceTypeEnumNG.Manual3DProjectMonitoring });

    //  LoadProjectBasedSubscriptions(asset.AssetUID, new List<int> { (int)ServiceTypeEnumNG.Landfill,
    //                            (int) ServiceTypeEnumNG.ProjectMonitoring });
    //});

    //  machineLevel = (int)GetProjectServiceType(assetID, projectID);
    //}

    //else // no assetID supplied/found, or a manualDeviceType. So get any projects whose ownerCustomer has a Manual3d plan
    //{
    //  CheckForManual3D(projectID, out assetID, out machineLevel);
    //}
    //if (string.IsNullOrEmpty(radioSerial) || deviceType == (int)DeviceTypeEnumCG.MANUALDEVICE)
    //{
    //  select AssetUID, LegacyAssetID
    //     from Device d
    //        inner join AssetDevice ad on ad.DeviceUID = d.DeviceUID
    //        inner join Asset ad on ad.AssetUID =
    //     where d.GPSDeviceID = radioSerial
    //       and d.DeviceTypeID = deviceTypeMapped
    //    }
    //else
    //{
    //  //Radio serial in tag file. Use it to map to asset in VL.
    //  AssetIDCache.Init();
    //  assetID = AssetIDCache.GetAssetID(radioSerial, (DeviceTypeEnum)deviceType) ?? -1;

    //  if (assetID != -1)
    //  {
    //    LoadServiceViewCache(assetID);

    //    machineLevel = (int)GetProjectServiceType(assetID, projectID);
    //  }
    //  else
    //  {
    //    CheckForManual3D(projectID, out assetID, out machineLevel);
    //  }
    //}

    //result = !((assetID == -1) && (machineLevel == (int)ServiceTypeEnum.Unknown));

    private List<subs.Subscription> LoadAssetBasedSubscriptions(Guid assetUID, List<string> serviceTypes)
    {
      //SELECT 'Asset' as ServiceFamilyType, sType.Name AS ServiceType, aSub.CustomerUID, null AS ProjectUID, AssetUID, StartDate, EndDate
      //  FROM AssetSubscription aSub
      //    INNER JOIN ServiceType sType ON sType.ServiceTypeID = aSub.fk_ServiceTypeID
      //  WHERE aSub.fk_AssetUID = @assetUID
      //    AND @nowUTC between aSub.StartDate and aSub.EndDate
      //    AND sType.Name like 'Manual 3D Project Monitoring'
      return new List<subs.Subscription>();
    }

    private List<subs.Subscription> LoadCustomerBasedSubscriptions(Guid customerUID, List<string> serviceTypes)
    {
      //SELECT 'Customer' as ServiceFamilyType, sType.Name AS serviceType, cSub.fk_CustomerUID, null AS ProjectUID, null AS AssetUID, StartDate, EndDate
      //  FROM CustomerSubscription cSub
      //    INNER JOIN ServiceType sType ON sType.ServiceTypeID = cSub.fk_ServiceTypeID
      //  WHERE cSub.fk_CustomerUID = @customerUID
      //    AND @nowUTC between cSub.StartDate and cSub.EndDate
      //    AND sType.Name like '3D Project Monitoring'
      return new List<subs.Subscription>();
    }

    private List<subs.Subscription> LoadProjectBasedSubscriptions(Guid projectUID, List<string> serviceTypes)
    {
      //SELECT 'Project' as ServiceFamilyType, sType.Name AS serviceType, pSub.fk_CustomerUID, pSub.fk_ProjectUID AS ProjectUID, null AS AssetUID, StartDate, EndDate
      //  FROM ProjectSubscription pSub
      //    INNER JOIN ServiceType sType ON sType.ServiceTypeID = pSub.fk_ServiceTypeID
      //  WHERE pSub.fk_ProjectUID = @projectUID
      //    AND @nowUTC between pSub.StartDate AND pSub.EndDate
      //    AND sType.Name IN ('Landfill', 'Project Monitoring')
      return new List<subs.Subscription>();
    }

  ****/