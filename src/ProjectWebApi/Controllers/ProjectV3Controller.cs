﻿using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.GenericConfiguration;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.ProjectWebApi.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.Utilities;
using VSS.Productivity3D.Repo;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.ProjectWebApi.Controllers
{
  /// <summary>
  /// Project controller v3
  /// </summary>
  public class ProjectV3Controller : ProjectBaseController
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectV3Controller"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="subscriptionsRepo">The subscriptions repo.</param>
    /// <param name="store">The store.</param>
    /// <param name="subsProxy">The subs proxy.</param>
    /// <param name="geofenceProxy">The geofence proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceExceptionHandler">The ServiceException handler</param>
    public ProjectV3Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
      : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, logger, serviceExceptionHandler)
    { }

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    /// and both active and archived projects.
    /// </summary>
    /// <returns>A list of projects</returns>
    [Route("api/v3/project")]
    [HttpGet]
    public async Task<ImmutableDictionary<int, ProjectDescriptor>> GetProjectsV3()
    {
      log.LogInformation("GetProjectsV3");
      var projects = (await GetProjectList());
      var customerUid = LogCustomerDetails("GetProjectsV3");


      return projects.Where(p => p.CustomerUID == customerUid).ToImmutableDictionary(key => key.LegacyProjectID,
        project =>
          new ProjectDescriptor()
          {
            ProjectType = project.ProjectType,
            Name = project.Name,
            ProjectTimeZone = project.ProjectTimeZone,
            IsArchived = project.IsDeleted || project.SubscriptionEndDate < DateTime.UtcNow,
            StartDate = project.StartDate.ToString("O"),
            EndDate = project.EndDate.ToString("O"),
            ProjectUid = project.ProjectUID,
            LegacyProjectId = project.LegacyProjectID,
            ProjectGeofenceWKT = project.GeometryWKT,
            CustomerUID = project.CustomerUID,
            LegacyCustomerId = project.LegacyCustomerID.ToString(),
            CoordinateSystemFileName = project.CoordinateSystemFileName
          }
      );
    }

    // POST: api/project
    /// <summary>
    /// Create Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v3/project")]
    [HttpPost]
    public async Task CreateProjectV3([FromBody] CreateProjectEvent project)
    {
      log.LogInformation("CreateProjectV3. project: {0}", JsonConvert.SerializeObject(project));
      ProjectBoundaryValidator.ValidateWKT(project.ProjectBoundary);
      string wktBoundary = project.ProjectBoundary;

      //Convert to old format for Kafka for consistency on kakfa queue
      string kafkaBoundary = project.ProjectBoundary
        .Replace(ProjectBoundaryValidator.POLYGON_WKT, string.Empty)
        .Replace("))", string.Empty)
        .Replace(',', ';')
        .Replace(' ', ',');
      await CreateProject(project, kafkaBoundary, wktBoundary);
    }

    // PUT: api/Project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="project">UpdateProjectEvent model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v3/project")]
    [HttpPut]
    public async Task UpdateProjectV3([FromBody] UpdateProjectEvent project)
    {
      log.LogInformation("UpdateProjectV3. project: {0}", JsonConvert.SerializeObject(project));

      await UpdateProject(project);
    }

    // DELETE: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="project">DeleteProjectEvent model</param>
    /// <remarks>Deletes existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v3/project")]
    [HttpDelete]
    public async Task DeleteProjectV3([FromBody] DeleteProjectEvent project)
    {
      log.LogInformation("DeleteProjectV3. project: {0}", JsonConvert.SerializeObject(project));

      await DeleteProject(project);
    }

    /// <summary>
    /// Associate customer and project
    /// </summary>
    /// <param name="customerProject">Customer - project</param>
    /// <remarks>Associate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("api/v3/project/AssociateCustomer")]
    [HttpPost]
    public async Task AssociateCustomerProjectV3([FromBody] AssociateProjectCustomer customerProject)
    {
      if (customerProject.LegacyCustomerID <= 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 38);
      }
      await AssociateProjectCustomer(customerProject);
    }

    /// <summary>
    /// Dissociate customer and asset
    /// </summary>
    /// <param name="customerProject">Customer - Project model</param>
    /// <remarks>Dissociate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("api/v3/project/DissociateCustomer")]
    [HttpPost]
    public async Task DissociateCustomerProjectV3([FromBody] DissociateProjectCustomer customerProject)
    {
      await DissociateProjectCustomer(customerProject);
    }

    /// <summary>
    /// Associate geofence and project
    /// </summary>
    /// <param name="geofenceProject">Geofence - project</param>
    /// <remarks>Associate geofence and project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("api/v3/project/AssociateGeofence")]
    [HttpPost]
    public async Task AssociateGeofenceProjectV3([FromBody] AssociateProjectGeofence geofenceProject)
    {
      await AssociateGeofenceProject(geofenceProject);
    }

    #region private

    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats. this method can be overriden
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="kafkaProjectBoundary">The project boundary in the old format (coords comma separated, points semicolon separated)</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    private async Task CreateProject(CreateProjectEvent project, string kafkaProjectBoundary, string databaseProjectBoundary)
    {
      ProjectDataValidator.Validate(project, projectService);
      if (project.ProjectID <= 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 44);

      project.ReceivedUTC = DateTime.UtcNow;

      //Save boundary as WKT
      project.ProjectBoundary = databaseProjectBoundary;
      var isCreated = await projectService.StoreEvent(project).ConfigureAwait(false);
      if (isCreated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61);

      //Send boundary as old format on kafka queue
      project.ProjectBoundary = kafkaProjectBoundary;
      var messagePayload = JsonConvert.SerializeObject(new { CreateProjectEvent = project });
      await producer.Send(kafkaTopicName,
        new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload));
    }

    /// <summary>
    /// Updates the project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    private async Task UpdateProject(UpdateProjectEvent project)
    {
      ProjectDataValidator.Validate(project, projectService);
      project.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectService.StoreEvent(project).ConfigureAwait(false);
      if (isUpdated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62);

      var messagePayload = JsonConvert.SerializeObject(new { UpdateProjectEvent = project });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
    }

    /// <summary>
    /// Associates the project customer.
    /// </summary>
    /// <param name="customerProject">The customer project.</param>
    /// <returns></returns>
    private async Task AssociateProjectCustomer(AssociateProjectCustomer customerProject)
    {
      ProjectDataValidator.Validate(customerProject, projectService);
      customerProject.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectService.StoreEvent(customerProject).ConfigureAwait(false);
      if (isUpdated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 63);

      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectCustomer = customerProject });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
        });
    }

    /// <summary>
    /// Dissociates the project customer. this actually deletes the link.
    /// </summary>
    /// <param name="customerProject">The customer project.</param>
    /// <returns></returns>
    private async Task DissociateProjectCustomer(DissociateProjectCustomer customerProject)
    {
      ProjectDataValidator.Validate(customerProject, projectService);
      customerProject.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectService.StoreEvent(customerProject).ConfigureAwait(false);
      if (isUpdated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 64);

      var messagePayload = JsonConvert.SerializeObject(new { DissociateProjectCustomer = customerProject });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
        });
    }

    /// <summary>
    /// Associates the geofence project.
    /// </summary>
    /// <param name="geofenceProject">The geofence project.</param>
    /// <returns></returns>
    private async Task AssociateGeofenceProject(AssociateProjectGeofence geofenceProject)
    {
      ProjectDataValidator.Validate(geofenceProject, projectService);
      geofenceProject.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectService.StoreEvent(geofenceProject).ConfigureAwait(false);
      if (isUpdated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 65);

      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectGeofence = geofenceProject });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(geofenceProject.ProjectUID.ToString(), messagePayload)
        });
    }

    /// <summary>
    /// Deletes the project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    private async Task DeleteProject(DeleteProjectEvent project)
    {
      ProjectDataValidator.Validate(project, projectService);
      project.ReceivedUTC = DateTime.UtcNow;

      var isDeleted = await projectService.StoreEvent(project).ConfigureAwait(false);
      if (isDeleted == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
    }

    #endregion private
  }
}