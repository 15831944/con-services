﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using log4netExtensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using MasterDataConsumer;
using VSS.Device.Data;
using VSS.Asset.Data;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Customer.Data;
using VSS.Geofence.Data;
using VSS.Project.Data;
using VSS.Project.Service.Repositories;

namespace RepositoryTests
{
  [TestClass]
  public class AssetRepositoryTests
  {
    IServiceProvider serviceProvider = null;
    DeviceRepository deviceContext = null;
    AssetRepository assetContext = null;
    IRepositoryFactory factory = null;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddSingleton<IRepositoryFactory, RepositoryFactory>()
          .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      factory = serviceProvider.GetRequiredService<IRepositoryFactory>();      

      assetContext = factory.GetRepository<IAssetEvent>() as AssetRepository;
      deviceContext = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
    }

    /// <summary>
    /// This is used in GetAssetId, taking a radioSerial + deviceType and returning an AssetDeviceId class    ///   
    /// </summary>
    [TestMethod]
    public void CanAssociateAnAssetWithDevice()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = 34457644576;
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      string deviceType = "woteva";
      var result = CreateAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType);
      Assert.IsTrue(result, "created assetDevice association");
      var g = deviceContext.GetAssociatedAsset(deviceSerialNumber, deviceType); g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve assetDevice association");
      Assert.AreEqual(assetUID.ToString(), g.Result.AssetUID, "AssetUID is incorrect");
      Assert.AreEqual(deviceType, g.Result.DeviceType, "DeviceType is incorrect");
      Assert.AreEqual(deviceUID.ToString(), g.Result.DeviceUID, "DeviceUID is incorrect");
      Assert.AreEqual(legacyAssetId, g.Result.LegacyAssetID, "LegacyAssetID is incorrect");
      Assert.AreEqual(owningCustomerUID.ToString(), g.Result.OwningCustomerUID, "OwningCustomerUID is incorrect");
      Assert.AreEqual(deviceSerialNumber, g.Result.RadioSerial, "DeviceSerialNumber is incorrect");
    }


 


    public bool CreateAssociation(Guid assetUID, long legacyAssetId, Guid owningCustomerUID, Guid deviceUID, string deviceSerialNumber, string deviceType)
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetEvent = new CreateAssetEvent()
      {
        AssetUID = assetUID,
        AssetName = "The asset Name",
        AssetType = "unknown",
        SerialNumber = "3453gg",
        LegacyAssetId = legacyAssetId,
        OwningCustomerUID = owningCustomerUID,
        ActionUTC = actionUTC
      };

      var createDeviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = deviceUID,
        DeviceSerialNumber = deviceSerialNumber,
        DeviceType = deviceType,
        DeviceState = "active",
        ActionUTC = actionUTC
      };

      var associateDeviceAssetEvent = new AssociateDeviceAssetEvent()
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUTC
      };

      assetContext.StoreEvent(createAssetEvent).Wait();
      deviceContext.StoreEvent(createDeviceEvent).Wait();
      deviceContext.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceContext.GetAssociatedAsset(createDeviceEvent.DeviceSerialNumber, createDeviceEvent.DeviceType); g.Wait();
      return (g.Result != null ? true : false);
    }
  }

}
 
 