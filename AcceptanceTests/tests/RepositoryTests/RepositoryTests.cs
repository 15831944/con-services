﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class RepositoryTests : TestControllerBase
  {

    [TestInitialize]
    public void Init()
    {
      SetupLoggingAndRepos();
    }

    #region Filters
    [TestMethod]
    public void FilterSchemaExists()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID", "FilterUID", "fk_CustomerUID", "fk_ProjectUID", "UserID" , "Name" , "FilterJson", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }


    /// <summary>
    /// Get Happy path i.e. active, persistent only
    /// </summary>
    [TestMethod]
    public void GetFiltersForProject_PersistentOnly()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();
      var userId = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createTransientFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createTransientFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "HasAName1",
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "HasAName2",
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      filterRepo.StoreEvent(createTransientFilterEvent1).Wait();
      filterRepo.StoreEvent(createTransientFilterEvent2).Wait();
      filterRepo.StoreEvent(createPersistentFilterEvent1).Wait();
      filterRepo.StoreEvent(createPersistentFilterEvent2).Wait();

      var g = filterRepo.GetFiltersForProject(projUid.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(2, g.Result.Count(), "retrieved filter count is incorrect");

      g = filterRepo.GetFiltersForProjectUser(custUid.ToString(), projUid.ToString(), userId.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(2, g.Result.Count(), "retrieved filter count is incorrect");

      var f = filterRepo.GetFilter(createTransientFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createTransientFilterEvent1.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = filterRepo.GetFilter(createTransientFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createTransientFilterEvent2.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = filterRepo.GetFilter(createPersistentFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createPersistentFilterEvent1.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = filterRepo.GetFilter(createPersistentFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createPersistentFilterEvent2.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");
    }


    /// <summary>
    /// Create Happy path i.e. filter doesn't exist
    /// </summary>
    [TestMethod]
    public void CreateTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        ProjectUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      var s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");

      var f = filterRepo.GetFiltersForProject(createFilterEvent.ProjectUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(0, f.Result.Count(), "retrieved filter count is incorrect");

      f = filterRepo.GetFiltersForProjectUser(createFilterEvent.CustomerUID.ToString(), createFilterEvent.ProjectUID.ToString(), createFilterEvent.UserID);
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve user filters from filterRepo");
      Assert.AreEqual(0, f.Result.Count(), "retrieved user filter count is incorrect");
    }

    /// <summary>
    /// Create Transient should allow duplicate blank Names
    /// </summary>
    [TestMethod]
    public void CreateTranientFilter_DuplicateBlankNamesareValid()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createTransientFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userUid.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createTransientFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userUid.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var s = filterRepo.StoreEvent(createTransientFilterEvent1);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");

      s = filterRepo.StoreEvent(createTransientFilterEvent2);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event should have been written");
    }
    
    /// <summary>
    /// Create Persistent should not occur for duplicate Name.
    /// However, this is a business rule which should  be handled in the executor.
    /// </summary>
    [TestMethod]
    public void CreatePeristantFilter_DuplicateNamesNotValid()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createPersistentFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userUid.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "HasAName",
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userUid.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "HasAName",
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var s = filterRepo.StoreEvent(createPersistentFilterEvent1);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");

      s = filterRepo.StoreEvent(createPersistentFilterEvent2);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event should have been written");
    }
 
    
    /// <summary>
    /// Update Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    public void UpdateTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = string.Empty,
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Filter event not updateable");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }
    
    /// <summary>
    /// Update Transient unHappy path i.e. not allowed to update transient
    /// </summary>
    [TestMethod]
    public void UpdateTransientFilter_UnhappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID.ToString(),
        FilterUID = createFilterEvent.FilterUID,
        Name = string.Empty,
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Filter event should not be updated");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Update Persistent Happy path i.e. allowed to update persistent
    /// </summary>
    [TestMethod]
    public void UpdatePersistentFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "persistent",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "changed",
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = updateFilterEvent.CustomerUID.ToString(),
        ProjectUid = updateFilterEvent.ProjectUID.ToString(),
        UserId = updateFilterEvent.UserID,
        FilterUid = updateFilterEvent.FilterUID.ToString(),
        Name = updateFilterEvent.Name,
        FilterJson = updateFilterEvent.FilterJson,
        LastActionedUtc = updateFilterEvent.ActionUTC
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event should have been updated");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Update Persistent unHappy path i.e. update received before create
    /// </summary>
    [TestMethod]
    public void UpdatePersistentFilter_OutOfOrder()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "persistent",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "changed",
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = updateFilterEvent.Name,
        FilterJson = updateFilterEvent.FilterJson,
        LastActionedUtc = updateFilterEvent.ActionUTC
      };

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event should be created with the update");

      s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Filter event should not be updated with the create");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }



    /// <summary>
    /// Delete Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    public void DeleteTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteFilterEvent = new DeleteFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(deleteFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not deleted");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve filter from filterRepo");
    }

    /// <summary>
    /// Delete unHappy path i.e. filter doesn't exist
    ///   actually a deleted filter is created in case of out-of-order i.e. delete received before create.
    ///   this ensures that the filters 'deleted' status is retained.
    /// </summary>
    [TestMethod]
    public void DeleteTransientFilter_UnhappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var deleteFilterEvent = new DeleteFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var s = filterRepo.StoreEvent(deleteFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event set to deleted");

      var g = filterRepo.GetFilter(deleteFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve deleted filter from filterRepo");

      g = filterRepo.GetFilterForUnitTest(deleteFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Should be able to retrieve deleted filter from filterRepo");
    }

    /// <summary>
    /// Delete Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    public void DeletePersistentFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "hasOne",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteFilterEvent = new DeleteFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(deleteFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not deleted");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve filter from filterRepo");
    }
    #endregion

    #region Boundaries
    [TestMethod]
    public void GeofenceSchemaExists()
    {
      const string tableName = "Geofence";
      List<string> columnNames = new List<string>
      {
        "ID", "GeofenceUID", "Name" , "fk_GeofenceTypeID", "GeometryWKT", "FillColor", "IsTransparent", "IsDeleted", "Description", "AreaSqMeters", "fk_CustomerUID", "UserUID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectGeofenceSchemaExists()
    {
      const string tableName = "ProjectGeofence";
      List<string> columnNames = new List<string>
      {
        "ID", "fk_GeofenceUID", "fk_ProjectUID" , "LastActionedUTC", "InsertUTC", "UpdateUTC"

      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void GetAssociatedProjectGeofences()
    {
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createAssociateEvent1 = new AssociateProjectGeofence
      {
        ProjectUID = projUid,
        GeofenceUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createAssociateEvent2 = new AssociateProjectGeofence
      {
        ProjectUID = projUid,
        GeofenceUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      projectRepo.StoreEvent(createAssociateEvent1).Wait();
      projectRepo.StoreEvent(createAssociateEvent2).Wait();
      var a = projectRepo.GetAssociatedGeofences(projUid.ToString());
      a.Wait();
      Assert.IsNotNull(a.Result, "Failed to get associated boundaries");
      Assert.AreEqual(2, a.Result.Count(), "Wrong number of boundaries retrieved");
    }

    [TestMethod]
    public void CreateAssociatedProjectGeofence_HappyPath()
    {
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createAssociateEvent = new AssociateProjectGeofence
      {
        ProjectUID = projUid,
        GeofenceUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var p = projectRepo.StoreEvent(createAssociateEvent);
      p.Wait();
      Assert.AreEqual(1, p.Result, "Associate event not written");
    }

    [TestMethod]
    public void GetGeofence_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceType = GeofenceType.Filter;
      var geofenceUid = Guid.NewGuid();
      var createGeofenceEvent = new CreateGeofenceEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        GeofenceUID = geofenceUid,
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType.ToString(),
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();

      var g = geofenceRepo.GetGeofence(geofenceUid.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve geofence from geofenceRepo");
      Assert.AreEqual(geofenceUid.ToString(), g.Result.GeofenceUID, "Wrong geofence retrieved");
    }

    [TestMethod]
    public void GetGeofences_HappyPath()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceType = GeofenceType.Filter.ToString();
      var geofenceUId = Guid.NewGuid();
      var createGeofenceEvent1 = new CreateGeofenceEvent
      {
        CustomerUID = custUid,
        UserUID = userId,
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType,
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createGeofenceEvent2 = new CreateGeofenceEvent
      {
        CustomerUID = custUid,
        UserUID = userId,
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary two",
        GeofenceType = geofenceType,
        GeometryWKT = "POLYGON((81.257874 13.677856,80.856873 14.039345,81.375977 14.443052,81.257874 13.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createGeofenceEvent3 = new CreateGeofenceEvent
      {
        CustomerUID = custUid,
        UserUID = userId,
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary three",
        GeofenceType = geofenceType,
        GeometryWKT = "POLYGON((82.257874 14.677856,81.856873 15.039345,82.375977 15.443052,82.257874 14.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var deleteGeofenceEvent = new DeleteGeofenceEvent
      {
        GeofenceUID = createGeofenceEvent1.GeofenceUID,
        UserUID = userId,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      geofenceRepo.StoreEvent(createGeofenceEvent1).Wait();
      geofenceRepo.StoreEvent(createGeofenceEvent2).Wait();
      geofenceRepo.StoreEvent(createGeofenceEvent3).Wait();
      geofenceRepo.StoreEvent(deleteGeofenceEvent).Wait();

      var ids = new List<string>
      {
        createGeofenceEvent1.GeofenceUID.ToString(),
        createGeofenceEvent2.GeofenceUID.ToString(),
        createGeofenceEvent3.GeofenceUID.ToString()
      };
      var g = geofenceRepo.GetGeofences(ids);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve geofences from geofenceRepo");
      Assert.AreEqual(2, g.Result.Count(), "Wrong number of geofences retrieved");
    }

    [TestMethod]
    public void CreateGeofence_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceType = GeofenceType.Filter;
      var createGeofenceEvent = new CreateGeofenceEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType.ToString(),
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var boundary = new Geofence
      {
        CustomerUID = createGeofenceEvent.CustomerUID.ToString(),
        UserUID = createGeofenceEvent.UserUID.ToString(),
        GeofenceUID = createGeofenceEvent.GeofenceUID.ToString(),
        Name = createGeofenceEvent.GeofenceName,
        GeofenceType = geofenceType,
        GeometryWKT = createGeofenceEvent.GeometryWKT,
        LastActionedUTC = createGeofenceEvent.ActionUTC
      };

      var s = geofenceRepo.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Geofence event not written");

      var g = geofenceRepo.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve geofence from geofenceRepo");
      //Note: cannot compare objects as Geofence has nullables while CreateGeofenceEvent doesn't
      Assert.AreEqual(boundary.CustomerUID, g.Result.CustomerUID, "Wrong CustomerUID");
      Assert.AreEqual(boundary.UserUID, g.Result.UserUID, "Wrong UserUID");
      Assert.AreEqual(boundary.GeofenceUID, g.Result.GeofenceUID, "Wrong GeofenceUID");
      Assert.AreEqual(boundary.Name, g.Result.Name, "Wrong Name");
      Assert.AreEqual(boundary.GeofenceType, g.Result.GeofenceType, "Wrong GeofenceType");
      Assert.AreEqual(boundary.GeometryWKT, g.Result.GeometryWKT, "Wrong GeometryWKT");
    }

    [TestMethod]
    public void DeleteGeofence_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var geofenceType = GeofenceType.Filter;
      var createGeofenceEvent = new CreateGeofenceEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = userUid,
        GeofenceUID = geofenceUid,
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType.ToString(),
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteGeofenceEvent = new DeleteGeofenceEvent
      {
        GeofenceUID = geofenceUid,
        UserUID = userUid,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();

      var s = geofenceRepo.StoreEvent(deleteGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Geofence event not deleted");

      var g = geofenceRepo.GetGeofence(geofenceUid.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve geofence from geofenceRepo");
    }

    #endregion

    #region privates

    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(configStore.GetConnectionString("VSPDB")))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();
          Assert.IsNotNull(table, "Missing " + tableName + " table schema");
          Assert.AreEqual(tableName, table.TABLE_NAME, "Wrong table name");

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.IsNotNull(columns, "Missing " + tableName + " table columns");
          Assert.AreEqual(columnNames.Count, columns.Count, "Wrong number of " + tableName + " columns");
          foreach (var columnName in columnNames)
            Assert.IsNotNull(columns.Find(c => c.COLUMN_NAME == columnName), "Missing " + columnName + " column in " + tableName + " table");
        }
        finally
        {
          connection.Close();
        }
      }
    }

    private string GetQuery(string tableName, bool selectTable)
    {
      string what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = string.Format("SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, configStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }

    #endregion privates
  }
}
