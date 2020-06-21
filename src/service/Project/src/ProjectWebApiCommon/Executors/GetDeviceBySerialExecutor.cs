﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets Device details from a) cws and b) localDB
  /// </summary>
  public class GetDeviceBySerialExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the Get device by serial number from cws. This uses an application token
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deviceSerial = CastRequestObjectTo<DeviceSerial>(item, errorCode: 68);

      try
      {
        var deviceData = new DeviceData();

        var deviceResponseModel = await cwsDeviceClient.GetDeviceBySerialNumber(deviceSerial.SerialNumber, customHeaders);
        if (deviceResponseModel == null)
        {
          var message = "Unable to locate device by serialNumber in cws";
          log.LogInformation($"GetDeviceBySerialExecutor: {message}");
          return new DeviceDescriptorSingleResult(code: 100, message: message, deviceData); 
        }

        deviceData = new DeviceData() {DeviceUID = deviceResponseModel.Id, DeviceName = deviceResponseModel.DeviceName, SerialNumber = deviceResponseModel.SerialNumber};

        // now get the customerId and 2xstatus
        // Note that this step may not be needed in future if/when WM can return these fields in cwsDeviceClient.GetDeviceBySerialNumber() CCSSSCON-28
        // 2020_06_19 GetDeviceBySerialNumber still does not include customerId
        var deviceAccountListDataResult = await cwsDeviceClient.GetAccountsForDevice(new Guid(deviceData.DeviceUID), customHeaders);
        if (deviceAccountListDataResult?.Accounts == null || !deviceAccountListDataResult.Accounts.Any())
        {
          var message = "Unable to locate any account for the device in cws";
          log.LogInformation($"GetDeviceBySerialExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDescriptorSingleResult(code: 102, message: message, deviceData); 
        }

        log.LogInformation($"GetDeviceBySerialExecutor: deviceAccountListDataResult {JsonConvert.SerializeObject(deviceAccountListDataResult)}");
        if (deviceAccountListDataResult.Accounts
          .Count(da => string.Compare(da.RelationStatus.ToString(), RelationStatusEnum.Active.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0) > 1)
        {
          var message = "There is >1 active account for the device in cws";
          log.LogInformation($"GetDeviceBySerialExecutor: {message} deviceData: {JsonConvert.SerializeObject(deviceData)}");
          return new DeviceDescriptorSingleResult(code: 103, message: message, deviceData);
        }

        var deviceCustomer = deviceAccountListDataResult.Accounts
          .FirstOrDefault(da => string.Compare(da.RelationStatus.ToString(), RelationStatusEnum.Active.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0);
        deviceData.CustomerUID = deviceCustomer?.Id;
        deviceData.RelationStatus = deviceCustomer?.RelationStatus ?? RelationStatusEnum.Unknown;
        deviceData.TccDeviceStatus = deviceCustomer?.TccDeviceStatus ?? TCCDeviceStatusEnum.Unknown;
        log.LogInformation($"GetDeviceBySerialExecutor: deviceData {JsonConvert.SerializeObject(deviceData)}");
        return new DeviceDescriptorSingleResult(deviceData); 
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 124, e.Message, e.Message);
      }

      return null;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
