﻿using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectAndAssetUidsExecutor : RequestExecutorContainer
  {
    public ICustomRadioSerialProjectMap CustomRadioSerialMapper { get; set; }

    ///  <summary>
    ///  There are 2 modes this may be called in:
    ///     a projectUid is provided, for which we determine if            
    ///            either the projects customer has a paying-devicePackage (>0 i.e. not Free)
    ///              or the asset (if provided) Customer has a paying-devicePackage AND the project is owned by the same customer
    ///          and the location is inside the project
    /// 
    ///  b) Auto Import
    ///     a deviceSerial provided.
    ///     This must be resolvable and its customer must have paying-devicePackage
    ///     A customers active projects cannot overlap spatially at the same point-in-time
    ///                  therefore this should legitimately retrieve max of ONE match
    ///    
    ///  if a deviceSerial/dtype is provided and can be resolved, the deviceUid will also be returned.
    ///  Archived projects are not considered, also note that there are only standard projects available
    ///
    ///  TFA has the capability to be provided a radio/device type -> Asset/Project map to cover special cases
    ///  where a device has no provisioning but we want to bring it into a known project. In this case, if the
    ///  radio serial number and device type are found in the map, the item is processed as if were a manual
    ///  import into the project, under the asset, located in the map
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));
      }

      // Radio serial -> Asset/Project override
      if (CustomRadioSerialMapper.LocateAsset(request.RadioSerial, request.DeviceType, out var id))
      {
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: LocateAsset id {JsonConvert.SerializeObject(id)}");
        return GetProjectAndAssetUidsResult.FormatResult(id.ProjectUid.ToString(), id.AssetUid.ToString());
      }

      ProjectData project = null;

      // manualImport, the project must be there and have deviceLicenses
      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        project = await dataRepository.GetProject(request.ProjectUid);
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded project? {JsonConvert.SerializeObject(project)}");

        if (project != null)
        {
          if (project.IsArchived)
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 43);
          var projectAccountDeviceLicenseTotal = await dataRepository.GetDeviceLicenses(project.CustomerUID);
          log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded ProjectAccount deviceLicenses? {JsonConvert.SerializeObject(projectAccountDeviceLicenseTotal)}");
          if (projectAccountDeviceLicenseTotal < 1)
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 31);
        }
        else
        {
          return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 38);
        }
      }

      DeviceData device = null;
      // a CB will have a RadioSerial, whose suffix defines the type
      if (!string.IsNullOrEmpty(request.RadioSerial))
      {
        device = await dataRepository.GetDevice(request.RadioSerial);
        var deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: { contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by RadioSerial?: {request.RadioSerial} device: {JsonConvert.SerializeObject(device)} {deviceStatus}");
      }

      if ((device == null || device.Code != 0 || device.DeviceUID == null) && !string.IsNullOrEmpty(request.Ec520Serial))
      {
        device = await dataRepository.GetDevice(request.Ec520Serial);
        var deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: { contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by Ec520Serial?: {request.Ec520Serial} device: {JsonConvert.SerializeObject(device)} {deviceStatus}");
      }

      if (!string.IsNullOrEmpty(request.ProjectUid))
        return await HandleManualImport(request, project, device);

      if (device == null || device.Code != 0 || device.DeviceUID == null)
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: device?.Code ?? 47);

      return await HandleAutoImport(request, device);
    }


    private async Task<GetProjectAndAssetUidsResult> HandleManualImport(GetProjectAndAssetUidsRequest request,
      ProjectData project, DeviceData device = null)
    {
      // by this stage...
      //  got an active project, with a payed-DeviceEntitlement,
      //  possibly identified a device
      //
      //  Rules:
      //  Can manually import tag files regardless if tag file time outside projectTime
      //  Can manually import tag files where we don't know the device
      //  Can manually import tag files where we don't know the device, and regardless of the device customers deviceEntitlement
      //  If device is available, it must be associated with the project
      //  For ManualImport we want to maximize ability so don't bother checking deviceStatus?

      var intersectingProjects = await dataRepository.GetIntersectingProjectsForManual(project, request.Latitude,
            request.Longitude, device, request.Northing, request.Easting);
      log.LogDebug(
        $"{nameof(HandleManualImport)}: GotIntersectingProjectsForManual: {JsonConvert.SerializeObject(intersectingProjects)}");

      if (!intersectingProjects.ProjectDescriptors.Any())
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device == null ? string.Empty : device.DeviceUID, uniqueCode: 41);

      if (intersectingProjects.ProjectDescriptors.Count > 1)
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device == null ? string.Empty : device.DeviceUID, uniqueCode: 49);

      return GetProjectAndAssetUidsResult.FormatResult(project.ProjectUID, device == null ? string.Empty : device.DeviceUID);
    }

    private async Task<GetProjectAndAssetUidsResult> HandleAutoImport(GetProjectAndAssetUidsRequest request,
      DeviceData device)
    {
      var deviceAccountDeviceLicenseTotal = await dataRepository.GetDeviceLicenses(device.CustomerUID);
      log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded DeviceAccount deviceLicenses? {JsonConvert.SerializeObject(deviceAccountDeviceLicenseTotal)}");
      if (deviceAccountDeviceLicenseTotal < 1)
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device.DeviceUID, uniqueCode: 1);

      var potentialProjects = dataRepository.GetIntersectingProjectsForDevice(device, request.Latitude, request.Longitude, request.Northing, request.Easting, out var errorCode);

      log.LogDebug(
        $"{nameof(HandleAutoImport)}: GotIntersectingProjectsForDevice: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.ProjectDescriptors.Any())
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device.DeviceUID, uniqueCode: errorCode);

      if (potentialProjects.ProjectDescriptors.Count > 1)
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device.DeviceUID, uniqueCode: 49);

      return GetProjectAndAssetUidsResult.FormatResult(potentialProjects.ProjectDescriptors[0].ProjectUID, device.DeviceUID);
    }
    
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
