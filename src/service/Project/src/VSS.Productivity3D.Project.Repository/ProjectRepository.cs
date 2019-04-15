﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.MasterData.Repositories.Extensions;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectDataModel=VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.Productivity3D.Project.Repository
{
  public class ProjectRepository : RepositoryBase, IRepository<IProjectEvent>, IProjectRepository
  {
    private const int LegacyProjectIdCutoff = 2000000;

    // The landfill Service requires the existance of a Geofence representing the Projects Boundary.
    // Its type is ProjectType and it must be associated with a ProjectGeofence 
    private static bool _isProjectTypeGeofenceRequired = false;

    public ProjectRepository(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore,
      logger)
    {
      Log = logger.CreateLogger<ProjectRepository>();
      if (!bool.TryParse(configurationStore.GetValueString("ENVIRONMENT_PROJECTTYPEGEOFENCE_ISREQUIRED"),
        out _isProjectTypeGeofenceRequired))
      {
        _isProjectTypeGeofenceRequired = false;
      }
    }

    #region store

    public async Task<int> StoreEvent(IProjectEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        Log.LogWarning("Unsupported project event type");
        return 0;
      }

      Log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent) evt;
        var project = new ProjectDataModel
        {
          LegacyProjectID = projectEvent.ProjectID,
          Description = projectEvent.Description,
          Name = projectEvent.ProjectName,
          ProjectTimeZone = projectEvent.ProjectTimezone,
          LandfillTimeZone = PreferencesTimeZones.WindowsToIana(projectEvent.ProjectTimezone),
          ProjectUID = projectEvent.ProjectUID.ToString(),
          EndDate = projectEvent.ProjectEndDate.Date,
          LastActionedUTC = projectEvent.ActionUTC,
          StartDate = projectEvent.ProjectStartDate.Date,
          ProjectType = projectEvent.ProjectType
        };

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        project.GeometryWKT = RepositoryHelper.GetPolygonWKT(projectEvent.ProjectBoundary);
        if (!string.IsNullOrEmpty(project.GeometryWKT))
        {
          upsertedCount = await UpsertProjectDetail(project, "CreateProjectEvent");
        }
        else
        {
          Log.LogWarning(
            $"ProjectRepository/CreateProject: Unable to createProject as Boundary is missing. Project: {JsonConvert.SerializeObject(project)}))')");
        }
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent) evt;

        var project = new ProjectDataModel
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          Name = projectEvent.ProjectName,
          Description = projectEvent.Description,
          EndDate = projectEvent.ProjectEndDate.Date,
          LastActionedUTC = projectEvent.ActionUTC,
          ProjectType = projectEvent.ProjectType,
          ProjectTimeZone = projectEvent.ProjectTimezone,
          LandfillTimeZone = PreferencesTimeZones.WindowsToIana(projectEvent.ProjectTimezone)
        };

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        project.GeometryWKT = RepositoryHelper.GetPolygonWKT(projectEvent.ProjectBoundary);
        upsertedCount = await UpsertProjectDetail(project, "UpdateProjectEvent");
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent) evt;
        var project = new ProjectDataModel
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectDetail(project, "DeleteProjectEvent", projectEvent.DeletePermanently);
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer) evt;
        var customerProject = new CustomerProject();
        customerProject.ProjectUID = projectEvent.ProjectUID.ToString();
        customerProject.CustomerUID = projectEvent.CustomerUID.ToString();
        customerProject.LegacyCustomerID = projectEvent.LegacyCustomerID;
        customerProject.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertCustomerProjectDetail(customerProject, "AssociateProjectCustomerEvent");
      }
      else if (evt is DissociateProjectCustomer)
      {
        var projectEvent = (DissociateProjectCustomer) evt;
        var customerProject = new CustomerProject
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          CustomerUID = projectEvent.CustomerUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertCustomerProjectDetail(customerProject, "DissociateProjectCustomerEvent");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var projectEvent = (AssociateProjectGeofence) evt;
        var projectGeofence = new ProjectGeofence
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          GeofenceUID = projectEvent.GeofenceUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
      }
      else if (evt is DissociateProjectGeofence)
      {
        var projectEvent = (DissociateProjectGeofence) evt;
        var projectGeofence = new ProjectGeofence
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          GeofenceUID = projectEvent.GeofenceUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "DissociateProjectGeofenceEvent");
      }
      else if (evt is CreateImportedFileEvent)
      {
        var projectEvent = (CreateImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          ImportedFileId = projectEvent.ImportedFileID,
          CustomerUid = projectEvent.CustomerUID.ToString(),
          ImportedFileType = projectEvent.ImportedFileType,
          Name = projectEvent.Name,
          FileDescriptor = projectEvent.FileDescriptor,
          FileCreatedUtc = projectEvent.FileCreatedUtc,
          FileUpdatedUtc = projectEvent.FileUpdatedUtc,
          ImportedBy = projectEvent.ImportedBy,
          SurveyedUtc = projectEvent.SurveyedUTC,
          DxfUnitsType = projectEvent.DxfUnitsType,
          MinZoomLevel = projectEvent.MinZoomLevel,
          MaxZoomLevel = projectEvent.MaxZoomLevel,
          LastActionedUtc = projectEvent.ActionUTC,
          ParentUid = projectEvent.ParentUID?.ToString(),
          Offset = projectEvent.Offset
        };
        upsertedCount = await UpsertImportedFile(importedFile, "CreateImportedFileEvent");
      }
      else if (evt is UpdateImportedFileEvent)
      {
        var projectEvent = (UpdateImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          FileDescriptor = projectEvent.FileDescriptor,
          FileCreatedUtc = projectEvent.FileCreatedUtc,
          FileUpdatedUtc = projectEvent.FileUpdatedUtc,
          ImportedBy = projectEvent.ImportedBy,
          SurveyedUtc = projectEvent.SurveyedUtc,
          MinZoomLevel = projectEvent.MinZoomLevel,
          MaxZoomLevel = projectEvent.MaxZoomLevel,
          LastActionedUtc = projectEvent.ActionUTC,
          Offset = projectEvent.Offset
        };
        upsertedCount = await UpsertImportedFile(importedFile, "UpdateImportedFileEvent");
      }
      else if (evt is DeleteImportedFileEvent)
      {
        var projectEvent = (DeleteImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "DeleteImportedFileEvent",
          projectEvent.DeletePermanently);
      }
      else if (evt is UndeleteImportedFileEvent)
      {
        var projectEvent = (UndeleteImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "UndeleteImportedFileEvent");
      }
      else if (evt is UpdateProjectSettingsEvent)
      {
        var projectEvent = (UpdateProjectSettingsEvent) evt;
        var projectSettings = new ProjectSettings
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ProjectSettingsType = projectEvent.ProjectSettingsType,
          Settings = projectEvent.Settings,
          UserID = projectEvent.UserID,
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectSettings(projectSettings);
      }

      return upsertedCount;
    }

    #endregion store


    #region project

    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertProjectDetail(Abstractions.Models.DatabaseModels.Project project, string eventType, bool isDeletePermanently = false)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                ProjectUID, Description, LegacyProjectID, Name, fk_ProjectTypeID AS ProjectType, IsDeleted,
                ProjectTimeZone, LandfillTimeZone, 
                LastActionedUTC, StartDate, EndDate, AsWKT(PolygonST) AS GeometryWKT,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC
              FROM Project
              WHERE ProjectUID = @ProjectUID
                OR LegacyProjectId = @LegacyProjectID",
        new {ProjectUID = project.ProjectUID, LegacyProjectID = project.LegacyProjectID}
      )).FirstOrDefault();

      if (eventType == "CreateProjectEvent")
        upsertedCount = await CreateProject(project, existing);

      if (eventType == "UpdateProjectEvent")
        upsertedCount = await UpdateProject(project, existing);

      if (eventType == "DeleteProjectEvent")
        upsertedCount = await DeleteProject(project, existing, isDeletePermanently);
      return upsertedCount;
    }

    private async Task<int> CreateProject(Abstractions.Models.DatabaseModels.Project project, Abstractions.Models.DatabaseModels.Project existing)
    {
      var upsertedCount = 0;
      Log.LogDebug($"ProjectRepository/CreateProject: project={JsonConvert.SerializeObject(project)}))')");

      if (project.StartDate > project.EndDate)
      {
        Log.LogDebug("Project will not be created as startDate > endDate");
        return upsertedCount;
      }

      if (existing == null)
      {
        string insert = BuildProjectInsertString(project);

        upsertedCount = await ExecuteWithAsyncPolicy(insert, project);
        Log.LogDebug($"ProjectRepository/CreateProject: (insert): inserted {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
          await UpsertProjectTypeGeofence("CreatedProject", project);
        }

        return upsertedCount;
      }

      // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      //       update everything but ActionUTC from the create
      if ((existing.LastActionedUTC >= project.LastActionedUTC) && existing.IsDeleted == true)
      {
        project.IsDeleted = true;

        // this create could have the legit legacyProjectId
        project.LegacyProjectID =
          project.LegacyProjectID > 0 && project.LegacyProjectID < LegacyProjectIdCutoff
            ? project.LegacyProjectID
            : existing.LegacyProjectID;

        // leave more recent values
        project.Name = string.IsNullOrEmpty(existing.Name) ? project.Name : existing.Name;
        project.Description = string.IsNullOrEmpty(existing.Description) ? project.Description : existing.Description;
        project.ProjectTimeZone = string.IsNullOrEmpty(existing.ProjectTimeZone)
          ? project.ProjectTimeZone
          : existing.ProjectTimeZone;
        project.LandfillTimeZone = string.IsNullOrEmpty(existing.LandfillTimeZone)
          ? project.LandfillTimeZone
          : existing.LandfillTimeZone;
        project.StartDate = existing.StartDate == DateTime.MinValue ? project.StartDate : existing.StartDate;
        project.EndDate = existing.EndDate == DateTime.MinValue ? project.EndDate : existing.EndDate;
        project.LastActionedUTC = existing.LastActionedUTC;

        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }

        project.GeometryWKT = string.IsNullOrEmpty(existing.GeometryWKT) ? project.GeometryWKT : existing.GeometryWKT;

        string update = BuildProjectUpdateString(project);
        Log.LogDebug("ProjectRepository/CreateProject: going to update a dummy project");

        upsertedCount = await ExecuteWithAsyncPolicy(update, project);
        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        Log.LogDebug($"ProjectRepository/CreateProject: (update): updated {upsertedCount} rows ");
        return upsertedCount;
      }

      // an update was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      // leave the more recent EndDate, Name, Description, ProjectType and actionUTC alone
      if (existing.LastActionedUTC >= project.LastActionedUTC)
      {
        Log.LogDebug("ProjectRepository/CreateProject: create arrived after an update so updating project");

        // this create could have the legit legacyProjectId
        project.LegacyProjectID =
          project.LegacyProjectID > 0 && project.LegacyProjectID < LegacyProjectIdCutoff
            ? project.LegacyProjectID
            : existing.LegacyProjectID;

        // leave more recent values
        project.Name = string.IsNullOrEmpty(existing.Name) ? project.Name : existing.Name;
        project.Description = string.IsNullOrEmpty(existing.Description) ? project.Description : existing.Description;
        project.ProjectTimeZone = string.IsNullOrEmpty(existing.ProjectTimeZone)
          ? project.ProjectTimeZone
          : existing.ProjectTimeZone;
        project.LandfillTimeZone = string.IsNullOrEmpty(existing.LandfillTimeZone)
          ? project.LandfillTimeZone
          : existing.LandfillTimeZone;
        project.StartDate = existing.StartDate == DateTime.MinValue ? project.StartDate : existing.StartDate;
        project.EndDate = existing.EndDate == DateTime.MinValue ? project.EndDate : existing.EndDate;
        project.LastActionedUTC = existing.LastActionedUTC;

        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }

        project.GeometryWKT = string.IsNullOrEmpty(existing.GeometryWKT) ? project.GeometryWKT : existing.GeometryWKT;

        string update = BuildProjectUpdateString(project);
        upsertedCount = await ExecuteWithAsyncPolicy(update, project);
        Log.LogDebug($"ProjectRepository/CreateProject: (updateExisting): updated {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
          await UpsertProjectTypeGeofence("UpdatedProject", project);
        }

        return upsertedCount;
      }

      Log.LogDebug("ProjectRepository/CreateProject: No action as project already exists.");
      return upsertedCount;
    }

    private async Task<int> UpdateProject(Abstractions.Models.DatabaseModels.Project project, Abstractions.Models.DatabaseModels.Project existing)
    {
      Log.LogDebug($"ProjectRepository/UpdateProject: project={JsonConvert.SerializeObject(project)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.EndDate < existing.StartDate)
        {
          Log.LogDebug(
            $"ProjectRepository/UpdateProject: failed to update project={project.ProjectUID} EndDate < StartDate");
          return upsertedCount;
        }

        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          project.LegacyProjectID = existing.LegacyProjectID;
          project.Name = string.IsNullOrEmpty(project.Name) ? existing.Name : project.Name;
          project.Description = string.IsNullOrEmpty(project.Description) ? existing.Description : project.Description;
          project.ProjectTimeZone = string.IsNullOrEmpty(project.ProjectTimeZone)
            ? existing.ProjectTimeZone
            : project.ProjectTimeZone;
          project.LandfillTimeZone = string.IsNullOrEmpty(project.LandfillTimeZone)
            ? existing.LandfillTimeZone
            : project.LandfillTimeZone;
          project.StartDate = project.StartDate == DateTime.MinValue ? existing.StartDate : project.StartDate;

          if (string.IsNullOrEmpty(project.CoordinateSystemFileName))
          {
            project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
            project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
          }

          project.GeometryWKT = string.IsNullOrEmpty(project.GeometryWKT) ? existing.GeometryWKT : project.GeometryWKT;

          Log.LogDebug($"ProjectRepository/UpdateProject: updating project={project.ProjectUID}");

          string update = BuildProjectUpdateString(project);
          upsertedCount = await ExecuteWithAsyncPolicy(update, project);
          Log.LogDebug(
            $"ProjectRepository/UpdateProject: upserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

          if (upsertedCount > 0)
          {
            upsertedCount = await InsertProjectHistory(project);
            await UpsertProjectTypeGeofence("UpdatedProject", project);
          }

          return upsertedCount;
        }

        Log.LogDebug($"ProjectRepository/UpdateProject: old update event ignored project={project.ProjectUID}");
      }
      else
      {
        // an update was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        Log.LogDebug(
          $"ProjectRepository/UpdateProject: project doesn't already exist, creating one. project={project.ProjectUID}");
        if (String.IsNullOrEmpty(project.ProjectTimeZone))
          project.ProjectTimeZone = "";

        string insert = BuildProjectInsertString(project);
        upsertedCount = await ExecuteWithAsyncPolicy(insert, project);
        Log.LogDebug($"ProjectRepository/UpdateProject: (insert): inserted {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
          await UpsertProjectTypeGeofence("CreatedProject", project);
        }

        return upsertedCount;
      }

      return upsertedCount;
    }

    private async Task<int> DeleteProject(Abstractions.Models.DatabaseModels.Project project, Abstractions.Models.DatabaseModels.Project existing, bool isDeletePermanently)
    {
      Log.LogDebug(
        $"ProjectRepository/DeleteProject: project={JsonConvert.SerializeObject(project)} permanently: {isDeletePermanently}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          // this is for internal use only to roll-back after failed series of steps
          if (isDeletePermanently)
          {
            Log.LogDebug(
              $"ProjectRepository/DeleteProject: deleting a project permanently: {JsonConvert.SerializeObject(project)}");
            const string delete =
              @"DELETE FROM Project
                    WHERE ProjectUID = @ProjectUID";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, project);
            Log.LogDebug(
              $"ProjectRepository/DeleteProject: deleted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

            return upsertedCount;
          }
          else
          {
            Log.LogDebug($"ProjectRepository/DeleteProject: updating project={project.ProjectUID}");

            // on deletion, the projects endDate will be set to now, in its local time.
            var localEndDate = project.LastActionedUTC.ToLocalDateTime(existing.LandfillTimeZone);
            if (localEndDate != null)
            {
              project.EndDate = localEndDate.Value.Date;
              const string update =
                @"UPDATE Project                
                  SET IsDeleted = 1,
                    EndDate = @EndDate,
                    LastActionedUTC = @LastActionedUTC
                  WHERE ProjectUID = @ProjectUID";
              upsertedCount = await ExecuteWithAsyncPolicy(update, project);
              Log.LogDebug(
                $"ProjectRepository/DeleteProject: upserted {upsertedCount} rows for: projectUid:{project.ProjectUID} new endDate: {project.EndDate}");
            }
            else
            {
              Log.LogError($"ProjectRepository/DeleteProject: Unable to convert current Utc date to local. Unknown timeZone: {existing.LandfillTimeZone}");
            }

            if (upsertedCount > 0)
            {
              upsertedCount = await InsertProjectHistory(project);
            }

            return upsertedCount;
          }
        }
      }
      else
      {
        // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        Log.LogDebug(
          $"ProjectRepository/DeleteProject: delete event where no project exists, creating one. project={project.ProjectUID}");
        project.Name = "";
        project.ProjectTimeZone = "";
        project.LandfillTimeZone = "";
        project.ProjectType = ProjectType.Standard;

        const string delete =
          "INSERT Project " +
          "    (ProjectUID, Name, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC)" +
          "  VALUES " +
          "    (@ProjectUID, @Name, @ProjectType, 1, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(delete, project);
        Log.LogDebug(
          $"ProjectRepository/DeleteProject: inserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        return upsertedCount;
      }

      return upsertedCount;
    }


    private string BuildProjectInsertString(Abstractions.Models.DatabaseModels.Project project)
    {
      string formattedPolygon = RepositoryHelper.WKTToSpatial(project.GeometryWKT);
  
      string insert = null;
      if (project.LegacyProjectID <= 0) // allow db autoincrement on legacyProjectID
        insert = string.Format(
          "INSERT Project " +
          "    (ProjectUID, Name, Description, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate, PolygonST, CoordinateSystemFileName, CoordinateSystemLastActionedUTC) " +
          "  VALUES " +
          "    (@ProjectUID, @Name, @Description, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate, {0}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC)"
          , formattedPolygon);
      else
        insert = string.Format(
          "INSERT Project " +
          "    (ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate, PolygonST, CoordinateSystemFileName, CoordinateSystemLastActionedUTC ) " +
          "  VALUES " +
          "    (@ProjectUID, @LegacyProjectID, @Name, @Description, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate, {0}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC)"
          , formattedPolygon);
      return insert;
    }

    private string BuildProjectUpdateString(Abstractions.Models.DatabaseModels.Project project)
    {
      string formattedPolygon = RepositoryHelper.WKTToSpatial(project.GeometryWKT);

      string update = null;
      if (project.LegacyProjectID <= 0) // allow db autoincrement on legacyProjectID
      {
        update = string.Format(
          @"UPDATE Project
                SET 
                  Name = @Name, Description = @Description, fk_ProjectTypeID = @ProjectType,
                  IsDeleted = @IsDeleted,
                  ProjectTimeZone = @ProjectTimeZone, LandfillTimeZone = @LandfillTimeZone,
                  LastActionedUTC = @LastActionedUTC,
                  StartDate = @StartDate, EndDate = @EndDate,   
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC,
                  PolygonST = {0}
                WHERE ProjectUID = @ProjectUID"
          , formattedPolygon);
      }
      else
      {
        update = string.Format(
          @"UPDATE Project
                SET LegacyProjectID = @LegacyProjectID, 
                  Name = @Name, Description = @Description, fk_ProjectTypeID = @ProjectType,
                  IsDeleted = @IsDeleted,
                  ProjectTimeZone = @ProjectTimeZone, LandfillTimeZone = @LandfillTimeZone,
                  LastActionedUTC = @LastActionedUTC,
                  StartDate = @StartDate, EndDate = @EndDate,   
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC,
                  PolygonST = {0}
                WHERE ProjectUID = @ProjectUID"
          , formattedPolygon);
      }

      return update;
    }

    #endregion project

    #region landfill

    private async Task UpsertProjectTypeGeofence(string upsertType, Abstractions.Models.DatabaseModels.Project project)
    {
      if (!_isProjectTypeGeofenceRequired)
        return;

      if (string.IsNullOrEmpty(project.GeometryWKT))
      {
        Log.LogInformation(
          $"ProjectRepository/UpsertProjectTypeGeofence: Unable to Upsert GeofenceBoundary as boundary not available. UpsertType {upsertType}. project={project.ProjectUID}.");
        return;
      }

      // may be an existing one if this create comes from a replay of kafka que.
      var select = "SELECT GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, AsWKT(PolygonST) AS GeometryWKT, " +
                   "     FillColor, IsTransparent, IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID, " +
                   "     AreaSqMeters, g.LastActionedUTC " +
                   "  FROM ProjectGeofence pg " +
                   "   INNER JOIN Geofence g ON g.GeofenceUID = pg.fk_GeofenceUID " +
                   $" WHERE fk_ProjectUID = '{project.ProjectUID}' " +
                   $"  AND fk_GeofenceTypeID = {(int) GeofenceType.Project}; ";
      var existingGeofence = (await QueryWithAsyncPolicy<Geofence>(select)).FirstOrDefault();

      Log.LogDebug(
        $"ProjectRepository/UpsertProjectTypeGeofence: going to upsert. upsertType {upsertType}. project={project.ProjectUID} existingGeofence? {existingGeofence}");

      if (existingGeofence == null)
        await CreateGeofenceAndAssociation(project);
      else
        await UpdateGeofence(project, existingGeofence);
    }

    private async Task<int> CreateGeofenceAndAssociation(Abstractions.Models.DatabaseModels.Project project)
    {
      var geofence = new Geofence().Setup();
      geofence.GeofenceUID = Guid.NewGuid().ToString();
      geofence.Name = project.Name;
      geofence.GeofenceType = GeofenceType.Project;
      geofence.GeometryWKT = project.GeometryWKT;
      geofence.CustomerUID = ""; // we don't know this from a Project Kafka event
      geofence.AreaSqMeters = GeofenceValidation.CalculateAreaSqMeters(project.GeometryWKT);
      geofence.IsDeleted = false;
      geofence.LastActionedUTC = DateTime.UtcNow;

      string formattedPolygon = RepositoryHelper.WKTToSpatial(project.GeometryWKT);

      string insert = string.Format(
         "INSERT Geofence " +
         "     (GeofenceUID, Name, Description, PolygonST, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID, AreaSqMeters) " +
         " VALUES " +
         "     (@GeofenceUID, @Name, @Description, {0}, @FillColor, @IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType, @AreaSqMeters)", formattedPolygon);

      var upsertedCount = await ExecuteWithAsyncPolicy(insert, geofence);
      Log.LogDebug(
        $"ProjectRepository/UpsertGeofence inserted. upsertedCount {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");

      if (upsertedCount == 1)
      {
        var projectGeofence = new ProjectGeofence()
        {
          ProjectUID = project.ProjectUID,
          GeofenceUID = geofence.GeofenceUID,
          LastActionedUTC = DateTime.UtcNow
        };
        await AssociateProjectGeofence(projectGeofence, null);
        return upsertedCount;
      }

      return 0;
    }

    private async Task<int> UpdateGeofence(Abstractions.Models.DatabaseModels.Project project, Geofence existingGeofence)
    {
      string formattedPolygon = RepositoryHelper.WKTToSpatial(project.GeometryWKT);

      var update = "UPDATE Geofence " +
                   $" SET PolygonST = {formattedPolygon} " +
                   $" WHERE GeofenceUID = '{existingGeofence.GeofenceUID}' " +
                   $"  AND fk_GeofenceTypeID = {(int) GeofenceType.Project}; ";
      var upsertedCount = await ExecuteWithAsyncPolicy(update);
      Log.LogDebug(
        $"ProjectRepository/UpsertGeofence updated. upsertedCount {upsertedCount} rows for: geofenceUid:{existingGeofence.GeofenceUID}");

      return upsertedCount;
    }

    #endregion landfill

    #region associate

    private async Task<int> UpsertCustomerProjectDetail(CustomerProject customerProject, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<CustomerProject>
      (@"SELECT 
                fk_CustomerUID AS CustomerUID, LegacyCustomerID, fk_ProjectUID AS ProjectUID, LastActionedUTC
              FROM CustomerProject
              WHERE fk_CustomerUID = @CustomerUID 
                AND fk_ProjectUID = @ProjectUID",
        new {CustomerUID = customerProject.CustomerUID, ProjectUID = customerProject.ProjectUID}
      )).FirstOrDefault();

      if (eventType == "AssociateProjectCustomerEvent")
        upsertedCount = await AssociateProjectCustomer(customerProject, existing);
      if (eventType == "DissociateProjectCustomerEvent")
        upsertedCount = await DissociateProjectCustomer(customerProject, existing);
      return upsertedCount;
    }

    private async Task<int> AssociateProjectCustomer(CustomerProject customerProject, CustomerProject existing)
    {
      Log.LogDebug(
        $"ProjectRepository/AssociateProjectCustomer: customerProject={JsonConvert.SerializeObject(customerProject)}");

      const string insert =
        @"INSERT CustomerProject
              (fk_ProjectUID, fk_CustomerUID, LegacyCustomerID, LastActionedUTC)
            VALUES
              (@ProjectUID, @CustomerUID, @LegacyCustomerID, @LastActionedUTC)
            ON DUPLICATE KEY UPDATE
              LastActionedUTC =
                IF ( VALUES(LastActionedUTC) >= LastActionedUTC, 
                    VALUES(LastActionedUTC), LastActionedUTC),
              LegacyCustomerID =
                IF ( VALUES(LastActionedUTC) >= LastActionedUTC, 
                    VALUES(LegacyCustomerID), LegacyCustomerID)";

      var upsertedCount = await ExecuteWithAsyncPolicy(insert, customerProject);
      Log.LogDebug(
        $"ProjectRepository/AssociateProjectCustomer: upserted {upsertedCount} rows (1=insert, 2=update) for: customerProjectUid:{customerProject.CustomerUID}");
      return upsertedCount.CalculateUpsertCount();
    }

    private async Task<int> DissociateProjectCustomer(CustomerProject customerProject, CustomerProject existing)
    {
      var upsertedCount = 0;

      Log.LogDebug(
        $"ProjectRepository/DissociateProjectCustomer: customerProject={JsonConvert.SerializeObject(customerProject)} existing={JsonConvert.SerializeObject(existing)}");

      if (existing != null)
      {
        if (customerProject.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE FROM CustomerProject
                WHERE fk_CustomerUID = @CustomerUID 
                  AND fk_ProjectUID = @ProjectUID";
          upsertedCount = await ExecuteWithAsyncPolicy(delete, customerProject);
          Log.LogDebug(
            $"ProjectRepository/DissociateProjectCustomer: upserted {upsertedCount} rows for: customerUid:{customerProject.CustomerUID}");
          return upsertedCount;
        }

        // may have been associated again since, so don't delete
        Log.LogDebug("ProjectRepository/DissociateProjectCustomer: old delete event ignored");
      }
      else
      {
        Log.LogDebug("ProjectRepository/DissociateProjectCustomer: can't delete as none existing");
      }

      return upsertedCount;
    }

    private async Task<int> UpsertProjectGeofenceDetail(ProjectGeofence projectGeofence, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ProjectGeofence>
      (@"SELECT 
              fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
            FROM ProjectGeofence
            WHERE fk_ProjectUID = @ProjectUID AND fk_GeofenceUID = @GeofenceUID",
        new {ProjectUID = projectGeofence.ProjectUID, GeofenceUID = projectGeofence.GeofenceUID}
      )).FirstOrDefault();

      if (eventType == "AssociateProjectGeofenceEvent")
        upsertedCount = await AssociateProjectGeofence(projectGeofence, existing);
      if (eventType == "DissociateProjectGeofenceEvent")
        upsertedCount = await DissociateProjectGeofence(projectGeofence, existing);

      return upsertedCount;
    }

    private async Task<int> AssociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        Log.LogDebug(
          $"ProjectRepository/AssociateProjectGeofence: projectGeofence={JsonConvert.SerializeObject(projectGeofence)}");

        const string insert =
          @"INSERT ProjectGeofence
                (fk_GeofenceUID, fk_ProjectUID, LastActionedUTC)
              VALUES
                (@GeofenceUID, @ProjectUID, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, projectGeofence);
        Log.LogDebug(
          $"ProjectRepository/AssociateProjectGeofence: inserted {upsertedCount} rows for: projectUid:{projectGeofence.ProjectUID} geofenceUid:{projectGeofence.GeofenceUID}");

        return upsertedCount;
      }

      Log.LogDebug(
        $"ProjectRepository/AssociateProjectGeofence: can't create as already exists projectGeofence={JsonConvert.SerializeObject(projectGeofence)}");
      return upsertedCount;
    }

    private async Task<int> DissociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
    {
      var upsertedCount = 0;

      Log.LogDebug(
        $"ProjectRepository/DissociateProjectGeofence: projectGeofence={JsonConvert.SerializeObject(projectGeofence)} existing={JsonConvert.SerializeObject(existing)}");

      if (existing != null)
      {
        if (projectGeofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE FROM ProjectGeofence
                WHERE fk_GeofenceUID = @GeofenceUID 
                  AND fk_ProjectUID = @ProjectUID";
          upsertedCount = await ExecuteWithAsyncPolicy(delete, projectGeofence);
          Log.LogDebug(
            $"ProjectRepository/DissociateProjectGeofence: upserted {upsertedCount} rows for: geofenceUid:{projectGeofence.GeofenceUID}");
          return upsertedCount;
        }

        // may have been associated again since, so don't delete
        Log.LogDebug("ProjectRepository/DissociateProjectGeofence: old delete event ignored");
      }
      else
      {
        Log.LogDebug("ProjectRepository/DissociateProjectGeofence: can't delete as none existing");
      }

      return upsertedCount;
    }

    #endregion associate


    #region importedFiles

    private async Task<int> UpsertImportedFile(ImportedFile importedFile, string eventType,
      bool isDeletePermanently = false)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID as CustomerUID,
              fk_ImportedFileTypeID as ImportedFileType, Name, 
              FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, 
              fk_DXFUnitsTypeID as DxfUnitsType, MinZoomLevel, MaxZoomLevel, Offset, fk_ReferenceImportedFileUID as ParentUID,
              IsDeleted, LastActionedUTC
            FROM ImportedFile
            WHERE ImportedFileUID = @ImportedFileUid", new {ImportedFileUid = importedFile.ImportedFileUid}
      )).FirstOrDefault();

      if (eventType == "CreateImportedFileEvent")
        upsertedCount = await CreateImportedFile(importedFile, existing);

      if (eventType == "UpdateImportedFileEvent")
        upsertedCount = await UpdateImportedFile(importedFile, existing);

      if (eventType == "DeleteImportedFileEvent")
        upsertedCount = await DeleteImportedFile(importedFile, existing, isDeletePermanently);

      if (eventType == "UndeleteImportedFileEvent")
        upsertedCount = await UndeleteImportedFile(importedFile, existing);

      return upsertedCount;
    }

    private async Task<int> CreateImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      var upsertedCount = 0;

      if (existing == null)
      {
        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: going to create importedFile={JsonConvert.SerializeObject(importedFile)}");

        var insert = string.Format(
          "INSERT ImportedFile " +
          "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID, MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID) " +
          "  VALUES " +
          "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUtc, @FileUpdatedUtc, @ImportedBy, @SurveyedUtc, @DxfUnitsType, @MinZoomLevel, @MaxZoomLevel, 0, @LastActionedUtc, @Offset, @ParentUid)");

        upsertedCount = await ExecuteWithAsyncPolicy(insert, importedFile);
        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: (insert): inserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

        if (upsertedCount > 0)
          upsertedCount = await UpsertImportedFileHistory(importedFile);
      }
      else if (existing.LastActionedUtc >= importedFile.LastActionedUtc)
      {
        // an update/delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
        // We don't store (a), and leave actionUTC as the more recent. 

        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: create arrived after an update so inserting importedFile={importedFile.ImportedFileUid}");

        const string update =
          @"UPDATE ImportedFile
              SET fk_ProjectUID = @ProjectUid, 
                ImportedFileID = @ImportedFileId,
                fk_CustomerUID = @CustomerUid,
                fk_ImportedFileTypeID = @ImportedFileType,
                Name = @Name,
                FileDescriptor = @FileDescriptor,
                FileCreatedUTC = @FileCreatedUtc,
                FileUpdatedUTC = @FileUpdatedUtc,
                ImportedBy = @ImportedBy, 
                SurveyedUTC = @SurveyedUtc,
                MinZoomLevel = @MinZoomLevel,
                MaxZoomLevel = @MaxZoomLevel,
                fk_DXFUnitsTypeID = @DxfUnitsType,
                Offset = @Offset,
                fk_ReferenceImportedFileUID  = @ParentUid
              WHERE ImportedFileUID = @ImportedFileUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: (updateExisting): upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

        // don't really care if this didn't pass as may already exist for create/update utc
        if (upsertedCount > 0)
          await UpsertImportedFileHistory(importedFile);
      }
      else
      {

        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: can't create as older actioned importedFile already exists: {importedFile.ImportedFileUid}.");
      }

      return upsertedCount;
    }

    private async Task<int> UpdateImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
      // We don't store (a), and leave actionUTC as the more recent. 
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string update =
            @"UPDATE ImportedFile
                SET 
                  FileDescriptor = @FileDescriptor,
                  FileCreatedUTC = @FileCreatedUtc,
                  FileUpdatedUTC = @FileUpdatedUtc,
                  ImportedBy = @ImportedBy, 
                  SurveyedUTC = @SurveyedUtc,
                  MinZoomLevel = @MinZoomLevel,
                  MaxZoomLevel = @MaxZoomLevel,
                  Offset = @Offset,
                  LastActionedUTC = @LastActionedUtc
                WHERE ImportedFileUID = @ImportedFileUid";

          upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
          Log.LogDebug(
            $"ProjectRepository/UpdateImportedFile: updated {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

          // don't really care if this didn't pass as may already exist for create/update utc
          if (upsertedCount > 0)
            await UpsertImportedFileHistory(importedFile);
        }
        else
        {
          Log.LogDebug(
            $"ProjectRepository/UpdateImportedFile: old update event ignored importedFile {importedFile.ImportedFileUid}");
        }
      }
      else
      {
        // can't create as don't know fk_ImportedFileTypeID, fk_DXFUnitsTypeID or customerUID
        Log.LogDebug(
          $"ProjectRepository/UpdateImportedFile: No ImportedFile exists {importedFile.ImportedFileUid}. Can't create one as don't have enough info e.g. customerUID/type");
      }

      return upsertedCount;
    }

    /// <summary>
    /// Round date time to nearest second
    /// </summary>
    private DateTime RoundDateTimeToSeconds(DateTime dateTime)
    {
      return DateTime.Parse(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    private async Task<int> InsertProjectHistory(Abstractions.Models.DatabaseModels.Project project)
    {
      var insertedCount = 0;
      var insert = string.Format(
        @"INSERT INTO ProjectHistory
              (ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID,
                IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate,
                PolygonST,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
                LastActionedUTC)
              SELECT 
                  ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID,
                  IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate,
                  PolygonST,
                  CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
                  LastActionedUTC
                FROM Project
                WHERE ProjectUID = @ProjectUID;");
      insertedCount = await ExecuteWithAsyncPolicy(insert, project);
      Log.LogDebug($"ProjectRepository/CreateProjectHistory: inserted {insertedCount} rows");
      return insertedCount;
    }

    private async Task<int> UpsertImportedFileHistory(ImportedFile importedFile)
    {
      var insertedCount = 0;
      var importedFileHistoryExisting = (await QueryWithAsyncPolicy<ImportedFileHistoryItem>
      (@"SELECT 
            fk_ImportedFileUID AS ImportedFileUid, FileCreatedUTC, FileUpdatedUTC, ImportedBy
          FROM ImportedFileHistory
            WHERE fk_ImportedFileUID = @ImportedFileUid",
        new {ImportedFileUid = importedFile.ImportedFileUid}
      )).ToList();

      bool alreadyExists = false;
      // comparing sql dateTimes to c# doesn't work
      if (importedFileHistoryExisting.Any())
      {
        var newCreatedUtcRounded = RoundDateTimeToSeconds(importedFile.FileCreatedUtc);
        var newUpdatedUtcRounded = RoundDateTimeToSeconds(importedFile.FileUpdatedUtc);

        alreadyExists = importedFileHistoryExisting
          .Any(h => RoundDateTimeToSeconds(h.FileCreatedUtc) == newCreatedUtcRounded &&
                    RoundDateTimeToSeconds(h.FileUpdatedUtc) == newUpdatedUtcRounded);
      }

      if (!alreadyExists)
      {
        const string insert =
          @"INSERT ImportedFileHistory
                 (fk_ImportedFileUID, FileCreatedUtc, FileUpdatedUtc, ImportedBy)
            VALUES
              (@ImportedFileUid, @FileCreatedUtc, @FileUpdatedUtc, @ImportedBy)";

        insertedCount = await ExecuteWithAsyncPolicy(insert, importedFile);

        Log.LogDebug(
          $"ProjectRepository/UpsertImportedFileHistory: inserted {insertedCount} rows for: ImportedFileUid:{importedFile.ImportedFileUid} FileCreatedUTC: {importedFile.FileCreatedUtc} FileUpdatedUTC: {importedFile.FileUpdatedUtc}");
      }
      else
      {
        Log.LogDebug(
          $"ProjectRepository/UpsertImportedFileHistory: History already exists ImportedFileUid:{importedFile.ImportedFileUid} FileCreatedUTC: {importedFile.FileCreatedUtc} FileUpdatedUTC: {importedFile.FileUpdatedUtc}");
      }

      return insertedCount;
    }

    private async Task<int> DeleteImportedFile(ImportedFile importedFile, ImportedFile existing,
      bool isDeletePermanently)
    {
      Log.LogDebug(
        $"ProjectRepository/DeleteImportedFile: deleting importedFile: {JsonConvert.SerializeObject(importedFile)} permanent flag:{isDeletePermanently}");
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc >= existing.LastActionedUtc)
        {
          if (isDeletePermanently)
          {
            Log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: deleting importedFile permanently: {importedFile.ImportedFileUid}");
            const string delete =
              @"DELETE FROM ImportedFile
                  WHERE ImportedFileUID = @ImportedFileUid";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, importedFile);
            Log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: deleted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
            return upsertedCount;
          }
          else
          {
            Log.LogDebug($"ProjectRepository/DeleteImportedFile: deleting importedFile {importedFile.ImportedFileUid}");

            const string update =
              @"UPDATE ImportedFile                               
                SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUtc
                WHERE ImportedFileUID = @ImportedFileUid";

            upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
            Log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
            return upsertedCount;
          }
        }
      }
      else
      {
        Log.LogDebug(
          $"ProjectRepository/DeleteImportedFile: can't delete as none existing, ignored. importedFile={importedFile.ImportedFileUid}. Can't create one as don't have enough info e.g.customerUID / type.");
      }

      return upsertedCount;
    }

    private async Task<int> UndeleteImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      // this is an interfaces extension model used solely by ProjectMDM to allow a rollback of a DeleteImportedFile
      Log.LogDebug(
        $"ProjectRepository/UndeleteImportedFile: undeleting importedFile: {JsonConvert.SerializeObject(importedFile)}.");
      var upsertedCount = 0;

      if (existing != null)
      {
        Log.LogDebug($"ProjectRepository/UndeleteImportedFile: undeleting importedFile {importedFile.ImportedFileUid}");

        const string update =
          @"UPDATE ImportedFile                               
                SET IsDeleted = 0
              WHERE ImportedFileUID = @ImportedFileUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
        Log.LogDebug(
          $"ProjectRepository/UndeleteImportedFile: upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
        return upsertedCount;
      }

      Log.LogDebug(
        $"ProjectRepository/UndeleteImportedFile: can't undelete as none existing ignored importedFile={importedFile.ImportedFileUid}.");
      return upsertedCount;
    }

    #endregion importedFiles


    #region projectSettings

    /// <summary>
    ///     Only an upsert is implemented.
    /// 1) because as that is the only endpoint in ProjectMDM
    /// 2) because create and Update have to cover both scenarios anyway
    /// can't update the type or UserID, only the Settings
    /// </summary>
    /// <param name="projectSettings"></param>
    /// <returns></returns>
    private async Task<int> UpsertProjectSettings(ProjectSettings projectSettings)
    {
      Log.LogDebug(
        $"ProjectRepository/UpsertProjectSettings: projectSettings={JsonConvert.SerializeObject(projectSettings)}))')");

      const string upsert =
        @"INSERT ProjectSettings
                 (fk_ProjectUID, fk_ProjectSettingsTypeID, Settings, UserID, LastActionedUTC)
            VALUES
              (@ProjectUid, @ProjectSettingsType, @Settings, @UserID, @LastActionedUtc)
            ON DUPLICATE KEY UPDATE
              LastActionedUTC =
                IF ( VALUES(LastActionedUtc) >= LastActionedUTC, 
                    VALUES(LastActionedUtc), LastActionedUTC),
              Settings =
                IF ( VALUES(LastActionedUtc) >= LastActionedUTC, 
                    VALUES(Settings), Settings)";

      var upsertedCount = await ExecuteWithAsyncPolicy(upsert, projectSettings);
      Log.LogDebug(
        $"ProjectRepository/UpsertProjectSettings: upserted {upsertedCount} rows (1=insert, 2=update) for: projectSettingsProjectUid:{projectSettings.ProjectUid}");
      return upsertedCount.CalculateUpsertCount();
    }

    #endregion projectSettings


    #region gettersProject

    /// <summary>
    ///     There may be 0 or n subscriptions for this project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Abstractions.Models.DatabaseModels.Project> GetProject(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.ProjectUID = @ProjectUID 
                AND p.IsDeleted = 0",
        new {ProjectUID = projectUid})).FirstOrDefault();
      return project;
    }

    /// <summary>
    ///     Gets by legacyProjectID. No subs
    /// </summary>
    /// <returns></returns>
    public async Task<Abstractions.Models.DatabaseModels.Project> GetProject(long legacyProjectId)
    {
      var project = await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(@"SELECT
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
              WHERE p.LegacyProjectID = @LegacyProjectID 
                AND p.IsDeleted = 0",
        new {LegacyProjectID = legacyProjectId});
      return project.FirstOrDefault();
    }


    /// <summary>
    ///     There may be 0 or n subscriptions for this project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjectAndSubscriptions(long legacyProjectID, DateTime validAtDate)
    {
      var projectSubList = QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID,
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID
              WHERE p.LegacyProjectID = @LegacyProjectID 
                AND p.IsDeleted = 0
                AND @validAtDate BETWEEN s.StartDate AND s.EndDate",
        new {LegacyProjectID = legacyProjectID, validAtDate = validAtDate.Date}
      );


      return projectSubList;
    }

    /// <summary>
    ///     There should be 1 or more per ProjectUID
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjectHistory(string projectUid)
    {
      var projectList = QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(@"SELECT 
                ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID as ProjectType, 
                IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate, 
                AsWKT(PolygonST) as GeometryWKT,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
                LastActionedUTC 
              FROM ProjectHistory             
              WHERE ProjectUID = @ProjectUID",
        new {ProjectUID = projectUid});
      return projectList;
    }

    /// <summary>
    ///     gets only 1 row for a particular sub. only 1 projectUID and be associated with a sub
    /// </summary>
    /// <param name="subscriptionUid"></param>
    /// <returns></returns>
    public async Task<Abstractions.Models.DatabaseModels.Project> GetProjectBySubcription(string subscriptionUid)
    {
      var projects = (await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE ps.fk_SubscriptionUID = @SubscriptionUID 
                AND p.IsDeleted = 0",
        new {SubscriptionUID = subscriptionUid}
      )).FirstOrDefault();
      ;


      return projects;
    }


    /// <summary>
    ///     There may be 0 or n subscriptions for each project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjectsForUser(string userUid)
    {
      var projects = QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID,
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN CustomerUser cu on cu.fk_CustomerUID = c.CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE cu.UserUID = @userUid 
                AND p.IsDeleted = 0",
        new {userUid}
      );


      return projects;
    }

    /// <summary>
    ///     There may be 0 or n subscriptions for each project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjectsForCustomerUser(string customerUid, string userUid)
    {
      var projects = QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE cp.fk_CustomerUID = @CustomerUID 
                AND cu.UserUID = @userUid 
                AND p.IsDeleted = 0",
        new {CustomerUID = customerUid, userUid}
      );


      return projects;
    }

    /// <summary>
    ///     There may be 0 or n subscriptions for each project. None/many may be current.
    ///     This method gets the latest EndDate so at most 1 sub per project
    ///     Also returns the GeofenceWRK. List returned includes archived projects.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjectsForCustomer(string customerUid)
    {
      // mysql doesn't have any nice mssql features like rowNumber/paritionBy, so quicker to do in c#
      var projects = await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
              c.CustomerUID, cp.LegacyCustomerID, 
              p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
              p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
              p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
              ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
            FROM Customer c  
              JOIN CustomerProject cp ON cp.fk_CustomerUID = c.CustomerUID 
              JOIN Project p on p.ProjectUID = cp.fk_ProjectUID           
              LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
              LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
            WHERE c.CustomerUID = @CustomerUID",
        new {CustomerUID = customerUid}
      );


      // need to get the row with the later SubscriptionEndDate if there are duplicates
      // Also if there are >1 projectGeofences.. hmm.. it will just return either
      return projects.OrderByDescending(proj => proj.SubscriptionEndDate).GroupBy(d => d.ProjectUID)
        .Select(g => g.First()).ToList();
    }

    /// <summary>
    ///     Gets the specified project without linked data like customer and subscription.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>The project</returns>
    public async Task<Abstractions.Models.DatabaseModels.Project> GetProjectOnly(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC
              FROM Project p 
              WHERE p.ProjectUID = @ProjectUID",
        new {ProjectUID = projectUid}
      )).FirstOrDefault();


      return project;
    }

    /// <summary>
    ///     Checks if a project with the specified projectUid exists.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>true if project exists or false otherwise</returns>
    public async Task<bool> ProjectExists(string projectUid)
    {
      var uid = (await QueryWithAsyncPolicy<string>
      (@"SELECT p.ProjectUID
              FROM Project p 
              WHERE p.ProjectUID = @ProjectUID",
        new {ProjectUID = projectUid}
      )).FirstOrDefault();


      return !string.IsNullOrEmpty(uid);
    }

    /// <summary>
    ///     Checks if a project with the specified projectUid is associated with a customer.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>true if project is associated with a customer or false otherwise</returns>
    public async Task<bool> CustomerProjectExists(string projectUid)
    {
      var uid = (await QueryWithAsyncPolicy<string>
      (@"SELECT cp.fk_ProjectUID
              FROM CustomerProject cp 
              WHERE cp.fk_ProjectUID = @ProjectUID",
        new {ProjectUID = projectUid}
      )).FirstOrDefault();


      return !string.IsNullOrEmpty(uid);
    }

    /// <summary>
    ///     for unit tests - so we don't have to create everything (associations) for a test
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Abstractions.Models.DatabaseModels.Project> GetProject_UnitTest(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                  p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                  p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                  p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                  cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                  ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                LEFT JOIN CustomerProject cp ON p.ProjectUID = cp.fk_ProjectUID
                LEFT JOIN Customer c ON c.CustomerUID = cp.fk_CustomerUID
                LEFT JOIN ProjectSubscription ps on p.ProjectUID = ps.fk_ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.ProjectUID = @ProjectUID",
        new {ProjectUID = projectUid}
      )).FirstOrDefault();


      return project;
    }

    /// <summary>
    /// Gets the list of geofence UIDs associated wih the specified project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>List of associations</returns>
    public Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid)
    {
      return QueryWithAsyncPolicy<ProjectGeofence>
      (@"SELECT 
                fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, pg.LastActionedUTC, g.fk_GeofenceTypeID AS GeofenceType 
              FROM ProjectGeofence pg
                LEFT OUTER JOIN Geofence g on g.GeofenceUID = pg.fk_GeofenceUID
              WHERE fk_ProjectUID = @ProjectUID",
        new {ProjectUID = projectUid}
      );
    }

    /// <summary>
    /// Gets the list of geofence UIDs for the customer, along with any potential projectUid association
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns>List of geofences and potential ProjectUid</returns>
    public Task<IEnumerable<GeofenceWithAssociation>> GetCustomerGeofences(string customerUid)
    {
      return QueryWithAsyncPolicy<GeofenceWithAssociation>
      (@"SELECT 
                g.GeofenceUID, g.Name, g.fk_GeofenceTypeID AS GeofenceType, AsWKT(g.PolygonST) as GeometryWKT, g.FillColor, g.IsTransparent,
                g.IsDeleted, g.Description, g.fk_CustomerUID AS CustomerUID, g.UserUID, g.AreaSqMeters,
                g.LastActionedUTC, pg.fk_ProjectUID AS ProjectUID 
              FROM Geofence g 
                LEFT OUTER JOIN ProjectGeofence pg on pg.fk_GeofenceUID = g.GeofenceUID 
              WHERE fk_CustomerUID = @CustomerUID 
                AND g.IsDeleted = 0",
        new {CustomerUID = customerUid}
      );
    }

    #endregion gettersProject


    #region gettersProjectSettings

    /// <summary>
    /// At this stage 2 types
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <param name="projectSettingsType"></param>
    /// <returns></returns>
    public async Task<ProjectSettings> GetProjectSettings(string projectUid, string userId,
      ProjectSettingsType projectSettingsType)
    {
      return (await QueryWithAsyncPolicy<ProjectSettings>(@"SELECT 
                fk_ProjectUID AS ProjectUid, fk_ProjectSettingsTypeID AS ProjectSettingsType, Settings, UserID, LastActionedUTC
              FROM ProjectSettings
              WHERE fk_ProjectUID = @ProjectUid
                AND UserID = @UserID
                AND fk_ProjectSettingsTypeID = @ProjectSettingsType
              ORDER BY fk_ProjectUID, UserID, fk_ProjectSettingsTypeID",
          new {ProjectUid = projectUid, UserID = userId, ProjectSettingsType = projectSettingsType}))
        .FirstOrDefault();
    }

    /// <summary>
    /// At this stage 2 types, user must eval result
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<IEnumerable<ProjectSettings>> GetProjectSettings(string projectUid, string userId)
    {
      return QueryWithAsyncPolicy<ProjectSettings>
      (@"SELECT 
                fk_ProjectUID AS ProjectUid, fk_ProjectSettingsTypeID AS ProjectSettingsType, Settings, UserID, LastActionedUTC
              FROM ProjectSettings
              WHERE fk_ProjectUID = @ProjectUid
                AND UserID = @UserID",
        new {ProjectUid = projectUid, UserID = userId}
      );
    }

    #endregion gettersProjectSettings


    #region gettersImportedFiles

    public async Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid)
    {
      var importedFileList = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType,
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID as ParentUID 
          FROM ImportedFile
            WHERE fk_ProjectUID = @ProjectUid
              AND IsDeleted = 0",
        new {ProjectUid = projectUid}
      )).ToList();

      var historyAllFiles = await GetImportedFileHistory(projectUid);
      foreach (var importedFile in importedFileList)
      {
        var historyOne = historyAllFiles.FindAll(x => x.ImportedFileUid == importedFile.ImportedFileUid);
        if (historyOne.Any())
        {
          importedFile.ImportedFileHistory = new ImportedFileHistory(historyOne);
        }
      }

      return importedFileList;
    }

    public async Task<ImportedFile> GetImportedFile(string importedFileUid)
    {
      var importedFile = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType, 
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID as ParentUID
          FROM ImportedFile
            WHERE importedFileUID = @ImportedFileUid",
        new {ImportedFileUid = importedFileUid}
      )).FirstOrDefault();

      if (importedFile != null)
      {
        var historyAllFiles = await GetImportedFileHistory(importedFile.ProjectUid, importedFileUid);
        if (historyAllFiles.Any())
        {
          importedFile.ImportedFileHistory = new ImportedFileHistory(historyAllFiles);
        }
      }

      return importedFile;
    }

    private async Task<List<ImportedFileHistoryItem>> GetImportedFileHistory(string projectUid,
      string importedFileUid = null)
    {
      return (await QueryWithAsyncPolicy<ImportedFileHistoryItem>
      (@"SELECT 
              ImportedFileUID, ifh.FileCreatedUTC, ifh.FileUpdatedUTC, ifh.ImportedBy
            FROM ImportedFile iff
              INNER JOIN ImportedFileHistory ifh ON ifh.fk_ImportedFileUID = iff.ImportedFileUID
            WHERE fk_ProjectUID = @projectUid
              AND IsDeleted = 0
              AND (@ImportedFileUid IS NULL OR ImportedFileUID = @ImportedFileUid)
            ORDER BY ImportedFileUID, ifh.FileUpdatedUTC",
        new {projectUid, ImportedFileUid = importedFileUid}
      )).ToList();
    }

    #endregion gettersImportedFiles


    #region gettersSpatial

    /// <summary>
    ///     Gets any standard project which the lat/long is within,
    ///     which satisfies all conditions for the asset
    /// </summary>
    /// <param name="customerUID"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="timeOfPosition"></param>
    /// <returns>The project</returns>
    public async Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetStandardProject(string customerUID, double latitude,
      double longitude, DateTime timeOfPosition)
    {
      var point = $"ST_GeomFromText('POINT({longitude} {latitude})')";
      var select = "SELECT DISTINCT " +
                   "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
                   "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT, " +
                   "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
                   "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID " + "      FROM Project p " +
                   "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
                   "      WHERE p.fk_ProjectTypeID = 0 " + "        AND p.IsDeleted = 0 " +
                   "        AND @timeOfPosition BETWEEN p.StartDate AND p.EndDate " +
                   "        AND cp.fk_CustomerUID = @CustomerUID " +
                   $"        AND st_Intersects({point}, PolygonST) = 1";

      var projects =
        await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(select,
          new {CustomerUID = customerUID, timeOfPosition = timeOfPosition.Date});

      return projects;
    }

    /// <summary>
    ///     Gets any ProjectMonitoring or Landfill (as requested) project which the lat/long is within,
    ///     which satisfies all conditions for the tccOrgid
    ///     note that project can be backfilled i.e.set to a date earlier than the serviceView
    /// </summary>
    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjectMonitoringProject(string customerUID,
      double latitude, double longitude, DateTime timeOfPosition, int projectType, int serviceType)
    {
      var point = $"ST_GeomFromText('POINT({longitude} {latitude})')";
      var select = "SELECT DISTINCT " +
                   "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
                   "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT, " +
                   "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
                   "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, " +
                   "        ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID " +
                   "      FROM Project p " +
                   "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
                   "        INNER JOIN ProjectSubscription ps ON ps.fk_ProjectUID = cp.fk_ProjectUID " +
                   "        INNER JOIN Subscription s ON s.SubscriptionUID = ps.fk_SubscriptionUID " +
                   "      WHERE p.fk_ProjectTypeID = @ProjectType " + "        AND p.IsDeleted = 0 " +
                   "        AND @timeOfPosition BETWEEN p.StartDate AND p.EndDate " +
                   "        AND @timeOfPosition <= s.EndDate " + "        AND s.fk_ServiceTypeID = @serviceType " +
                   "        AND cp.fk_CustomerUID = @CustomerUID " +
                   $"        AND st_Intersects({point}, PolygonST) = 1";

      return QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(select,
        new {CustomerUID = customerUID, timeOfPosition = timeOfPosition.Date, ProjectType = projectType, serviceType});
    }

    /// <summary>
    ///     Gets any project which
    ///     1) for this Customer
    ///     2) is active at the time
    ///     3) the lat/long is within,
    ///     4) but ignore the project if it's an update
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="geometryWkt"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="excludeProjectUid"></param>
    /// <returns>The project</returns>
    public async Task<bool> DoesPolygonOverlap(string customerUid, string geometryWkt, DateTime startDate,
      DateTime endDate, string excludeProjectUid = "")
    {
      string polygonToCheck = RepositoryHelper.WKTToSpatial(geometryWkt);

      var select = $@"SELECT DISTINCT
                          p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                          p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                          p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                          cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID
                        FROM Project p 
                          INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                        WHERE p.IsDeleted = 0
                          AND @StartDate <= p.EndDate
                          AND @EndDate >= p.StartDate
                          AND cp.fk_CustomerUID = @CustomerUID
                          AND p.ProjectUid != @excludeProjectUid
                          AND st_Intersects({polygonToCheck}, PolygonST) = 1";

      return (await QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(select,
          new {CustomerUID = customerUid, StartDate = startDate.Date, EndDate = endDate.Date, excludeProjectUid}))
        .Any();
    }

    /// <summary>
    ///     Gets any project for the customer
    ///     which the lat/long is within
    ///       optionally can check for a) subset of projectTypes and b) within time 
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="projectTypes"></param>
    /// <param name="timeOfPosition"></param>
    /// <returns>The project</returns>
    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetIntersectingProjects(string customerUid,
      double latitude, double longitude, int[] projectTypes, DateTime? timeOfPosition)
    {
      var point = $"ST_GeomFromText('POINT({longitude} {latitude})')";
      var projectTypesString = string.Empty;
      if (projectTypes.Any())
      {
        projectTypesString += " AND p.fk_ProjectTypeID IN ( ";
        for (int i = 0; i < projectTypes.Length; i++)
        {
          projectTypesString += projectTypes[i] + ((i < projectTypes.Length - 1) ? "," : "");
        }

        projectTypesString += " ) ";
      }

      var timeRangeString = string.Empty;
      if (timeOfPosition != null)
      {
        var formattedDate = (timeOfPosition.Value.Date.ToString("yyyy-MM-dd"));
        timeRangeString = $"  AND '{formattedDate}' BETWEEN p.StartDate AND p.EndDate ";
      }

      var select = "SELECT DISTINCT " +
                   "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
                   "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT, " +
                   "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
                   "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, " +
                   "        ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID " +
                   "      FROM Project p " +
                   "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
                   "        LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID " +
                   "        LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID " +
                   "       WHERE     p.IsDeleted = 0 " +
                   $"        AND cp.fk_CustomerUID = '{customerUid}' " +
                   $"       {projectTypesString} " +
                   $"       {timeRangeString} " +
                   $"        AND st_Intersects({point}, PolygonST) = 1";

      return QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>(select);
    }

    #endregion gettersSpatial

    public Task<IEnumerable<Abstractions.Models.DatabaseModels.Project>> GetProjects_UnitTests()
    {
      return QueryWithAsyncPolicy<Abstractions.Models.DatabaseModels.Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, AsWKT(p.PolygonST) as GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.IsDeleted = 0"
      );
    }

  }




}
