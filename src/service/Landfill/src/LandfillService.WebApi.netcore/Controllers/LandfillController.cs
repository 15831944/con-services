﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.netstandard.ApiClients;
using Common.Repository;
using LandfillService.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LandfillService.WebApi.netcore.Controllers
{
  /// <summary>
  ///   Handles project related requests
  /// </summary>
  public class ProjectsController : Controller
  {
    private readonly ILogger Log;
    private IConfigurationStore config;
    private IRaptorProxy raptorProxy;
    private IFileListProxy files;


    public ProjectsController(ILogger<ProjectsController> logger, IConfigurationStore config, IRaptorProxy raptorProxy, IFileListProxy files )
    {
      Log = logger;
      this.raptorProxy = raptorProxy;
      this.files = files;
      this.config = config;
    }

    #region Projects

    /// <summary>
    ///   Retrieves a list of projects from the db
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="customerUid">User ID</param>
    /// <returns>A list of projects or error details</returns>
    private IEnumerable<Project> PerhapsUpdateProjectList(string userUid,string customerUid)
    {
      Log.LogDebug("PerhapsUpdateProjectList: user: " + userUid + " customer: " + customerUid);
      try
      {
        return LandfillDb.GetProjects(userUid, customerUid);
      }
      catch ( Exception e)
      {
        Log.LogError(e, $"PerhapsUpdateProjectList failed with exception. userUid: {userUid} customerUid: {customerUid}");
        throw;
      }
    }


    private bool IfProjectAuthorized(string userUid, string customerUid, long projectId)
    {
      var projects = PerhapsUpdateProjectList(userUid, customerUid);
      if (projects != null && projects.Any(project => project.id == projectId))
        return true;
      throw new ServiceException(HttpStatusCode.Forbidden, new ContractExecutionResult());
    }

    /// <summary>
    ///   Returns the list of projects available to the user
    /// </summary>
    /// <returns>List of available projects</returns>
    [Route("api/v2/projects")]
    public object Get()
    {
      var principal = HttpContext.User as TIDCustomPrincipal;
      Log.LogDebug("GetProjects: user: " + principal.Identity.Name + " customer: " + principal.CustomerUid);
      return PerhapsUpdateProjectList(principal.Identity.Name, principal.CustomerUid);
    }

    /// <summary>
    ///   TEST CODE: generate random project data entries
    /// </summary>
    /// <returns>Random project data entries</returns>
    private IEnumerable<DayEntry> GetRandomEntries()
    {
      var totalDays = 730;
      var startDate = DateTime.Today.AddDays(-totalDays);

      var entries = new List<DayEntry>();

      var rnd = new Random();

      var densityExtra = rnd.Next(1, 3);
      var weightExtra = rnd.Next(200, 300);


      foreach (var i in Enumerable.Range(0, totalDays))
      {
        var skip = i < 728 && rnd.Next(5) % 6 == 0;

        double density = skip ? 0 : rnd.Next(1200 / densityExtra, 1600 / densityExtra);
        double weight = skip ? 0 : rnd.Next(500, 800 + weightExtra);
        entries.Add(new DayEntry
        {
          date = DateTime.Today.AddDays(-totalDays + i),
          entryPresent = !skip,
          weight = weight,
          volume = skip ? 0 : weight * 1000 / density
        });
      }

      return entries.ToArray();
    }

    /// <summary>
    ///   Returns the project data for the given project. If geofenceUid is not specified,
    ///   data for the entire project area is returned otherwise data for the geofenced area is returned.
    ///   If no date range specified, returns data for the last 2 years to today in the project time zone
    ///   otherwise returns data for the specified date range.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="startDate">Start date in project time zone for which to return data</param>
    /// <param name="endDate">End date in project time zone for which to return data</param>
    /// <returns>List of data entries for each day in date range and the status of volume retrieval for the project</returns>
    [Route("api/v2/projects/{id}")]
    public object Get(uint id, Guid? geofenceUid = null, DateTime? startDate = null,DateTime? endDate = null)
    {
      // Get the available data
      // Kick off missing volumes retrieval IF not already running
      // Check if there are missing volumes and indicate to the client
      var principal = HttpContext.User as TIDCustomPrincipal;
      var jwt = HttpContext.Request.Headers["X-Jwt-Assertion"];
      Log.LogDebug("X-Jwt-Assertion=" + jwt);

      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);
      Log.LogDebug("GetEntriesForProject: user: " + principal.Identity.Name + " customer: " + principal.CustomerUid + " Project:" + id);
      var projects = PerhapsUpdateProjectList(principal.Identity.Name, principal.CustomerUid);
      {
        try
        {
          var project = projects.First(p => p.id == id);
          //  GetMissingVolumesInBackground(userUid, project);  // retry volume requests which weren't successful before
          var entries = new ProjectData
          {
            Project = project,
            entries = LandfillDb.GetEntries(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, startDate,
              endDate),
            retrievingVolumes = false //  todo LandfillDb.RetrievalInProgress(project)
          };

          return Ok(entries);
          // TEST CODE: use this to test chart updates on the client
          //return Ok(new ProjectData { entries = GetRandomEntries(), retrievingVolumes = true });
        }
        catch (InvalidOperationException)
        {
          return Ok();
        }
      }
    }

    #endregion

    #region Weights

    /// <summary>
    ///   Returns the weights for all geofences for the project for the date range
    ///   of the last 2 years to today in the project time zone.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>List of entries for each day in date range and the weight for each geofence for that day</returns>
    [Route("api/v2/projects/{id}/weights")]
    public object GetWeights(uint id)
    {
      var principal = HttpContext.User as TIDCustomPrincipal;

      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);

      Log.LogDebug("GetWeights: user: " + principal.Identity.Name + " customer: " + principal.CustomerUid + " Project:" + id);
      var projects = PerhapsUpdateProjectList(principal.Identity.Name, principal.CustomerUid);
      {
        try
        {
          var project = projects.Where(p => p.id == id).First();

          var data = new WeightData
          {
            Project = project,
            entries = GetGeofenceWeights(project),
            retrievingVolumes = false // todo LandfillDb.RetrievalInProgress(project)
          };

          return Ok(data);
        }
        catch (InvalidOperationException)
        {
          return Ok();
        }
      }
    }

    /// <summary>
    ///   Retrieves weights entries for each geofence in the project.
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>List of dates with the weight for each geofence for that date</returns>
    private List<GeofenceWeightEntry> GetGeofenceWeights(Project project)
    {
      var weights = new Dictionary<DateTime, List<GeofenceWeight>>();
      var geofenceUids = LandfillDb.GetGeofences(project.projectUid).Select(g => g.uid);
      foreach (var geofenceUid in geofenceUids)
      {
        var entries = LandfillDb.GetEntries(project, geofenceUid.ToString(), null, null);
        foreach (var entry in entries)
        {
          if (!weights.ContainsKey(entry.date))
            weights.Add(entry.date, new List<GeofenceWeight>());
          weights[entry.date].Add(new GeofenceWeight {geofenceUid = geofenceUid, weight = entry.weight});
        }
      }

      return (from w in weights
        select new GeofenceWeightEntry
        {
          date = w.Key,
          entryPresent = w.Value.Any(v => v.weight != 0),
          geofenceWeights = w.Value
        }).ToList();
    }

    /// <summary>
    ///   Saves weights submitted in the request.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <returns>Project data and status of volume retrieval</returns>
    [HttpPost]
    [Route("api/v2/projects/{id}/weights")]
    public object PostWeights(uint id, [FromBody] WeightEntry[] entries, Guid? geofenceUid = null)
    {
      //When the request goes through TPaaS the headers get changed to Transfer-Encoding: chunked and the Content-Length is 0.
      //For some reason the Web API framework can't handle this and doesn't deserialize the 'entries'.
      //So we do it manually here. Note this problem only occurs when URI and body contain parameters.
      //See http://w3foverflow.com/question/asp-net-web-api-the-framework-is-not-converting-json-to-object-when-using-chunked-transfer-encoding/
      //If you hit the Landfill service directly it all works.
      //string jsonContent = Request.Content.ReadAsStringAsync().Result; //this gets proper JSON
      //LoggerSvc.LogMessage(null, null, null, "PostWeights: id=" + id + ", request content=" + jsonContent);          
      //var entries = JsonConvert.DeserializeObject<WeightEntry[]>(jsonContent);

      var principal = HttpContext.User as TIDCustomPrincipal;
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);


      if (entries == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Missing weight entries"));

      if (!geofenceUid.HasValue || geofenceUid == Guid.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Missing geofence UID"));

      var geofenceUidStr = geofenceUid.Value.ToString();

      Log.LogDebug("PostWeights: user: " + principal.Identity.Name + " customer " + principal.CustomerUid); 

      var projects = PerhapsUpdateProjectList(principal.Identity.Name, principal.CustomerUid);
      {
        var project = projects.Where(p => p.id == id).First();

        var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];
        var utcNow = DateTime.UtcNow;
        var projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
        //Console.WriteLine("projTimeZoneOffsetFromUtc=" + projTimeZoneOffsetFromUtc);
        var yesterdayInProjTimeZone = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).AddDays(-1);

        //Console.WriteLine("yesterdayInProjTimeZone=" + yesterdayInProjTimeZone);

        var validEntries = new List<DateEntry>();
        foreach (var entry in entries)
        {
          var valid = entry.weight >= 0 && entry.date.Date <= yesterdayInProjTimeZone.Date;
          Debug.WriteLine(entry + "--->" + valid);

          if (valid)
          {
            LandfillDb.SaveEntry(project, geofenceUidStr, entry);
            validEntries.Add(new DateEntry {date = entry.date, geofenceUid = geofenceUidStr});
          }
        }

        Console.WriteLine("Finished posting weights");

        return Ok(new WeightData
        {
          Project = project,
          entries = GetGeofenceWeights(project),
          retrievingVolumes = true
        });
      }
    }

    #endregion

    #region Volumes

    /// <summary>
    ///   Gets volume and time summary for a landfill project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Current week volume, current month volume, remaining volume (air space) and time remaining (days)</returns>
    [Route("api/v2/projects/{id}/volumeTime")]
    public async Task<object> GetVolumeTimeSummary(uint id)
    {
      var principal = HttpContext.User as TIDCustomPrincipal;
      //Secure with project list
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);
      Log.LogDebug("volumeTime: user: " + principal.Identity.Name + " customer: " + principal.CustomerUid + " Project:" + id);
      try
      {
        var projects = LandfillDb.GetProjects(principal.Identity.Name, principal.CustomerUid);
        var project = projects.First(p => p.id == id);
        var todayinProjTimeZone = LandfillDb.GetTodayInProjectTimeZone(project.timeZoneName);
        //Console.WriteLine("Volumes todayinProjTimeZone:" + todayinProjTimeZone);
        var startWeek = CurrentWeekMonday(todayinProjTimeZone);
        var weekVol = LandfillDb.GetEntries(project, null, startWeek, todayinProjTimeZone).Sum(e => e.volume);
        var startMonth = new DateTime(todayinProjTimeZone.Year, todayinProjTimeZone.Month, 1);
        var monthVol = LandfillDb.GetEntries(project, null, startMonth, todayinProjTimeZone).Sum(e => e.volume);
        Log.LogDebug("Volumes week:" + weekVol);
        Log.LogDebug("Volumes month:" + monthVol);

        //Get designIds from ProjectMonitoring service
        var raptorApiClient = new RaptorApiClient(Log, config, raptorProxy, files, Request.Headers.GetCustomHeaders());
        var res = await raptorApiClient.GetDesignID(Request.Headers["X-Jwt-Assertion"], project, principal.CustomerUid);
        var designId = res.Where(r => r.name == "TOW.ttm").Select(i => i.id).First();
        Log.LogDebug("Volumes designId for TOW.ttm:" + designId);
        var firstAirspaceVol = await GetAirspaceVolumeInBackground(principal.Identity.Name, project, true, designId);
        var lastAirspaceVol = await GetAirspaceVolumeInBackground(principal.Identity.Name, project, false, designId);
        var statsDates = await GetProjectStatisticsInBackground(principal.Identity.Name, project);
        var dates = statsDates.ToList();
        var volPerDay = firstAirspaceVol.HasValue && lastAirspaceVol.HasValue
          ? Math.Abs(firstAirspaceVol.Value - lastAirspaceVol.Value) /
            Math.Abs((dates[0] - dates[1]).TotalDays)
          : (double?) null;
        var timeLeft = volPerDay.HasValue ? lastAirspaceVol / volPerDay.Value : null;
        return Ok(new VolumeTime
        {
          currentWeekVolume = weekVol,
          currentMonthVolume = monthVol,
          remainingVolume = lastAirspaceVol,
          remainingTime = timeLeft
        });
      }
      catch (Exception e) 
      {
        Log.LogError(e, $"GetVolumeTimeSummary failed with exception. Id: {id}");
        return Ok();
      }
    }

    /// <summary>
    ///   Returns the date representing the Monday of the week the given date falls in.
    /// </summary>
    /// <param name="date">The date for which to find the start of the week</param>
    /// <returns>The date of the Monday of that week</returns>
    private static DateTime CurrentWeekMonday(DateTime date)
    {
      var daysToSubtract = 0;
      switch (date.DayOfWeek)
      {
        case DayOfWeek.Sunday:
          daysToSubtract = 6;
          break;
        case DayOfWeek.Saturday:
          daysToSubtract = 5;
          break;
        case DayOfWeek.Friday:
          daysToSubtract = 4;
          break;
        case DayOfWeek.Thursday:
          daysToSubtract = 3;
          break;
        case DayOfWeek.Wednesday:
          daysToSubtract = 2;
          break;
        case DayOfWeek.Tuesday:
          daysToSubtract = 1;
          break;
        case DayOfWeek.Monday:
          daysToSubtract = 0;
          break;
      }

      return date.AddDays(-daysToSubtract);
    }

    /// <summary>
    ///   Retrieves airspace volume summary from Raptor.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project</param>
    /// <param name="returnEarliest">Indicates if filtering by earliest or latest cell pass</param>
    /// <param name="designId"></param>
    /// <returns></returns>
    private async Task<double?> GetAirspaceVolumeInBackground(string userUid, Project project,
      bool returnEarliest,
      int designId)
    {
      try
      {
        var raptorApiClient = new RaptorApiClient(Log, config, raptorProxy, files, Request.Headers.GetCustomHeaders());
        var res = await raptorApiClient.GetAirspaceVolumeAsync(userUid, project, returnEarliest, designId);
        Log.LogDebug("Airspace Volume res:" + res);
        Log.LogDebug("Airspace Volume: " + res.Fill);

        return res.Fill;
      }
      catch (RaptorApiException e)
      {
        if (e.code == HttpStatusCode.BadRequest)
          return null;
        throw;
      }
      catch (Exception e)
      {
        Log.LogDebug("Exception while retrieving airspace volume: " + e);
        throw;
      }
    }

    /// <summary>
    ///   Retrieves project statistics from Raptor.
    /// </summary>
    /// <param name="userUid">User ID</param>
    /// <param name="project">Project</param>
    /// <returns>Date extents from project statistics</returns>
    private async Task<IEnumerable<DateTime>> GetProjectStatisticsInBackground(string userUid,
      Project project)
    {
      try
      {
        var raptorApiClient = new RaptorApiClient(Log, config, raptorProxy, files, Request.Headers.GetCustomHeaders());

        var res = await raptorApiClient.GetProjectStatisticsAsync(userUid, project);
        Log.LogDebug("Statistics dates: " + res.startTime + " - " + res.endTime);
        return new List<DateTime> {res.startTime, res.endTime};
      }
      catch (Exception e)
      {
        Log.LogDebug("Exception while retrieving statistics: " + e);
        throw;
      }
    }

    #endregion

    #region Geofences

    /// <summary>
    ///   Returns a list of geofences for the project. A geofence is associated with a project if its
    ///   boundary is inside or intersects that of the project and it is of type 'Landfill'. The project
    ///   geofence is also returned.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>List of geofences with their bounding boxes</returns>
    [Route("api/v2/projects/{id}/geofences")]
    public object GetGeofences(uint id)
    {
      var principal = HttpContext.User as TIDCustomPrincipal;
      //Secure with project list
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);

      try
      {
        var project = LandfillDb.GetProjects(principal.Identity.Name, principal.CustomerUid)
          .First(p => p.id == id);
        var geofences = LandfillDb.GetGeofences(project.projectUid);
        return Ok(geofences);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"GetGeofences failed with exception. Id: {id}");
        return Ok();
      }
    }

    /// <summary>
    ///   Returns a geofence boundary.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <returns>List of WGS84 boundary points in radians</returns>
    [Route("api/v2/projects/{id}/geofences/{geofenceUid}")]
    public object GetGeofenceBoundary(uint id, Guid geofenceUid)
    {
      var principal = HttpContext.User as TIDCustomPrincipal;
      //Secure with project list
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);

      var geofenceUidStr = geofenceUid.ToString();
      Log.LogDebug("Geofences: user:" + principal.Identity.Name + " customer:" + principal.CustomerUid + " geofence:" + geofenceUidStr);
      try
      {
        var points = LandfillDb.GetGeofencePoints(geofenceUidStr);
        return Ok(points);
      }
      catch (InvalidOperationException)
      {
        return Ok();
      }
    }

    private Dictionary<string, List<WGSPoint>> GetGeofenceBoundaries(uint id, List<string> geofenceUids)
    {
      var geofences = geofenceUids.ToDictionary(g => g,
        g => LandfillDb.GetGeofencePoints(g).ToList());

      return geofences;
    }

    #endregion

    #region CCA

    /// <summary>
    ///   Gets CCA ratio data on a daily basis for a landfill project for all machines. If geofenceUid is not
    ///   specified,
    ///   CCA ratio data for the entire project area is returned otherwise CCA ratio data for the geofenced area is
    ///   returned.
    ///   If no date range specified, returns CCA ratio data for the last 2 years to today in the project time zone
    ///   otherwise returns CCA ratio data for the specified date range.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="startDate">Start date in project time zone for which to return data</param>
    /// <param name="endDate">End date in project time zone for which to return data</param>
    /// <returns>List of machines and daily CCA ratio</returns>
    [Route("api/v2/projects/{id}/ccaratio")]
    public object GetCCARatio(uint id, Guid? geofenceUid = null, DateTime? startDate = null,DateTime? endDate = null)
    {
      var principal = HttpContext.User as TIDCustomPrincipal;
      //Secure with project list
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);

      try
      {
        var project = LandfillDb.GetProjects(principal.Identity.Name, principal.CustomerUid).First(p => p.id == id);
        var ccaData = LandfillDb.GetCCA(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, startDate,
          endDate, null, null);
        var groupedData = ccaData.GroupBy(c => c.machineId).ToDictionary(k => k.Key, v => v.ToList());
        var machines = groupedData.ToDictionary(k => k.Key, v => LandfillDb.GetMachine(v.Key));
        var data = groupedData.Select(d => new CCARatioData
        {
          machineName = machines[d.Key] == null ? "Unknown" : machines[d.Key].machineName,
          entries = groupedData[d.Key].Select(v => new CCARatioEntry
          {
            date = v.date,
            ccaRatio = Math.Round(v.complete + v.overcomplete)
          }).ToList()
        }).ToList();
        return Ok(data);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"GetCCARatio failed with exception. Id: {id}");
        return Ok();
      }
    }

    /// <summary>
    ///   Gets CCA summary data for a landfill project for the specified date. If geofenceUid is not specified,
    ///   CCA summary data for the entire project area is returned otherwise CCA data for the geofenced area is
    ///   returned.
    ///   If machine (asset ID, machine name and John Doe flag) is not specified, returns data for all machines
    ///   else for the specified machine. If lift ID is not specified returns data for all lifts else for the specified lift.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="date">Date in project time zone for which to return data</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="assetId">Asset ID (from MachineDetails)</param>
    /// <param name="assetUid">Asset UID (from MachineDetails)</param>
    /// <param name="machineName">Machine name (from MachineDetails)</param>
    /// <param name="isJohnDoe">IsJohnDoe flag (from MachineDetails)</param>
    /// <param name="liftId">Lift/Layer ID</param>
    /// <returns>CCA summary for the date</returns>
    [Route("api/v2/projects/{id}/ccasummary")]
    public object GetCCASummary(uint id, DateTime? date, Guid? geofenceUid = null,
      uint? assetId = null, Guid? assetUid = null, string machineName = null, bool? isJohnDoe = null, int? liftId = null)
    {
      //NOTE: CCA summary is not cumulative. 
      //If data for more than one day is required, client must call Raptor service directly

      var principal = HttpContext.User as TIDCustomPrincipal;
      //Secure with project list
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);

      if (!date.HasValue)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing date"));

      var gotMachine = (assetId.HasValue || assetUid.HasValue) && isJohnDoe.HasValue && !string.IsNullOrEmpty(machineName);
      var noMachine = !assetId.HasValue && !assetUid.HasValue && !isJohnDoe.HasValue && string.IsNullOrEmpty(machineName);
      if (!gotMachine && !noMachine)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Either all or none of the machine details parameters must be provided"));

      try
      {
        var project = LandfillDb.GetProjects(principal.Identity.Name, principal.CustomerUid).First(p => p.id == id);
        var machineId = noMachine
          ? (long?) null
          : LandfillDb.GetMachineId(project.projectUid,
            new MachineDetails
            {
              assetId = assetId ?? 0,
              assetUid = assetUid,
              machineName = machineName,
              isJohnDoe = isJohnDoe.Value
            });

        if (gotMachine && machineId.Value == 0)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Machine does not exist"));

        var ccaData = LandfillDb.GetCCA(project, geofenceUid.HasValue ? geofenceUid.ToString() : null, date, date,
          machineId, liftId);
        var groupedData = ccaData.GroupBy(c => c.machineId).ToDictionary(k => k.Key, v => v.ToList());
        var machines = groupedData.ToDictionary(k => k.Key, v => LandfillDb.GetMachine(v.Key));

        var data = ccaData.Select(d => new CCASummaryData
        {
          machineName = machines[d.machineId] == null ? "Unknown" : machines[d.machineId].machineName,
          liftId = d.liftId,
          incomplete = Math.Round(d.incomplete),
          complete = Math.Round(d.complete),
          overcomplete = Math.Round(d.overcomplete)
        }).ToList();

        Log.LogDebug("ccasummary: " + project.name);
        return Ok(data);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"GetCCASummary failed with exception. Id: {id}");
        return Ok();
      }
    }

    /// <summary>
    ///   Gets a list of machines and lifts for a landfill project. If no date range specified,
    ///   the last 2 years to today in the project time zone is used.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="startDate">Start date in project time zone for which to return data</param>
    /// <param name="endDate">End date in project time zone for which to return data</param>
    /// <returns>List of machines and lifts in project time zone</returns>
    [Route("api/v2/projects/{id}/machinelifts")]
    public async Task<object> GetMachineLifts(uint id, DateTime? startDate = null, DateTime? endDate = null)
    {
      var principal = HttpContext.User as TIDCustomPrincipal;
      //Secure with project list
      IfProjectAuthorized(principal.Identity.Name, principal.CustomerUid, id);

      try
      {
        var project = LandfillDb.GetProjects(principal.Identity.Name, principal.CustomerUid).First(p => p.id == id);
        //    var project = LandfillDb.GetProject(id).First(); 
        var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];

        var utcNow = DateTime.UtcNow;
        var projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
        if (!endDate.HasValue)
          endDate = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date; //today in project time zone
        if (!startDate.HasValue)
          startDate = endDate.Value.AddYears(-2);
        var raptorApiClient = new RaptorApiClient(Log, config, raptorProxy, files, Request.Headers.GetCustomHeaders());
        var task = await raptorApiClient.GetMachineLiftsInBackground(null, project, startDate.Value, endDate.Value);
        Log.LogDebug("machinelifts: " + project.name);

        return Ok(task);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"GetMachineLifts failed with exception. Id: {id}");
        return Ok();
      }
    }

    #endregion
  }
}
