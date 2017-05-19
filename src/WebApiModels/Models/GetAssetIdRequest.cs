﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using WebApiModels.Enums;
using WebApiModels.ResultHandling;

namespace WebApiModels.Models
{
  /// <summary>
  /// The request representation used to request the asset Id and project monitoring subscription for a given machine whose tagfiles
  /// are to be processed. The id of the project into which the tagfile data should be processed. A value of -1 indicates 'unknown' 
  /// which is when the tagfiles are being automatically processed. A value greater than zero is when the project  is known 
  /// which is when a tagfile is being manually imported by a user.
  /// </summary>
  public class GetAssetIdRequest : ContractRequest
  {
    /// <summary>
    /// The id of the project into which the tagfile data should be processed. A value of -1 indicates 'unknown' 
    /// which is when the tagfiles are being automatically processed. A value greater than zero is when the project 
    /// is known which is when a tagfile is being manually imported by a user.
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get; set; }

    /// <summary>
    /// The device type of the machine. Valid values are 0=Manual Device (John Doe machines) and 6=SNM940 (torch machines).
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int deviceType { get; set; }

    /// <summary>
    /// The radio serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string radioSerial { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetAssetIdRequest()
    {
    }

    /// <summary>
    /// Create instance of GetAssetIdRequest
    /// </summary>
    public static GetAssetIdRequest CreateGetAssetIdRequest(long projectId, int deviceType, string radioSerial)
    {
      return new GetAssetIdRequest
      {
        projectId = projectId,
        deviceType = deviceType,
        radioSerial = radioSerial
      };
    }


    /// <summary>
    /// Validates assetID And/or projectID is provided
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrEmpty(radioSerial) && projectId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, ContractExecutionStatesEnum.ValidationError,
            "Must have assetId and/or projectID"));
      }

      // if the number is not in enum then it returns the number
      var isDeviceTypeValid = (((DeviceTypeEnum) deviceType).ToString() != deviceType.ToString());

      if (!string.IsNullOrEmpty(radioSerial) && (deviceType < 1 || !isDeviceTypeValid))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, ContractExecutionStatesEnum.ValidationError,
            "AssetId must have valid deviceType"));
      }

      if (deviceType == 0 && projectId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, ContractExecutionStatesEnum.ValidationError,
            "A manual/unknown deviceType must have a projectID"));
      }

    }
  }
}
