﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using App.Metrics.Health.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.Models.ContractExecutionStatesEnum;
using CoordinateSystemFileValidationRequest = VSS.MasterData.Models.Models.CoordinateSystemFileValidationRequest;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// Represents abstract container for all request executors.
  /// Uses abstract factory pattern to separate executor logic from
  ///   controller logic for testability and possible executor version.
  /// </summary>
  public class DataRepository : IDataRepository
  {
    private ILogger _log;

    private readonly ICwsAccountClient _cwsAccountClient;

    // We need to use ProjectSvc IProjectProxy as that's where the project data is
    private readonly IProjectInternalProxy _projectProxy;

    // We need to use ProjectSvc IDeviceProxy 
    //    as when we get devices from IDeviceClient, 
    //    we need to write them into ProjectSvc local db to generate the shortRaptorAssetId
    private readonly IDeviceInternalProxy _deviceProxy;

    // convert NE to LL using the projects CSIB via TRex
    private readonly ITRexCompactionDataProxy _tRexCompactionDataProxy;

    private IHeaderDictionary _mergedCustomHeaders;

    public DataRepository(ILogger log, ITPaaSApplicationAuthentication authorization, ICwsAccountClient cwsAccountClient, IProjectInternalProxy projectProxy, IDeviceInternalProxy deviceProxy, ITRexCompactionDataProxy tRexCompactionDataProxy,
      IHeaderDictionary requestCustomHeaders)
    {
      _log = log;
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;
      _tRexCompactionDataProxy = tRexCompactionDataProxy;
      _mergedCustomHeaders = requestCustomHeaders;

      foreach (var header in authorization.CustomHeaders())
      {
        _mergedCustomHeaders.Add(header);
      }
    }


    #region account
    /// <summary>
    /// We could use the ProjectSvc ICustomerProxy to then call IAccountClient. For now, just go straight to client.
    /// </summary>
    [Obsolete("Not used at present. As per SP, leave in case needed in future")]
    public async Task<int> GetDeviceLicenses(string customerUid)
    {
      if (string.IsNullOrEmpty(customerUid))
        return 0;

      try
      {
        return (await _cwsAccountClient.GetDeviceLicenses(new Guid(customerUid), _mergedCustomHeaders))?.Total ?? 0;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "cwsAccount", e.Message));
      }
    }

    #endregion account


    #region project
    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        return null;
      try
      {
        return await _projectProxy.GetProject(projectUid, _mergedCustomHeaders);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }
    }

    // manual import, no time, optional device
    public async Task<ProjectDataResult> GetIntersectingProjectsForManual(ProjectData project, double latitude, double longitude,
      DeviceData device = null, double? northing = null, double? easting = null)
    {
      var accountProjects = new ProjectDataResult();
      if (project == null || string.IsNullOrEmpty(project.ProjectUID))
        return accountProjects;

      try
      {
        accountProjects = (await _projectProxy.GetIntersectingProjects(project.CustomerUID, latitude, longitude, project.ProjectUID, northing, easting, _mergedCustomHeaders));
        // should not be possible to get > 1 as call was limited by the projectUid       
        if (accountProjects?.Code == 0 && accountProjects.ProjectDescriptors.Count() != 1)
          return accountProjects;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }

      if (device == null || string.IsNullOrEmpty(device.DeviceUID))
        return accountProjects;

      // what are the marketing requirements here e.g. restrict to projects which the device is active in
      // what projects does this device have visibility to?
      try
      {
        var projectsAssociatedWithDevice = (await _deviceProxy.GetProjectsForDevice(device.DeviceUID, _mergedCustomHeaders));
        if (projectsAssociatedWithDevice?.Code == 0 && projectsAssociatedWithDevice.ProjectDescriptors.Any())
        {
          var result = new ProjectDataResult();
          var gotIt = projectsAssociatedWithDevice.ProjectDescriptors.FirstOrDefault(p => p.ProjectUID == accountProjects.ProjectDescriptors[0].ProjectUID);
          result.ProjectDescriptors.Add(gotIt);
          return result;
        }

        return accountProjects;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    ///
    /// the difference to GetIntersectingProjectsForManual() is that device is required
    ///
    public ProjectDataResult GetIntersectingProjectsForDevice(DeviceData device,
      double latitude, double longitude, double? northing, double? easting, out int errorCode)
    {
      errorCode = 0;
      var accountProjects = new ProjectDataResult();
      if (device == null || string.IsNullOrEmpty(device.CustomerUID) || string.IsNullOrEmpty(device.DeviceUID))
        return accountProjects;

      // what projects does this customer have which intersect the lat/long?
      try
      {
        accountProjects = _projectProxy.GetIntersectingProjects(device.CustomerUID, latitude, longitude, null, northing, easting, customHeaders: _mergedCustomHeaders).Result;
        if (accountProjects?.Code != 0 || !accountProjects.ProjectDescriptors.Any())
        {
          errorCode = 44;
          return accountProjects;
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }

      // what are the marketing requirements here e.g. restrict to projects which the device is active in?
      // what projects does this device have visibility to?
      try
      {
        var intersectingProjectsForDevice = new ProjectDataResult();
        var projectsAssociatedWithDevice = _deviceProxy.GetProjectsForDevice(device.DeviceUID, _mergedCustomHeaders).Result;
        if (projectsAssociatedWithDevice?.Code == 0 && projectsAssociatedWithDevice.ProjectDescriptors.Any())
        {
          var intersection = projectsAssociatedWithDevice.ProjectDescriptors.Select(dp => dp.ProjectUID).Intersect(accountProjects.ProjectDescriptors.Select(ap => ap.ProjectUID));
          intersectingProjectsForDevice.ProjectDescriptors = projectsAssociatedWithDevice.ProjectDescriptors.Where(p => intersection.Contains(p.ProjectUID)).ToList();
        }

        if (!intersectingProjectsForDevice.ProjectDescriptors.Any())
          errorCode = 45;
        return intersectingProjectsForDevice;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
          ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    #endregion project


    #region device

    // Need to get cws: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    public async Task<DeviceData> GetDevice(string serialNumber)
    {
      if (string.IsNullOrEmpty(serialNumber))
        return null;
      try
      {
        return await _deviceProxy.GetDevice(serialNumber, _mergedCustomHeaders);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    #endregion device

    #region TRex
    public Task<CoordinateConversionResult> ConvertNEtoLL(CoordinateConversionRequest request)
    {
      return _tRexCompactionDataProxy.SendDataPostRequest<CoordinateConversionResult, CoordinateConversionRequest>(request, "/coordinateconversion", _mergedCustomHeaders);
    }
    #endregion TRex
  }
}
