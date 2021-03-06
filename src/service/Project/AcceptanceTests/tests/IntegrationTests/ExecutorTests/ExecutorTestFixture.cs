﻿using System;
using System.Threading.Tasks;
using CCSS.CWS.Client;
using CCSS.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using TestUtility;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.ExecutorTests
{
  public class ExecutorTestFixture : IDisposable
  {
    private readonly IServiceProvider _serviceProvider;
    public static IConfigurationStore ConfigStore;
    public static ILoggerFactory Logger;
    public static IServiceExceptionHandler ServiceExceptionHandler;
    public static ProjectRepository ProjectRepo;
    public static ICwsProjectClient CwsProjectClient;
    public static IProductivity3dV2ProxyCompaction Productivity3dV2ProxyCompaction;

    public ExecutorTestFixture()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("IntegrationTests.ExecutorTests.log", null));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
        .AddSingleton(loggerFactory)
        .AddHttpClient()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<ICwsProjectClient, CwsProjectClient>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()

        // for serviceDiscovery
        .AddServiceDiscovery()
        .AddTransient<IWebRequest, GracefulWebRequest>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()

        .AddTransient<IProductivity3dV1ProxyCoord, Productivity3dV1ProxyCoord>()
        .AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>()
        .AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();


      _serviceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = _serviceProvider.GetRequiredService<IConfigurationStore>();
      Logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
      ServiceExceptionHandler = _serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      ProjectRepo = _serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      CwsProjectClient = _serviceProvider.GetRequiredService<ICwsProjectClient>();
      Productivity3dV2ProxyCompaction = _serviceProvider.GetRequiredService<IProductivity3dV2ProxyCompaction>();
    }

    public static IHeaderDictionary CustomHeaders(string customerUid)
    {
      return new HeaderDictionary
      {
        { "X-JWT-Assertion", RestClient.DEFAULT_JWT },
        { "X-VisionLink-CustomerUid", customerUid },
        { "X-VisionLink-ClearCache", "true" }
      };
    }


    public static async Task<CreateProjectResponseModel> CreateCustomerProject(string customerUid, string name = "woteva",
      string boundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))")
    {
      var createProjectRequestModel = new CreateProjectRequestModel
      {
        AccountId = customerUid,
        ProjectName = name,
        Boundary = GeometryConversion.MapProjectBoundary(boundary)
      };

      var response = await CwsProjectClient.CreateProject(createProjectRequestModel);
      return response;
    }
    
    public static bool CreateProjectSettings(string projectUid, string userId, string settings, ProjectSettingsType settingsType)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = new Guid(projectUid),
        UserID = userId,
        Settings = settings,
        ProjectSettingsType = settingsType,
        ActionUTC = actionUtc
      };
      Console.WriteLine($"Create project settings event created");
      Console.WriteLine(
          $"UpdateProjectSettingsEvent ={JsonConvert.SerializeObject(createProjectSettingsEvent)}))')");

      var projectEvent = createProjectSettingsEvent;
      var projectSettings = new ProjectSettings
      {
        ProjectUid = projectEvent.ProjectUID.ToString(),
        ProjectSettingsType = projectEvent.ProjectSettingsType,
        Settings = projectEvent.Settings,
        UserID = projectEvent.UserID,
        LastActionedUtc = projectEvent.ActionUTC
      };

      Console.WriteLine(
        $"projectSettings after cast/convert ={JsonConvert.SerializeObject(projectSettings)}))')");
      ProjectRepo.StoreEvent(createProjectSettingsEvent).Wait();
      var g = ProjectRepo.GetProjectSettings(projectUid, userId, settingsType); g.Wait();
      return (g.Result != null ? true : false);
    }

    public void Dispose()
    { }
  }

  [CollectionDefinition("Service collection")]
  public class CollectionFixure : ICollectionFixture<ExecutorTestFixture>
  {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
  }
}
