﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ProjectGeofenceExecutorTests : IClassFixture<ExecutorBaseTests>
  {
    private readonly ExecutorBaseTests _testFixture;
    private readonly ProjectErrorCodesProvider _projectErrorCodesProvider;

    private static string _validBoundary =
      "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";

    private T GetDIService<T>() => _testFixture.ServiceProvider.GetRequiredService<T>();

    public ProjectGeofenceExecutorTests(ExecutorBaseTests testFixture)
    {
      _testFixture = testFixture;
      _projectErrorCodesProvider = new ProjectErrorCodesProvider();
    }

    [Fact]
    public async Task ProjectGeofence_HappyPath_OneNewGeofenceAssociation()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel
                    {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>
                      {
        Guid.Parse(testGeofencesForCustomer[0].GeofenceUID),
        Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)
      };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = _testFixture.ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = _testFixture.ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = _testFixture.ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, _testFixture.KafkaTopicName,
        null, null, null, null, null,
        projectRepo.Object);
      await executor.ProcessAsync(request);
    }

    [Fact]
    public async Task ProjectGeofence_Error_InvalidProjectType()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel
                    {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard
      };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>
                      {
        Guid.Parse(testGeofencesForCustomer[0].GeofenceUID),
        Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)
      };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = _testFixture.ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = _testFixture.ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = _testFixture.ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, _testFixture.KafkaTopicName,
        null, null, null, null, null,
        projectRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(102), StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProjectGeofence_Error_ExistingAssociationMissingFromList()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel
                    {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid> { Guid.Parse(testGeofencesForCustomer[0].GeofenceUID) };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = _testFixture.ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = _testFixture.ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = _testFixture.ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, _testFixture.KafkaTopicName,
        null, null, null, null, null,
        projectRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(107), StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProjectGeofence_Error_NoNewAssociationsToAdd()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel
                    {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid> { Guid.Parse(testGeofencesForCustomer[2].GeofenceUID) };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = GetDIService<IConfigurationStore>();
      var logger = GetDIService<ILoggerFactory>();
      var serviceExceptionHandler = GetDIService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, _testFixture.KafkaTopicName,
        null, null, null, null, null,
        projectRepo.Object);
      await executor.ProcessAsync(request);
    }

    [Fact]
    public async Task ProjectGeofence_Error_ProjectNotFound()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(new List<ProjectDatabaseModel>());
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>
                      {
        Guid.Parse(testGeofencesForCustomer[0].GeofenceUID),
        Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)
      };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = GetDIService<IConfigurationStore>();
      var logger = GetDIService<ILoggerFactory>();
      var serviceExceptionHandler = GetDIService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, _testFixture.KafkaTopicName,
        null, null, null, null, null,
        projectRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(1), StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProjectGeofence_Error_GeofenceUidNotInDatabaseForCustomer()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid> { Guid.NewGuid(), Guid.Parse(testGeofencesForCustomer[2].GeofenceUID) };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = GetDIService<IConfigurationStore>();
      var logger = GetDIService<ILoggerFactory>();
      var serviceExceptionHandler = GetDIService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, _testFixture.KafkaTopicName,
        null, null, null, null, null,
        projectRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(104), StringComparison.Ordinal));
    }

    [Fact]
    public async Task Get_UnassignedLandfillGeofencesAsync()
    {
      var log = GetDIService<ILoggerFactory>().CreateLogger<ProjectGeofenceValidationTests>();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      var geofences = await ProjectRequestHelper
        .GetGeofenceList(customerUid, string.Empty, geofenceTypes, log, projectRepo.Object)
        .ConfigureAwait(false);

      Assert.Single(geofences);

      Assert.Equal(testGeofencesForCustomer[0].GeofenceUID, geofences[0].GeofenceUID);
      Assert.Equal(testGeofencesForCustomer[0].Name, geofences[0].Name);
      Assert.Equal(testGeofencesForCustomer[0].GeofenceType, geofences[0].GeofenceType);
      Assert.Equal(testGeofencesForCustomer[0].GeometryWKT, geofences[0].GeometryWKT);
      Assert.Equal(testGeofencesForCustomer[0].FillColor, geofences[0].FillColor);
      Assert.Equal(testGeofencesForCustomer[0].IsTransparent, geofences[0].IsTransparent);
      Assert.Equal(testGeofencesForCustomer[0].Description, geofences[0].Description);
      Assert.Equal(testGeofencesForCustomer[0].CustomerUID, geofences[0].CustomerUID);
      Assert.Equal(testGeofencesForCustomer[0].UserUID, geofences[0].UserUID);
      Assert.Equal(testGeofencesForCustomer[0].AreaSqMeters, geofences[0].AreaSqMeters);
    }

    [Fact]
    public async Task Get_AssignedLandfillGeofences_FromProject()
    {
      var log = GetDIService<ILoggerFactory>().CreateLogger<ProjectGeofenceValidationTests>();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      var geofences = await ProjectRequestHelper
        .GetGeofenceList(customerUid, projectUid, geofenceTypes, log, projectRepo.Object)
        .ConfigureAwait(false);

      Assert.Single(geofences);

      Assert.Equal(testGeofencesForCustomer[2].GeofenceUID, geofences[0].GeofenceUID);
      Assert.Equal(testGeofencesForCustomer[2].Name, geofences[0].Name);
      Assert.Equal(testGeofencesForCustomer[2].GeofenceType, geofences[0].GeofenceType);
      Assert.Equal(testGeofencesForCustomer[2].GeometryWKT, geofences[0].GeometryWKT);
      Assert.Equal(testGeofencesForCustomer[2].FillColor, geofences[0].FillColor);
      Assert.Equal(testGeofencesForCustomer[2].IsTransparent, geofences[0].IsTransparent);
      Assert.Equal(testGeofencesForCustomer[2].Description, geofences[0].Description);
      Assert.Equal(testGeofencesForCustomer[2].CustomerUID, geofences[0].CustomerUID);
      Assert.Equal(testGeofencesForCustomer[2].UserUID, geofences[0].UserUID);
      Assert.Equal(testGeofencesForCustomer[2].AreaSqMeters, geofences[0].AreaSqMeters);
    }

    private List<GeofenceWithAssociation> CreateGeofenceWithAssociations(string customerUid, string projectUid)
    {
      var geofencesWithAssociation = new List<GeofenceWithAssociation>
                                     {
        new GeofenceWithAssociation
        {
          CustomerUID = customerUid,
          Name = "geofence Name",
          Description = "geofence Description",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 12.45
        },
        new GeofenceWithAssociation
        {
          CustomerUID = customerUid,
          Name = "geofence Name2",
          Description = "geofence Description2",
          GeofenceType = GeofenceType.Project,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 223.45
        },
        new GeofenceWithAssociation
        {
          CustomerUID = customerUid,
          Name = "geofence Name3",
          Description = "geofence Description3",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45,
          ProjectUID = projectUid
        },
        new GeofenceWithAssociation
        {
          CustomerUID = customerUid,
          Name = "geofence Name4",
          Description = "geofence Description4",
          GeofenceType = GeofenceType.CutZone,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45
        }
      };
      return geofencesWithAssociation;
    }
  }
}
