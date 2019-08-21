﻿using System;
using System.Collections.Generic;
using DotNetCore.CAP.Dashboard.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICDecls;
using SVOICOptionsDecls;
using SVOSiteVisionDecls;
using VLPDDecls;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApiTests.Common
{
  [TestClass]
  public class RaptorConvertersTests
  {
    [TestClass]
    public class ConvertFilterTests : RaptorConvertersTests
    {
      private static Filter.Abstractions.Models.Filter CreateFilter(
        long? onMachineDesignId = null,
        ElevationType elevationType = ElevationType.First,
        bool? vibeStateOn = null,
        bool? forwardDirection = null,
        AutomaticsType? automaticsType = null,
        double? temperatureRangeMin = null,
        double? temperatureRangeMax = null,
        int? passCountRangeMin = null,
        int? passCountRangeMax = null,
        string onMachineDesignName = null)
      {
        return new Filter.Abstractions.Models.Filter(
            new DateTime(2019, 1, 10),
            new DateTime(2019, 1, 20),
            "",
            Strings.Metrics_EnqueuedCountOrNull,
            new List<MachineDetails>(),
            onMachineDesignId,
            elevationType,
            vibeStateOn,
            new List<WGSPoint>(),
            forwardDirection,
            0,
            automaticsType: automaticsType,
            temperatureRangeMin:  temperatureRangeMin,
            temperatureRangeMax: temperatureRangeMax,
            passCountRangeMin: passCountRangeMin,
            passCountRangeMax: passCountRangeMax,
            onMachineDesignName: onMachineDesignName);
      }

      [TestMethod]
      public void Should_return_Default_filter_When_input_filter_is_null()
      {
        var result = RaptorConverters.ConvertFilter(null, null, null);

        Assert.IsNotNull(result);
        Assert.IsTrue(TFilterLayerMethod.flmAutoMapReset == result.LayerMethod);
      }

      [TestMethod]
      public void Should_return_filter_with_correct_startUTC_When_overrideStartUTC_is_set()
      {
        var filter = CreateFilter();
        var filterResult = new FilterResult(filter:filter);
        var overrideStartUTC = new DateTime(2019, 1, 1);

        var result = RaptorConverters.ConvertFilter(filterResult, overrideStartUTC: overrideStartUTC);

        Assert.AreEqual(overrideStartUTC, result.StartTime.Value);
        Assert.IsNotNull(filter.StartUtc);
        Assert.AreNotEqual(filter.StartUtc.Value, result.StartTime.Value);
      }

      [TestMethod]
      public void Should_return_filter_with_correct_endUTC_When_overrideEndUTC_is_set()
      {
        var filter = CreateFilter();

        var filterResult = new FilterResult(filter: filter);
        var overrideEndUTC = new DateTime(2019, 1, 1);

        var result = RaptorConverters.ConvertFilter(filterResult, overrideEndUTC: overrideEndUTC);

        Assert.AreEqual(overrideEndUTC, result.EndTime.Value);
        Assert.IsNotNull(filter.EndUtc);
        Assert.AreNotEqual(filter.EndUtc.Value, result.StartTime.Value);
      }

      [DataTestMethod]
      [DataRow(null, 0)]
      [DataRow(30303, 30303)]
      public void Should_set_DesignNameID_From_OnMachineDesignId_value(long machineDesignId, int expectedValue)
      {
        var filterResult = new FilterResult(filter: CreateFilter(machineDesignId));
        var result = RaptorConverters.ConvertFilter(filterResult);

        Assert.AreEqual(expectedValue, result.DesignNameID);
      }

      [DataTestMethod]
      [DataRow("The Design Name1", 0, 304)]
      [DataRow("The Design Name2", 34, 34)]
      [DataRow("", 0, 0)]
      [DataRow("", 45, 45)]
      [DataRow("<No Design>", 0, 0)]
      public void Should_set_DesignID_From_OnMachineDesignName_value(string onMachineDesignName, long onMachineDesignIdInput, int expectedValue)
      {
        long? onMachineDesignId = onMachineDesignIdInput; // DataRow doesn't like long? as parameter
        var machineDesigns = new []
          {new TDesignName() {FID = expectedValue, FMachineID = 4, FName = onMachineDesignName}};
        var mockRaptorClient = new Mock<IASNodeClient>();
        mockRaptorClient.Setup(prj => prj.GetOnMachineDesignEvents(It.IsAny<long>())).Returns(machineDesigns);

        var filterResult = new FilterResult(filter: CreateFilter(onMachineDesignId, onMachineDesignName: onMachineDesignName));
        Assert.AreEqual(onMachineDesignId, filterResult.OnMachineDesignId);

        var raptorFilter = RaptorConverters.ConvertFilter(filterResult, 1, mockRaptorClient.Object);
        Assert.AreEqual(expectedValue, raptorFilter.DesignNameID);
      }

      [TestMethod]
      public void Should_set_DesignID_From_OnMachineDesignName_null()
      {
        string onMachineDesignName = null;
        long? onMachineDesignId = null;
        int expectedValue = 0;
        var machineDesigns = new[]
        {new TDesignName() {FID = expectedValue, FMachineID = 4, FName = onMachineDesignName}};
        var mockRaptorClient = new Mock<IASNodeClient>();
        mockRaptorClient.Setup(prj => prj.GetOnMachineDesignEvents(It.IsAny<long>())).Returns(machineDesigns);

        var filterResult = new FilterResult(filter:CreateFilter(onMachineDesignId, onMachineDesignName: onMachineDesignName));
        Assert.AreEqual(onMachineDesignId, filterResult.OnMachineDesignId);

        var raptorFilter = RaptorConverters.ConvertFilter(filterResult, 1, mockRaptorClient.Object);
        Assert.AreEqual(expectedValue, raptorFilter.DesignNameID);
      }

    [DataTestMethod]
      [DataRow(false)]
      [DataRow(true)]
      public void Should_set_Machines_From_AssetIDs_list_When_ContributingMachines_is_null_or_empty(bool setContributingMachines)
      {
        var assetIds = new List<long> { 1, 2, 3, 5, 8, 13 };
        var contributingMachines = setContributingMachines
          ? new List<MachineDetails>()
          : null;

        var filterResult = FilterResult.CreateFilterForCCATileRequest(assetIDs:assetIds, contributingMachines:contributingMachines);
        var result = RaptorConverters.ConvertFilter(filterResult);

        Assert.IsTrue(result.Machines.Length == assetIds.Count);
      }

      [DataTestMethod]
      [DataRow(false)]
      [DataRow(true)]
      public void Should_set_Machines_From_AssetIDs_list_When_ContributingMachines_is_not_empty(bool setAssetIds)
      {
        var assetIds = setAssetIds
          ? new List<long> { 1, 2, 3, 5, 8, 13 }
          : null;

        var contributingMachines = new List<MachineDetails>
        {
          new MachineDetails(21, "twentyone", false),
          new MachineDetails(43, "thirtyfour", false)
        };

        var filterResult = FilterResult.CreateFilterForCCATileRequest(assetIDs: assetIds, contributingMachines: contributingMachines);
        var result = RaptorConverters.ConvertFilter(filterResult);

        var expectedArrayLength = (assetIds?.Count ?? 0) + contributingMachines.Count;

        Assert.IsTrue(result.Machines.Length == expectedArrayLength);
        Assert.IsTrue(result.Machines[expectedArrayLength - 2].Name == "twentyone");
        Assert.IsTrue(result.Machines[expectedArrayLength - 1].Name == "thirtyfour");
      }

      [DataTestMethod]
      [DataRow(null, TICVibrationState.vsOff)]
      [DataRow(false, TICVibrationState.vsOff)]
      [DataRow(true, TICVibrationState.vsOn)]
      public void Should_set_VibeState_From_VibeStateOn_value(bool vibeStateOn, TICVibrationState expectedState)
      {
        var filterResult = new FilterResult(filter: CreateFilter(vibeStateOn: vibeStateOn));
        var result = RaptorConverters.ConvertFilter(filterResult);

        Assert.AreEqual(expectedState, result.VibeState);
      }

      [DataTestMethod]
      [DataRow(ElevationType.First, TICElevationType.etFirst)]
      [DataRow(ElevationType.Highest, TICElevationType.etHighest)]
      [DataRow(ElevationType.Last, TICElevationType.etLast)]
      [DataRow(ElevationType.Lowest, TICElevationType.etLowest)]
      public void Should_set_ElevationType_From_ElevationType_value(ElevationType elevationType, TICElevationType expectedElevationType)
      {
        var filterResult = new FilterResult(filter: CreateFilter(elevationType: elevationType));
        var result = RaptorConverters.ConvertFilter(filterResult);

        Assert.AreEqual(expectedElevationType, result.ElevationType);
      }

      // PolygonGrid tests

      [DataTestMethod]
      [DataRow(null, TICMachineDirection.mdForward)]
      [DataRow(false, TICMachineDirection.mdReverse)]
      [DataRow(true, TICMachineDirection.mdForward)]
      public void Should_set_MachineDirection_From_ForwardDirection_value(bool? forwardDirection, TICMachineDirection expectedState)
      {
        var filterResult = new FilterResult(filter: CreateFilter(forwardDirection: forwardDirection));
        var result = RaptorConverters.ConvertFilter(filterResult);

        Assert.AreEqual(expectedState, result.MachineDirection);
      }

      [DataTestMethod]
      [DataRow(AutomaticsType.Automatics, TGCSAutomaticsMode.amAutomatics)]
      [DataRow(AutomaticsType.Manual, TGCSAutomaticsMode.amManual)]
      [DataRow(AutomaticsType.Unknown, TGCSAutomaticsMode.amUnknown)]
      public void Should_set_GCSGuidanceMode_From_AutomaticsType_value(AutomaticsType automaticsType, TGCSAutomaticsMode automaticsMode)
      {
        var filterResult = new FilterResult(filter:CreateFilter(automaticsType: automaticsType));
        var result = RaptorConverters.ConvertFilter(filterResult);

        Assert.AreEqual(automaticsMode, result.GCSGuidanceMode);
      }

      [DataTestMethod]
      [DataRow(null, null)]
      [DataRow(null, 1.0)]
      [DataRow(1.23, null)]
      [DataRow(2.34, 3.45)]
      public void Should_set_TemperatureRange_From_TemperatureRange_values(double? temperatureRangeMin, double? temperatureRangeMax)
      {
        var filterResult = new FilterResult(filter: CreateFilter(temperatureRangeMin: temperatureRangeMin, temperatureRangeMax: temperatureRangeMax));
        var result = RaptorConverters.ConvertFilter(filterResult);

        if (!temperatureRangeMin.HasValue || !temperatureRangeMax.HasValue)
        {
          Assert.AreEqual(4096, result.TemperatureRangeMin);
          Assert.AreEqual(4096, result.TemperatureRangeMax);

          return;
        }

        Assert.AreEqual((ushort)(temperatureRangeMin * 10), result.TemperatureRangeMin);
        Assert.AreEqual((ushort)(temperatureRangeMax * 10), result.TemperatureRangeMax);
      }

      [DataTestMethod]
      [DataRow(null, null)]
      [DataRow(null, 1)]
      [DataRow(1, null)]
      [DataRow(2, 3)]
      public void Should_set_PassCountRange_From_PassCountRange_values(int? passCountRangeMin, int? passCountRangeMax)
      {
        var filterResult = new FilterResult(filter: CreateFilter(passCountRangeMin: passCountRangeMin, passCountRangeMax: passCountRangeMax));
        var result = RaptorConverters.ConvertFilter(filterResult);

        if (!passCountRangeMin.HasValue || !passCountRangeMax.HasValue)
        {
          Assert.AreEqual(0, result.PassCountRangeMin);
          Assert.AreEqual(0, result.PassCountRangeMax);

          return;
        }

        Assert.AreEqual(passCountRangeMin, result.PassCountRangeMin);
        Assert.AreEqual(passCountRangeMax, result.PassCountRangeMax);
      }
    }
  }
}
