﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Extensions;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.TCCFileAccess;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public partial class ProjectRequestHelper
  {

    /// <summary>
    /// Gets a Project list for customer uid.
    ///  Includes all projects, regardless of archived state and user role
    /// </summary>
    public static async Task<List<ProjectDatabaseModel>> GetProjectListForCustomer(Guid customerUid, Guid userUid,
    ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      log.LogDebug($"{nameof(GetProjectListForCustomer)} customerUid {customerUid}, userUid {userUid}");
      var projects = await cwsProjectClient.GetProjectsForCustomer(customerUid, userUid, customHeaders);

      var projectDatabaseModelList = new List<ProjectDatabaseModel>();
      foreach (var project in projects.Projects)
      {
        var projectDatabaseModel = ConvertCwsToWorksOSProject(project, log);
        if (projectDatabaseModel != null)
          projectDatabaseModelList.Add(projectDatabaseModel);
      }
      log.LogDebug($"{nameof(GetProjectListForCustomer)} Project list contains {projectDatabaseModelList.Count} projects");
      return projectDatabaseModelList;
    }

    /// <summary>
    /// Calibration file is optional for nonThreeDReady projects
    /// cws Filename format is: "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
    /// </summary>
    public static bool ExtractCalibrationFileDetails(List<ProjectConfiguration> projectConfigurations, out string fileName, out DateTime? fileDateUtc)
    {
      fileName = string.Empty;
      fileDateUtc = null;

      var projectConfiguration = projectConfigurations?.FirstOrDefault(c => c.FileType == ProjectConfigurationFileType.CALIBRATION.ToString());
      if (projectConfiguration == null)
        return false;
      var parts = projectConfiguration.FileName.Split(ProjectConfiguration.FilenamePathSeparator);
      if (parts.Length == 3)
      {
        fileName = parts[2].Trim();
        var acceptedFileExtensions = new AcceptedFileExtensions();
        if ((!acceptedFileExtensions.IsExtensionAllowed(new List<string> { "dc", "cal" }, fileName))
           || (!DateTime.TryParse(parts[1], out var fileDate)))
          return false;

        fileDateUtc = fileDate;
        return true;
      }

      return false;
    }

    public static ProjectDatabaseModel ConvertCwsToWorksOSProject(ProjectDetailResponseModel project, ILogger log)
    {
      var extractedCalibrationFileOk = false;
      var coordinateSystemFileName = string.Empty;
      DateTime? coordinateSystemLastActionedUtc = null;
      if (project.ProjectSettings?.Config?.ProjectConfigurations != null)
         extractedCalibrationFileOk = ExtractCalibrationFileDetails(project.ProjectSettings.Config.ProjectConfigurations, out coordinateSystemFileName, out coordinateSystemLastActionedUtc);
      if (project.ProjectSettings?.Boundary == null || project.ProjectSettings?.TimeZone == null)
        log.LogInformation($"{nameof(ConvertCwsToWorksOSProject)} no boundary or timezone available {project}");

      if (!extractedCalibrationFileOk)
      {
        //if (project.ProjectType == ProjectTypeEnum.ThreeDEnabled)
        //  log.LogError(@"{nameof(ConvertCwsToWorksOSProject)} unable to extract calibrationFile {project.ProjectSettings.Config.ProjectConfigurations}");
        //else
        log.LogInformation($"{nameof(ConvertCwsToWorksOSProject)} calibrationFile not available {project.ProjectSettings?.Config?.ProjectConfigurations}");
      }

      var projectDatabaseModel =
        new ProjectDatabaseModel() 
        {
          ProjectUID = project.ProjectId,
          CustomerUID = project.AccountId,
          Name = project.ProjectName,
          ProjectType = ProjectType.Standard,
          ProjectTimeZone = project.ProjectSettings != null ? PreferencesTimeZones.IanaToWindows(project.ProjectSettings.TimeZone) : string.Empty,
          ProjectTimeZoneIana = project.ProjectSettings?.TimeZone,
          Boundary = project.ProjectSettings?.Boundary != null ? RepositoryHelper.ProjectBoundaryToWKT(project.ProjectSettings.Boundary) : string.Empty,
          CoordinateSystemFileName = coordinateSystemFileName,
          CoordinateSystemLastActionedUTC = coordinateSystemLastActionedUtc,
          IsArchived = false, 
          LastActionedUTC = project.LastUpdate
        };
      return projectDatabaseModel;
    }

    /// <summary>
    /// Gets a Project by customer uid.
    /// </summary>
    public static async Task<ProjectDatabaseModel> GetProject(string projectUid, string customerUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var project =
        (await projectRepo.GetProjectsForCustomer(customerUid)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

      if (project == null)
      {
        log.LogWarning($"Customer doesn't have access to projectUid: {projectUid}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      log.LogInformation($"Project projectUid: {projectUid} retrieved");
      return project;
    }

    /// <summary>
    /// Gets a Project, even if archived
    /// </summary>
    public static async Task<ProjectDatabaseModel> GetProjectOnly(string projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var project = (await projectRepo.GetProjectOnly(projectUid));

      if (project == null)
      {
        log.LogWarning($"Unable to locate projectUid: {projectUid}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      log.LogInformation($"Project projectUid: {projectUid} retrieved");
      return project;
    }

    /// <summary>
    /// Gets a Project, even if archived, return project even if null
    /// </summary>
    public static async Task<ProjectDatabaseModel> GetProjectEvenIfArchived(string projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var project = (await projectRepo.GetProjectOnly(projectUid));

      log.LogInformation($"Project projectUid: {projectUid} project {project} retrieved");
      return project;
    }

    /// <summary>
    /// Gets a Project NO customer uid.
    /// </summary>
    public static async Task<ProjectDatabaseModel> GetProject(long shortRaptorProjectId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var project = (await projectRepo.GetProject(shortRaptorProjectId));

      log.LogInformation($"Project shortRaptorProjectId: {shortRaptorProjectId} retrieved");
      return project;
    }

    /// <summary>
    /// Gets intersecting projects in localDB . applicationContext i.e. no customer. 
    ///   if projectUid, get it if it overlaps in localDB
    ///    else get overlapping projects in localDB for this CustomerUID
    /// </summary>
    public static async Task<List<ProjectDatabaseModel>> GetIntersectingProjects(
      string customerUid, double latitude, double longitude,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var projects = (await projectRepo.GetIntersectingProjects(customerUid, latitude, longitude)).ToList(); ;

      log.LogInformation($"Projects for customerUid: {customerUid} count: {projects.Count}");
      return projects;
    }

    public static async Task<bool> DoesProjectOverlap(string customerUid, Guid projectUid, string databaseProjectBoundary,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var overlaps =
        await projectRepo.DoesPolygonOverlap(customerUid, databaseProjectBoundary, projectUid == Guid.Empty ? string.Empty : projectUid.ToString());
      if (overlaps)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 43);

      log.LogDebug($"No overlapping projects for: {projectUid}");
      return overlaps;
    }

    #region coordSystem


    /// <summary>
    /// validate CoordinateSystem if provided
    /// </summary>
    public static async Task<bool> ValidateCoordSystemInProductivity3D(IProjectEvent project,
    IServiceExceptionHandler serviceExceptionHandler, IHeaderDictionary customHeaders,
    IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord)
    {
      var csFileName = project is CreateProjectEvent
        ? ((CreateProjectEvent)project).CoordinateSystemFileName
        : ((UpdateProjectEvent)project).CoordinateSystemFileName;
      var csFileContent = project is CreateProjectEvent
        ? ((CreateProjectEvent)project).CoordinateSystemFileContent
        : ((UpdateProjectEvent)project).CoordinateSystemFileContent;
      if (!string.IsNullOrEmpty(csFileName) || csFileContent != null)
      {
        ProjectDataValidator.ValidateFileName(csFileName);
        CoordinateSystemSettingsResult coordinateSystemSettingsResult = null;
        try
        {
          coordinateSystemSettingsResult = await productivity3dV1ProxyCoord
            .CoordinateSystemValidate(csFileContent, csFileName, customHeaders);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
            "productivity3dV1ProxyCoord.CoordinateSystemValidate", e.Message);
        }

        if (coordinateSystemSettingsResult == null)
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 46);

        if (coordinateSystemSettingsResult != null &&
            coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 47,
            coordinateSystemSettingsResult.Code.ToString(),
            coordinateSystemSettingsResult.Message);
        }
      }

      return true;
    }

    /// <summary>
    /// Create CoordinateSystem in Raptor and save a copy of the file in TCC
    /// </summary>
    public static async Task CreateCoordSystemInProductivity3dAndTcc(Guid projectUid, int shortRaptorProjectId,
      string coordinateSystemFileName,
      byte[] coordinateSystemFileContent, bool isCreate,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, string customerUid,
      IHeaderDictionary customHeaders,
      IProjectRepository projectRepo, IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord, IConfigurationStore configStore,
      IFileRepository fileRepo, IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn,
      ICwsDesignClient cwsDesignClient, ICwsProfileSettingsClient cwsProfileSettingsClient)
    {
      if (!string.IsNullOrEmpty(coordinateSystemFileName))
      {
        var headers = customHeaders;
        headers.TryGetValue("X-VisionLink-ClearCache", out var caching);
        if (string.IsNullOrEmpty(caching)) // may already have been set by acceptance tests
          headers.Add("X-VisionLink-ClearCache", "true");

        try
        {
          //Pass coordinate system to Raptor
          var coordinateSystemSettingsResult = await productivity3dV1ProxyCoord
            .CoordinateSystemPost(shortRaptorProjectId, coordinateSystemFileContent,
              coordinateSystemFileName, headers);
          var message = string.Format($"Post of CS create to RaptorServices returned code: {0} Message {1}.",
            coordinateSystemSettingsResult?.Code ?? -1,
            coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          log.LogDebug(message);
          if (coordinateSystemSettingsResult == null ||
              coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
          {
            if (isCreate)
              await DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid, log, projectRepo);

            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 41,
              (coordinateSystemSettingsResult?.Code ?? -1).ToString(),
              coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          }

          //and save copy of file in TCC
          var fileSpaceId = configStore.GetValueString("TCCFILESPACEID");
          if (string.IsNullOrEmpty(fileSpaceId))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48);
          }

          using (var ms = new MemoryStream(coordinateSystemFileContent))
          {
            await TccHelper.WriteFileToTCCRepository(
              ms, customerUid, projectUid.ToString(), coordinateSystemFileName,
              false, null, fileSpaceId, log, serviceExceptionHandler, fileRepo);
          }

          //save copy to DataOcean
          var rootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
          if (string.IsNullOrEmpty(rootFolder))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 115);
          }

          using (var ms = new MemoryStream(coordinateSystemFileContent))
          {
            await DataOceanHelper.WriteFileToDataOcean(
              ms, rootFolder, customerUid, projectUid.ToString(),
              DataOceanFileUtil.DataOceanFileName(coordinateSystemFileName, false, projectUid, null),
              log, serviceExceptionHandler, dataOceanClient, authn, projectUid, configStore);
          }

          //save to CWS
          using (var ms = new MemoryStream(coordinateSystemFileContent))
          {
            //TODO: handle errors from CWS
            await CwsConfigFileHelper.SaveFileToCws(projectUid, coordinateSystemFileName, ms, ImportedFileType.CwsCalibration, 
              cwsDesignClient, cwsProfileSettingsClient, customHeaders);
          }
        }
        catch (Exception e)
        {
          if (isCreate)
            await DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid, log, projectRepo);

          //Don't hide exceptions thrown above
          if (e is ServiceException)
            throw;
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "productivity3dV1ProxyCoord.CoordinateSystemPost", e.Message);
        }
      }
    }

    #endregion coordSystem


    #region S3
    /// <summary>
    /// Writes the importedFile to S3
    ///   if file exists, it will be overwritten
    ///   returns FileDescriptor for backwards compatability
    /// </summary>
    /// <returns></returns>
    public static FileDescriptor WriteFileToS3Repository(
      Stream fileContents, string projectUid, string filename,
      bool isSurveyedSurface, DateTime? surveyedUtc,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      ITransferProxy persistantTransferProxy)
    {
      string finalFilename = filename;
      if (isSurveyedSurface && surveyedUtc != null) // validation should prevent this
        finalFilename = finalFilename.IncludeSurveyedUtcInName(surveyedUtc.Value);

      var s3FullPath = $"{projectUid}/{finalFilename}";
      try
      {
        persistantTransferProxy.Upload(fileContents, s3FullPath);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "transferProxy.Upload()",
          e.Message);
      }

      log.LogInformation($"WriteFileToS3Repository. Process add design :{finalFilename}, for Project:{projectUid}");
      return FileDescriptor.CreateFileDescriptor(string.Empty, string.Empty, finalFilename);
    }
    #endregion S3

    #region rollback

    /// <summary>
    /// Used internally, if a step fails, after a project has been CREATED, 
    ///    then delete it permanently i.e. don't just set IsArchived.
    /// Since v4 CreateProjectInDB also associates projectCustomer then roll this back also.
    /// DissociateProjectCustomer actually deletes the DB ent4ry
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="log"></param>
    /// <param name="projectRepo"></param>
    /// <returns></returns>
    public static async Task DeleteProjectPermanentlyInDb(Guid customerUid, Guid projectUid, ILogger log,
      IProjectRepository projectRepo)
    {
      log.LogDebug($"DeleteProjectPermanentlyInDB: {projectUid}");
      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = projectUid,
        DeletePermanently = true,
        ActionUTC = DateTime.UtcNow
      };
      await projectRepo.StoreEvent(deleteProjectEvent);
    }

    #endregion rollback

  }
}
