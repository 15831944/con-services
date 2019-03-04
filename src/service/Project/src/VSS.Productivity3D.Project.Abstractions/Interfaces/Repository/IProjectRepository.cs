﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces.Repository
{
  public interface IProjectRepository
  {
    Task<bool> ProjectExists(string projectUid);
    Task<bool> CustomerProjectExists(string projectUid);
    Task<ImportedFile> GetImportedFile(string importedFileUid);
    Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid);
    Task<Models.DatabaseModels.Project> GetProject(long legacyProjectID);
    Task<Models.DatabaseModels.Project> GetProject(string projectUid);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectAndSubscriptions(long legacyProjectID, DateTime validAtDate);
    Task<Models.DatabaseModels.Project> GetProjectBySubcription(string subscriptionUid);
    Task<Models.DatabaseModels.Project> GetProjectOnly(string projectUid);
    Task<ProjectSettings> GetProjectSettings(string projectUid, string userId, ProjectSettingsType projectSettingsType);
    Task<IEnumerable<ProjectSettings>> GetProjectSettings(string projectUid, string userId);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectsForCustomer(string customerUid);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectsForCustomerUser(string customerUid, string userUid);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectsForUser(string userUid);
    Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid);
    Task<IEnumerable<GeofenceWithAssociation>> GetCustomerGeofences(string customerUid);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjects_UnitTests();
    Task<Models.DatabaseModels.Project> GetProject_UnitTest(string projectUid);


    Task<bool> DoesPolygonOverlap(string customerUid, string geometryWkt, DateTime startDate,
      DateTime endDate, string excludeProjectUid = "");
    Task<IEnumerable<Models.DatabaseModels.Project>> GetStandardProject(string customerUID, double latitude, double longitude,
      DateTime timeOfPosition);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectMonitoringProject(string customerUID, double latitude, double longitude,
      DateTime timeOfPosition, int projectType, int serviceType);
    Task<IEnumerable<Models.DatabaseModels.Project>> GetIntersectingProjects(string customerUid, double latitude, double longitude,
      int[] projectTypes, DateTime? timeOfPosition = null);
    
    Task<int> StoreEvent(IProjectEvent evt);
  }
}