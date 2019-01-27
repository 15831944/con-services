﻿using System;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class FilterSpatialCacheFingerprint
  {
    [Fact]
    public void Test_GetCacheFingerPrint_Default()
    {
      string fp = new CombinedFilter().AttributeFilter.SpatialCacheFingerprint();

      Assert.True(string.IsNullOrEmpty(fp), $"Fingerprint for null filter was not empty, = '{fp}'");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_IncludeEarliestCellPass_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.ReturnEarliestFilteredCellPass = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("REFCP:1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain earliest filtered cell pass ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_IncludeEarliestCellPass_NotPresent()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.ReturnEarliestFilteredCellPass = false);

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("REFCP", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains earliest filtered cell pass ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_RestrictFilteredDataToCompactorsOnly_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasCompactionMachinesOnlyFilter = true;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("CMO:1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain compactor restriction ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_RestrictFilteredDataToCompactorsOnly_NotPresent()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasCompactionMachinesOnlyFilter = false);

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("CMO", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains compactor restriction ID");
    }

    private const string ExcludeSurveyedSurfacesID = "ESS:1";

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasDesignFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasDesignFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasMachineFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMachineFilter = true;
        x.AttributeFilter.MachineIDs = new short[] {0};      
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasMachineDirectionFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasMachineDirectionFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasVibeStateFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasVibeStateFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasCompactionMachinesOnlyFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasCompactionMachinesOnlyFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasGPSAccuracyFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasGPSAccuracyFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasPassTypeFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasPassTypeFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }


    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasTemperatureRangeFilter()
    {
      var filter = CombinedFilter.MakeFilterWith(x => x.AttributeFilter.HasTemperatureRangeFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ESS", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_TimeFilter_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasTimeFilter = true;
        x.AttributeFilter.StartTime = new DateTime(1111);
        x.AttributeFilter.EndTime = new DateTime(2222);
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TF:1111-2222", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain time filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_TimeFilter_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TF:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains time filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_DesignFilter_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasDesignFilter = true;
        x.AttributeFilter.DesignNameID = 123;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("DF:123", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain design name filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_DesignFilter_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("DF:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains design name filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineFilter_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMachineFilter = true;
        x.AttributeFilter.MachineIDs = new short[] {1, 12, 23};
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MF:-1-12-23", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain machine filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineFilter_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MF:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains machine filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineDirection_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMachineDirectionFilter = true;
        x.AttributeFilter.MachineDirection = MachineDirection.Forward;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MD:Forward", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain machine direction filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineDirection_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MD:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains machine direction filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_VibeState_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasVibeStateFilter = true;
        x.AttributeFilter.VibeState = VibrationState.On;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("VS:On", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain vibe state filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_VibeState_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("VS:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains vibe state ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationMappingMode_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationMappingModeFilter = true;
        x.AttributeFilter.MinElevationMapping = ElevationMappingMode.MinimumElevation;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("EMM:1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain min elevation mapping filter ID");

      filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationMappingModeFilter = true;
        x.AttributeFilter.MinElevationMapping = ElevationMappingMode.LatestElevation;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("EMM:0", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain min elevation mapping filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationMappingMode_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("EMM:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains vibe state ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationType_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationTypeFilter = true;
        x.AttributeFilter.ElevationType = ElevationType.Last;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ET:Last", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain elevation type filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationType_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ET:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains elevation type filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GuidanceMode_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGCSGuidanceModeFilter = true;
        x.AttributeFilter.GCSGuidanceMode = MachineAutomaticsMode.Manual;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GM:Manual", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain guidance mode filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GuidanceMode_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ET:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains guidance mode filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GPSAccuracy_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGPSAccuracyFilter = true;
        x.AttributeFilter.GPSAccuracy = GPSAccuracy.Fine;
        x.AttributeFilter.GPSAccuracyIsInclusive = true;

      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GA:1-Fine", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain GPS Accuracy filter ID");

      filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGPSAccuracyFilter = true;
        x.AttributeFilter.GPSAccuracy = GPSAccuracy.Fine;
        x.AttributeFilter.GPSAccuracyIsInclusive = false;

      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GA:0-Fine", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain GPS Accuracy filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GPSAccuracy_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GA:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains GPS Accuracy filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GPSTolerance_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGPSToleranceFilter = true;
        x.AttributeFilter.GPSTolerance = 123;
        x.AttributeFilter.GPSToleranceIsGreaterThan = true;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GT:1-123", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain GPS Tolerance filter ID");

      filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGPSToleranceFilter = true;
        x.AttributeFilter.GPSTolerance = 123;
        x.AttributeFilter.GPSToleranceIsGreaterThan = false;

      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GT:0-123", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain GPS Tolerance filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GPSTolerance_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GT:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains GPS Tolerance filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_PositiongTech_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasPositioningTechFilter = true;
        x.AttributeFilter.PositioningTech = PositioningTech.UTS;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("PT:UTS", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain positioning tech filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_PositiongTech_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("PT:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains positioning tech filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationRange_Present()
    {
      Guid designGuid = Guid.Parse("12345678-1234-1234-1234-123456781234");

      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationRangeFilter = true;
        x.AttributeFilter.ElevationRangeDesignUID = designGuid;
        x.AttributeFilter.ElevationRangeOffset = 123.456;
        x.AttributeFilter.ElevationRangeThickness = 1.234;
      });

      var s = filter.AttributeFilter.SpatialCacheFingerprint();

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ER:12345678-1234-1234-1234-123456781234-123.456-1.234", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain elevation range filter ID");

      filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationRangeFilter = true;
        x.AttributeFilter.ElevationRangeLevel = 123.456;
        x.AttributeFilter.ElevationRangeOffset = 456.789;
        x.AttributeFilter.ElevationRangeThickness = 2.345;
      });

      s = filter.AttributeFilter.SpatialCacheFingerprint();

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ER:123.456-456.789-2.345", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain elevation range filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationRange_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ER:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains elevation range filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_LayerState_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasLayerStateFilter = true;
        x.AttributeFilter.LayerState = LayerState.On;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("LS:On", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain layer state filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_LayerState_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("LS:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains layer state filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_LayerID_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasLayerIDFilter = true;
        x.AttributeFilter.LayerID = 1234;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("LID:1234", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain layer ID filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_LayerID_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("LID:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains layer ID filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_TemperatureRange_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasTemperatureRangeFilter = true;
        x.AttributeFilter.MaterialTemperatureMin = 123;
        x.AttributeFilter.MaterialTemperatureMax = 456;
        x.AttributeFilter.FilterTemperatureByLastPass = true;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TR:123-456-1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain temperature range filter ID");

      filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasTemperatureRangeFilter = true;
        x.AttributeFilter.MaterialTemperatureMin = 123;
        x.AttributeFilter.MaterialTemperatureMax = 456;
        x.AttributeFilter.FilterTemperatureByLastPass = false;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TR:123-456-0", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain temperature range filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_TemperatureRange_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TR:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains temperature range filter ID");
    }


    [Fact]
    public void Test_GetCacheFingerPrint_PassCountRange_Present()
    {
      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasPassCountRangeFilter = true;
        x.AttributeFilter.PasscountRangeMin = 2;
        x.AttributeFilter.PasscountRangeMax = 11;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("PC:2-11", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain layer ID filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_PassCountRange_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("PC:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains layer ID filter ID");
    }
  }
}
