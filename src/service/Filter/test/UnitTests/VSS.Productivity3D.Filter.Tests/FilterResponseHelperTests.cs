﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterResponseHelperTests
  {
    private IRaptorProxy mockedRaptorProxy;
    private Guid ProjectGuid = Guid.NewGuid();
    private static DateTime mockedStartTime = new DateTime(2016, 11, 5);
    private DateTime mockedEndTime = new DateTime(2018, 11, 6);

    [TestInitialize]
    public void TestInit()
    {
      var mockedRaptorProxySetup = new Mock<IRaptorProxy>();
      mockedRaptorProxySetup.Setup(IRaptorProxy =>
          IRaptorProxy.GetProjectStatistics(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(new ProjectStatisticsResult
        {
          startTime = mockedStartTime,
          endTime = mockedEndTime
        }));

      mockedRaptorProxy = mockedRaptorProxySetup.Object;
    }

    [TestMethod]
    public void Should_return_When_project_is_null()
    {
      try
      {
        var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = "{\"dateRangeType\":\"0\",\"elevationType\":null}" };
        FilterJsonHelper.ParseFilterJson(null, filter, mockedRaptorProxy, new Dictionary<string, string>());

        Abstractions.Models.Filter filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filter.FilterJson);
        Assert.AreEqual(DateRangeType.Today, filterObj.DateRangeType);
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_filter_is_null()
    {
      try
      {
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filter: (MasterData.Repositories.DBModels.Filter)null, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_filterDescriptor_is_null()
    {
      try
      {
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filter: (FilterDescriptor)null, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_filters_collection_is_null()
    {
      try
      {
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filters: null, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_project_ianaTimezone_is_null()
    {
      try
      {
        var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = "{\"dateRangeType\":\"4\",\"elevationType\":null}" };
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filter, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

        Abstractions.Models.Filter filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filter.FilterJson);
        Assert.AreEqual(DateRangeType.CurrentMonth, filterObj.DateRangeType);
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    [DataRow(DateRangeType.Custom, true)]
    [DataRow(DateRangeType.Custom, false)]
    public void Should_not_set_dates_based_on_DateRangeType(DateRangeType dateRangeType, bool asAtDate)
    {
      var startUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 5) : (DateTime?)null;
      var endUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 6) : (DateTime?)null;
      //Json deserialize interprets date as mm/dd/yyyy so format date that way
      var startUtcStr = startUtc?.ToString("MM/dd/yyyy");
      var endUtcStr = endUtc?.ToString("MM/dd/yyyy");
      var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"startUTC\":\"{startUtcStr}\",\"endUTC\":\"{endUtcStr}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = ProjectGuid.ToString() }, filter, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

      Abstractions.Models.Filter filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filter.FilterJson);
      Assert.AreEqual(dateRangeType, filterObj.DateRangeType);
      if (asAtDate)
        Assert.AreEqual(mockedStartTime, filterObj.StartUtc);
      else
        Assert.AreEqual(startUtc, filterObj.StartUtc);
      Assert.AreEqual(endUtc, filterObj.EndUtc);
    }

    [TestMethod]
    [DataRow(DateRangeType.Custom, true)]
    [DataRow(DateRangeType.Custom, false)]
    public void Should_not_set_dates_based_on_DateRangeType_When_using_Custom(DateRangeType dateRangeType, bool asAtDate)
    {
      var startUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 5) : (DateTime?)null;
      var endUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 6) : (DateTime?)null;


      //Json deserialize interprets date as mm/dd/yyyy so format date that way
      var startUtcStr = startUtc?.ToString("MM/dd/yyyy");
      var endUtcStr = endUtc?.ToString("MM/dd/yyyy");
      var filterDescriptor = new FilterDescriptor { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"startUTC\":\"{startUtcStr}\",\"endUTC\":\"{endUtcStr}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = ProjectGuid.ToString() }, filterDescriptor, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

      Abstractions.Models.Filter filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.AreEqual(asAtDate ? mockedStartTime : startUtc, filterObj.StartUtc);
      Assert.AreEqual(endUtc, filterObj.EndUtc);
    }


    [TestMethod]
    [DataRow(DateRangeType.ProjectExtents, true)]
    [DataRow(DateRangeType.ProjectExtents, false)]
    public void Should_return_project_extents_for_project_extents(DateRangeType dateRangeType, bool useNullDate)
    {
      var startUtc = useNullDate ? (DateTime?)null : new DateTime(2017, 11, 5);
      var endUtc = useNullDate ? (DateTime?)null : new DateTime(2017, 11, 6);

      //Json deserialize interprets date as mm/dd/yyyy so format date that way
      var startUtcStr = startUtc?.ToString("MM/dd/yyyy");
      var endUtcStr = endUtc?.ToString("MM/dd/yyyy");
      var filterDescriptor = new FilterDescriptor { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"false\",\"startUTC\":\"{startUtcStr}\",\"endUTC\":\"{endUtcStr}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = ProjectGuid.ToString() }, filterDescriptor, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

      Abstractions.Models.Filter filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.AreEqual(mockedStartTime, filterObj.StartUtc);
      Assert.AreEqual(mockedEndTime, filterObj.EndUtc);
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth, true)]
    [DataRow(DateRangeType.CurrentWeek, true)]
    [DataRow(DateRangeType.PreviousMonth, true)]
    [DataRow(DateRangeType.PreviousWeek, true)]
    [DataRow(DateRangeType.Today, true)]
    [DataRow(DateRangeType.Yesterday, true)]
    [DataRow(DateRangeType.PriorToYesterday, true)]
    [DataRow(DateRangeType.PriorToPreviousWeek, true)]
    [DataRow(DateRangeType.PriorToPreviousMonth, true)]
    [DataRow(DateRangeType.CurrentMonth, false)]
    [DataRow(DateRangeType.CurrentWeek, false)]
    [DataRow(DateRangeType.PreviousMonth, false)]
    [DataRow(DateRangeType.PreviousWeek, false)]
    [DataRow(DateRangeType.Today, false)]
    [DataRow(DateRangeType.Yesterday, false)]
    [DataRow(DateRangeType.PriorToYesterday, false)]
    [DataRow(DateRangeType.PriorToPreviousWeek, false)]
    [DataRow(DateRangeType.PriorToPreviousMonth, false)]
    public void Should_set_dates_based_on_DateRangeType_When_using_collection_of_Filters(DateRangeType dateRangeType, bool asAtDate)
    {
      var filters = new List<MasterData.Repositories.DBModels.Filter>();

      for (int i = 0; i < 10; i++)
      {
        filters.Add(new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null}}" });
      }
      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = ProjectGuid.ToString() }, filters, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

      foreach (var filter in filters)
      {
        ValidateDates(filter.FilterJson, asAtDate);
      }
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth, true)]
    [DataRow(DateRangeType.CurrentWeek, true)]
    [DataRow(DateRangeType.PreviousMonth, true)]
    [DataRow(DateRangeType.PreviousWeek, true)]
    [DataRow(DateRangeType.Today, true)]
    [DataRow(DateRangeType.Yesterday, true)]
    [DataRow(DateRangeType.PriorToYesterday, true)]
    [DataRow(DateRangeType.PriorToPreviousWeek, true)]
    [DataRow(DateRangeType.PriorToPreviousMonth, true)]
    [DataRow(DateRangeType.CurrentMonth, false)]
    [DataRow(DateRangeType.CurrentWeek, false)]
    [DataRow(DateRangeType.PreviousMonth, false)]
    [DataRow(DateRangeType.PreviousWeek, false)]
    [DataRow(DateRangeType.Today, false)]
    [DataRow(DateRangeType.Yesterday, false)]
    [DataRow(DateRangeType.PriorToYesterday, false)]
    [DataRow(DateRangeType.PriorToPreviousWeek, false)]
    [DataRow(DateRangeType.PriorToPreviousMonth, false)]
    public void Should_set_dates_based_on_DateRangeType_When_using_Filter(DateRangeType dateRangeType, bool asAtDate)
    {
      var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = ProjectGuid.ToString() }, filter, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

      ValidateDates(filter.FilterJson, asAtDate);
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth, true)]
    [DataRow(DateRangeType.CurrentWeek, true)]
    [DataRow(DateRangeType.PreviousMonth, true)]
    [DataRow(DateRangeType.PreviousWeek, true)]
    [DataRow(DateRangeType.Today, true)]
    [DataRow(DateRangeType.Yesterday, true)]
    [DataRow(DateRangeType.PriorToYesterday, true)]
    [DataRow(DateRangeType.PriorToPreviousWeek, true)]
    [DataRow(DateRangeType.PriorToPreviousMonth, true)]
    [DataRow(DateRangeType.CurrentMonth, false)]
    [DataRow(DateRangeType.CurrentWeek, false)]
    [DataRow(DateRangeType.PreviousMonth, false)]
    [DataRow(DateRangeType.PreviousWeek, false)]
    [DataRow(DateRangeType.Today, false)]
    [DataRow(DateRangeType.Yesterday, false)]
    [DataRow(DateRangeType.PriorToYesterday, false)]
    [DataRow(DateRangeType.PriorToPreviousWeek, false)]
    [DataRow(DateRangeType.PriorToPreviousMonth, false)]
    public void Should_set_dates_based_on_DateRangeType_When_using_FilterDescriptor(DateRangeType dateRangeType, bool asAtDate)
    {
      var filterDescriptor = new FilterDescriptor { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = ProjectGuid.ToString() }, filterDescriptor, raptorProxy: mockedRaptorProxy, customHeaders: new Dictionary<string, string>());

      ValidateDates(filterDescriptor.FilterJson, asAtDate);
    }

    private static void ValidateDates(string filterJson, bool startUtcShouldBeExtents)
    {
      Abstractions.Models.Filter filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);
      //todo tidy this up
      if (startUtcShouldBeExtents)
      {
        Assert.AreEqual(mockedStartTime, filterObj.StartUtc);
      }
      else
      {
        Assert.IsNotNull(filterObj.StartUtc);
      }
      Assert.IsNotNull(filterObj.EndUtc);
    }
  }
}
