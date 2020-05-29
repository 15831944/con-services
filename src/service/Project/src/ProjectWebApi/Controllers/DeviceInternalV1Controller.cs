﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Device controller v1
  ///     for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class DeviceInternalV1Controller : BaseController<DeviceInternalV1Controller>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public DeviceInternalV1Controller()
    { }

    /// <summary>
    /// Gets device by serialNumber
    ///  called by TFA AssetIdExecutor, ProjectAndAssetUidsEarthWorksExecutor, ProjectAndAssetUidsEarthWorksExecutor
    ///   a) retrieve from cws using serialNumber, response includes DeviceTRN 
    ///   b) get shortRaptorAssetId from localDB so we can add it into response
    ///     note that if it doesn't exist localDB it means that 
    ///     the user hasn't logged in to fill in our DB after adding the device to the account
    ///   c) to get accountId and 2xstatus temporarily needs to call cws.GetAccountsForDevice
    ///   d) only returns 1 ACTIVE account, else error code
    /// </summary>
    [HttpGet("internal/v1/device/serialnumber")]
    public async Task<DeviceDescriptorSingleResult> GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)} serialNumber {serialNumber}");
      
      var deviceSerial = new DeviceSerial(serialNumber);
      deviceSerial.Validate();

      var deviceDataResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetDeviceBySerialExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            headers: customHeaders, cwsDeviceClient: CwsDeviceClient)
          .ProcessAsync(deviceSerial)) as DeviceDescriptorSingleResult;

      return deviceDataResult;
    }

    /// <summary>
    /// Gets a list of projects which a device is associated with.
    ///    Get the list from cws.
    ///  called by TFA ProjectAndAssetUidsExecutor, ProjectAndAssetUidsEarthWorksExecutor
    /// </summary>
    [HttpGet("internal/v1/device/{deviceUid}/projects")]
    public async Task<ProjectDataListResult> GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)} deviceUid: {deviceUid}");

      var deviceIsUid = new DeviceIsUid(deviceUid);
      deviceIsUid.Validate();


      var projectDataListResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetProjectsForDeviceExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            headers: customHeaders, cwsDeviceClient: CwsDeviceClient)
          .ProcessAsync(deviceIsUid)) as ProjectDataListResult;

      return projectDataListResult;
    }
  }
}
