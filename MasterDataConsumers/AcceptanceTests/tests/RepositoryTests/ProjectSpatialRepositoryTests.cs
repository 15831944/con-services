﻿using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories;
using RepositoryTests.Internal;
using System;
using System.Linq;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectSpatialRepositoryTests
  {
    IServiceProvider serviceProvider;
    CustomerRepository customerContext;
    ProjectRepository projectContext;
    SubscriptionRepository subscriptionContext;

    [TestInitialize]
    public void Init()
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      customerContext = new CustomerRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      projectContext = new ProjectRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      subscriptionContext = new SubscriptionRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
    }


    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointInsideStandardProjectBoundary()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var g = projectContext.GetStandardProject(createCustomerEvent.CustomerUID.ToString(), 15, 180, createProjectEvent.ProjectStartDate.AddDays(1)); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(1, g.Result.Count(), "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(projects.ToList()[0].ProjectUID, createProjectEvent.ProjectUID.ToString(), "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointOutsideStandardProjectBoundary()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var g = projectContext.GetStandardProject(createCustomerEvent.CustomerUID.ToString(), 50, 180, createProjectEvent.ProjectStartDate.AddDays(1)); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(0, g.Result.Count(), "Should be no Projects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointOnPointStandardProjectBoundary()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var g = projectContext.GetStandardProject(createCustomerEvent.CustomerUID.ToString(), 40, 170, createProjectEvent.ProjectStartDate.AddDays(1)); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(1, g.Result.Count(), "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(projects.ToList()[0].ProjectUID, createProjectEvent.ProjectUID.ToString(), "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointInsidePMProjectBoundary()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.ProjectMonitoring,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent).Wait();

      var g = projectContext.GetProjectMonitoringProject(createCustomerEvent.CustomerUID.ToString(), 15, 180,
        createProjectEvent.ProjectStartDate.AddDays(1),
        (int)createProjectEvent.ProjectType, subscriptionContext._serviceTypes[createProjectSubscriptionEvent.SubscriptionType].ID); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(1, g.Result.Count(), "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(projects.ToList()[0].ProjectUID, createProjectEvent.ProjectUID.ToString(), "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Point is outside the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointOutsidePMProjectBoundary()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.ProjectMonitoring,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent).Wait();

      var g = projectContext.GetProjectMonitoringProject(createCustomerEvent.CustomerUID.ToString(), 50, 180,
        createProjectEvent.ProjectStartDate.AddDays(1),
        (int)createProjectEvent.ProjectType, subscriptionContext._serviceTypes[createProjectSubscriptionEvent.SubscriptionType].ID); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(0, g.Result.Count(), "Should not retrieve Project from ProjectRepo");
    }

    /// <summary>
    /// Point is outside the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointOnPMProjectBoundary()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.ProjectMonitoring,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent).Wait();

      var g = projectContext.GetProjectMonitoringProject(createCustomerEvent.CustomerUID.ToString(), 15, 190,
        createProjectEvent.ProjectStartDate.AddDays(1),
        (int)createProjectEvent.ProjectType, subscriptionContext._serviceTypes[createProjectSubscriptionEvent.SubscriptionType].ID); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(1, g.Result.Count(), "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(projects.ToList()[0].ProjectUID, createProjectEvent.ProjectUID.ToString(), "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [TestMethod]
    public void PointInsidePMProjectBoundary_WrongServiceType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.ProjectMonitoring,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent).Wait();

      var g = projectContext.GetProjectMonitoringProject(createCustomerEvent.CustomerUID.ToString(), 15, 180,
        createProjectEvent.ProjectStartDate.AddDays(1),
        (int)createProjectEvent.ProjectType, subscriptionContext._serviceTypes["Landfill"].ID); g.Wait();
      var projects = g.Result;
      Assert.IsNotNull(g.Result, "Unable to call ProjectRepo");
      Assert.AreEqual(0, g.Result.Count(), "Should be no Projects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon is within (internal) an existing projectboundary
    ///    and time is within
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_InternalBoundaryInternalTime()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      string testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var testCustomerUID = createCustomerEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsTrue(g.Result, "Should be overlappingProjects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon is not within an existing projectboundary
    ///    but time is overlapping
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_InternalBoundaryExternalTime()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      string testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var testCustomerUID = createCustomerEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectEndDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectEndDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsFalse(g.Result, "Should be no overlappingProjects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon is not within an existing projectboundary
    ///    but time is within
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_ExternalBoundaryInternalTime()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 200 20, 200 10))";
      var testCustomerUID = createCustomerEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsFalse(g.Result, "Should not be overlappingProjects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon is completely overlapping an existing projectboundary
    ///    and time is within
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_OverlappingBoundaryInternalTime()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      string testBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testCustomerUID = createCustomerEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsTrue(g.Result, "Should be overlappingProjects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon touches at a point an existing projectboundary
    ///    and time is within
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_TouchingBoundaryInternalTime()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 190 20, 200 10))";
      var testCustomerUID = createCustomerEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsTrue(g.Result, "Should be overlappingProjects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon overlaps but no internal points, an existing projectboundary
    ///    and time is within
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_OverlapExternalBoundaryInternalTime()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 175 45, 200 10))";
      var testCustomerUID = createCustomerEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsTrue(g.Result, "Should be overlappingProjects retrieved from ProjectRepo");
    }

    /// <summary>
    /// Polygon is within (internal) an existing projectboundary
    ///    and time is within
    /// </summary>
    [TestMethod]
    public void PolygonIntersection_InternalBoundaryInternalTimeDifferentCustomer()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var testCustomerUID = Guid.NewGuid();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectContext.DoesPolygonOverlap(testCustomerUID.ToString(), testBoundary, testStartDate, testEndDate); g.Wait();
      var projects = g.Result;
      Assert.IsFalse(g.Result, "Should be no overlappingProjects retrieved from ProjectRepo");
    }
  }
}