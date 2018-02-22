﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Filter = VSS.Productivity3D.Common.Models.Filter;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// Gets the user id from the current context
    /// </summary>
    /// <value>
    /// The user uid or applicationID as a string.
    /// </value>
    protected string userId => GetUserId();

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    private readonly IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore ConfigStore;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    protected readonly IFileListProxy FileListProxy;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    protected readonly IProjectSettingsProxy ProjectSettingsProxy;

    /// <summary>
    /// For getting list of persistent filters for a project
    /// </summary>
    protected readonly IFilterServiceProxy FilterServiceProxy;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    protected readonly ICompactionSettingsManager SettingsManager;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    /// <value>
    /// The custom headers.
    /// </value>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILogger log, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore, IFileListProxy fileListProxy,
      IProjectSettingsProxy projectSettingsProxy, IFilterServiceProxy filterServiceProxy, ICompactionSettingsManager settingsManager)
    {
      this.log = log;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.ConfigStore = configStore;
      this.FileListProxy = fileListProxy;
      this.ProjectSettingsProxy = projectSettingsProxy;
      this.FilterServiceProxy = filterServiceProxy;
      this.SettingsManager = settingsManager;
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }


    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action.</param>
    /// <returns></returns>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = action.Invoke();
        log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");

      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, ex.Message);
      }
      finally
      {
        log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }

    /// <summary>
    /// Asynch form of WithServiceExceptionTryExecute
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = await action.Invoke();
        log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");

      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, ex.Message);
      }
      finally
      {
        log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns>The project's Id for the Uid provided</returns>
    /// <exception cref="ArgumentException">Incorrect request context principal.</exception>
    protected long GetProjectId(Guid projectUid)
    {
      if (User is RaptorPrincipal principal)
      {
        return principal.GetProjectId(projectUid);
      }

      throw new ArgumentException("Incorrect request context principal.");
    }

    /// <summary>
    /// Gets the ids of the surveyed surfaces to exclude from Raptor calculations. 
    /// This is the deactivated ones.
    /// </summary>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <returns>The list of file ids for the surveyed surfaces to be excluded</returns>
    protected async Task<List<long>> GetExcludedSurveyedSurfaceIds(Guid projectUid)
    {
      var fileList = await this.FileListProxy.GetFiles(projectUid.ToString(), userId, CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return null;
      }

      var results = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => f.LegacyFileId).ToList();

      return results;
    }

    /// <summary>
    /// Gets the <see cref="DesignDescriptor"/> from a given project's fileUid.
    /// </summary>
    protected async Task<DesignDescriptor> GetAndValidateDesignDescriptor(Guid projectUid, Guid? fileUid, bool forProfile = false)
    {
      if (!fileUid.HasValue)
      {
        return null;
      }

      var fileList = await this.FileListProxy.GetFiles(projectUid.ToString(), userId, CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Project has no appropriate design files."));
      }

      FileData file = null;

      foreach (var f in fileList)
      {
        if (f.ImportedFileUid == fileUid.ToString() && f.IsActivated && (!forProfile || f.IsProfileSupportedFileType()))
        {
          file = f;

          break;
        }
      }

      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to access design file."));
      }

      var tccFileName = file.Name;
      if (file.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        //Note: ':' is an invalid character for filenames in Windows so get rid of them
        tccFileName = Path.GetFileNameWithoutExtension(tccFileName) +
                      "_" + file.SurveyedUtc.Value.ToIso8601DateTimeString().Replace(":", string.Empty) +
                      Path.GetExtension(tccFileName);
      }

      string fileSpaceId = FileDescriptor.GetFileSpaceId(this.ConfigStore, this.log);
      FileDescriptor fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, file.Path, tccFileName);

      return DesignDescriptor.CreateDesignDescriptor(file.LegacyFileId, fileDescriptor, 0.0);
    }

    /// <summary>
    /// Gets the project settings for the project.
    /// </summary>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <returns>The project settings</returns>
    protected async Task<CompactionProjectSettings> GetProjectSettings(Guid projectUid)
    {
      CompactionProjectSettings ps;
      var jsonSettings = await this.ProjectSettingsProxy.GetProjectSettings(projectUid.ToString(), userId, CustomHeaders);
      if (jsonSettings != null)
      {
        try
        {
          ps = jsonSettings.ToObject<CompactionProjectSettings>();
          ps.Validate();
        }
        catch (Exception ex)
        {
          log.LogInformation(
            $"JObject conversion to Project Settings or validation failure for projectUid {projectUid}. Error is {ex.Message}");
          ps = CompactionProjectSettings.DefaultSettings;
        }
      }
      else
      {
        log.LogDebug($"No Project Settings for projectUid {projectUid}. Using defaults.");
        ps = CompactionProjectSettings.DefaultSettings;
      }
      return ps;
    }

    /// <summary>
    /// Gets the list of contributing machines from the query parameters
    /// </summary>
    /// <param name="assetId">The asset ID</param>
    /// <param name="machineName">The machine name</param>
    /// <param name="isJohnDoe">The john doe flag</param>
    /// <returns>List of machines</returns>
    protected List<MachineDetails> GetMachines(long? assetId, string machineName, bool? isJohnDoe)
    {
      MachineDetails machine = null;
      if (assetId.HasValue || !string.IsNullOrEmpty(machineName) || isJohnDoe.HasValue)
      {
        if (!assetId.HasValue || string.IsNullOrEmpty(machineName) || !isJohnDoe.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "If using a machine, asset ID machine name and john doe flag must be provided"));
        }
        machine = MachineDetails.CreateMachineDetails(assetId.Value, machineName, isJohnDoe.Value);
      }
      return machine == null ? null : new List<MachineDetails> { machine };
    }

    /// <summary>
    /// Creates an instance of the Filter class and populate it with data.
    /// </summary>
    /// <param name="projectUid">Project Uid</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>An instance of the Filter class.</returns>
    protected async Task<Filter> GetCompactionFilter(Guid projectUid, Guid? filterUid)
    {
      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid);
      bool haveExcludedIds = excludedIds != null && excludedIds.Count > 0;

      DesignDescriptor designDescriptor = null;
      if (filterUid.HasValue)
      {
        try
        {
          var filterData = await GetFilterDescriptor(projectUid, filterUid.Value);
          if (filterData != null)
          {
            if (filterData.DesignUid != null && Guid.TryParse(filterData.DesignUid, out Guid designUidGuid))
            {
              designDescriptor = await GetAndValidateDesignDescriptor(projectUid, designUidGuid);
            }

            if (filterData.HasData() || haveExcludedIds || designDescriptor != null)
            {
              filterData = ApplyDateRange(projectUid, filterData);

              var polygonPoints = filterData.PolygonLL?.ConvertAll(p =>
                Common.Models.WGSPoint.CreatePoint(p.Lat.LatDegreesToRadians(), p.Lon.LonDegreesToRadians()));

              var layerMethod = filterData.LayerNumber.HasValue
                ? FilterLayerMethod.TagfileLayerNumber
                : FilterLayerMethod.None;

              bool? returnEarliest = null;
              if (filterData.ElevationType == ElevationType.First)
              {
                returnEarliest = true;
              }

              return Filter.CreateFilter(null, null, null, filterData.StartUtc, filterData.EndUtc,
                filterData.OnMachineDesignId, null, filterData.VibeStateOn, null, filterData.ElevationType,
                polygonPoints, null, filterData.ForwardDirection, null, null, null, null, null, null,
                layerMethod, null, null, filterData.LayerNumber, null, filterData.ContributingMachines,
                excludedIds, returnEarliest, null, null, null, null, null, designDescriptor);
            }
          }
        }
        catch (ServiceException ex)
        {
          log.LogDebug($"EXCEPTION caught - cannot find filter {ex.Message} {ex.GetContent} {ex.GetResult.Message}" );
          throw;
        }
        catch (Exception ex)
        {
          log.LogDebug("EXCEPTION caught - cannot find filter " + ex.Message);
          throw;
        }
      }

      return haveExcludedIds ? Filter.CreateFilter(excludedIds) : null;
    }

    /// <summary>
    /// Dynamically set the date range according to the date range type.
    /// Custom date range is unaltered. Project extents is always null.
    /// Other types are calculated in the project time zone.
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <param name="filter">The filter containg the date range type</param>
    /// <returns>The filter with the date range set</returns>
    private MasterData.Models.Models.Filter ApplyDateRange(Guid projectUid, MasterData.Models.Models.Filter filter)
    {

      if (!filter.DateRangeType.HasValue || filter.DateRangeType.Value == DateRangeType.Custom)
      {
        log.LogTrace("Filter provided doesn't have dateRangeType set or it is set to Custom. Returning without setting filter start and end dates.");
        return filter;
      }


      var project = (this.User as RaptorPrincipal)?.GetProject(projectUid);
      if (project == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to retrieve project."));
      }

      var utcNow = DateTime.UtcNow;

      //Force daterange filters to be null if ProjectExtents is specified
      DateTime? startUtc=null;
      DateTime? endUtc=null;

      if (filter.DateRangeType.Value != DateRangeType.ProjectExtents)
      {
         startUtc = utcNow.UtcForDateRangeType(filter.DateRangeType.Value, project.ianaTimeZone, true);
         endUtc = utcNow.UtcForDateRangeType(filter.DateRangeType.Value, project.ianaTimeZone, false);
      }

      return MasterData.Models.Models.Filter.CreateFilter(
        startUtc, endUtc, filter.DesignUid, filter.ContributingMachines, filter.OnMachineDesignId, filter.ElevationType,
        filter.VibeStateOn, filter.PolygonLL, filter.ForwardDirection, filter.LayerNumber, filter.PolygonUid, filter.PolygonName);
    }

    public async Task<MasterData.Models.Models.Filter> GetFilterDescriptor(Guid projectUid, Guid filterUid)
    {
      var filterDescriptor = await this.FilterServiceProxy.GetFilter(projectUid.ToString(), filterUid.ToString(), this.CustomHeaders);

      return filterDescriptor == null
        ? null
        : JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterDescriptor.FilterJson);
    }

    /// <summary>
    /// Gets the summary volumes parameters according to the calcultion type
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="volumeCalcType">The summary volumes calculation type</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <returns>Tuple of base filter, top filter and volume design descriptor</returns>
    protected async Task<Tuple<Filter, Filter, DesignDescriptor>> GetSummaryVolumesParameters(Guid projectUid, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid)
    {
      Filter baseFilter = null;
      Filter topFilter = null;
      DesignDescriptor volumeDesign = null;
      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.GroundToGround:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
          case VolumeCalcType.GroundToDesign:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeTopUid, true);
            break;
          case VolumeCalcType.DesignToGround:
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeBaseUid, true);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
        }
      }
      return new Tuple<Filter, Filter, DesignDescriptor>(baseFilter, topFilter, volumeDesign);
    }

  
  }
}