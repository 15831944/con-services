﻿#if RAPTOR
using ASNode.UserPreferences;
using ASNode.ExportProductionDataCSV.RPC;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using BoundingExtents;
using Fences;
using SubGridTreesDecls;
using SVOICDecls;
using SVOICFiltersDecls;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using SVOSiteVisionDecls;
using VLPDDecls;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using __Global = ProductionServer_TLB.__Global;

namespace VSS.Productivity3D.Common.Proxies
{
  //TODO simplify and refactor this ugly class

  public static class RaptorConverters
  {
    public static readonly DateTime PDS_MIN_DATE = new DateTime(1899, 12, 30, 0, 0, 0);

    // a filter may have a design name but a zero Id (if it came from database or TRex)
    // find the raptorId from Raptor using the designName, as at this stage Raptor needs it's Id.
    // this may be changed in future to have Raptor use the name
    // This returns designs on machine. Designs are specific to project, so can disregard machineID.
    public static int GetRaptorDesignId(IASNodeClient raptorClient, long? legacyProjectId, long? onMachineDesignId,
      string onMachineDesignName)
    {
      if (onMachineDesignId.HasValue && onMachineDesignId > 0)
        return (int) onMachineDesignId;

      if (legacyProjectId > VelociraptorConstants.NO_PROJECT_ID && !string.IsNullOrEmpty(onMachineDesignName) && 
          (string.Compare("<No Design>", onMachineDesignName, StringComparison.OrdinalIgnoreCase) != 0))
      {
        var raptorDesigns = raptorClient.GetOnMachineDesignEvents(legacyProjectId.Value);
        if (raptorDesigns != null)
          return (int) raptorDesigns?.ToList()
            .Select(d => (string.Compare(d.FName, onMachineDesignName, StringComparison.OrdinalIgnoreCase) == 0)
              ? d.FID : 0)
            .FirstOrDefault();
      }

      return (int) onMachineDesignId;
    }

    public static void AdjustFilterToFilter(ref TICFilterSettings baseFilter, TICFilterSettings topFilter)
    {
      //Special case for Raptor filter to filter comparisons.
      //If base filter is earliest and top filter is latest with a DateTime filter then replace
      //base filter with latest with a date filter with the start date at the beginning of time and 
      //the end date at the original start date. This is to avoid losing data between original start date
      //and first event after the start date with data.
      if (baseFilter.HasTimeComponent() && baseFilter.ReturnEarliestFilteredCellPass &&
          topFilter.HasTimeComponent() && !topFilter.ReturnEarliestFilteredCellPass)
      {
        topFilter.SetElevationTypeCellpassState(false);

        baseFilter = AdjustBaseFilter(baseFilter);
      }
    }

    /// <summary>
    /// Returns adjusted filter settings copy for case of cached filter.
    /// </summary>
    public static TICFilterSettings AdjustBaseFilter(TICFilterSettings baseFilter)
    {
      var copy = new TICFilterSettings();
      copy.Assign(baseFilter);
      copy.EndTime = baseFilter.StartTime;
      copy.StartTime = PDS_MIN_DATE;
      copy.ReturnEarliestFilteredCellPass = false;
      copy.ElevationType = TICElevationType.etLast;
      copy.SetElevationTypeCellpassState(false);

      return copy;
    }

    public static TColourPalettes convertColorPalettes(List<ColorPalette> palettes, DisplayMode mode)
    {
      TColourPalettes result = new TColourPalettes();

      if ((mode == DisplayMode.CCA) || (mode == DisplayMode.CCASummary))
        return result;

      if (palettes == null || palettes.Count == 0)
        palettes = defaultColorPalette(mode);

      if (palettes.Count > 0)
      {
        result.Transitions = new TColourPalette[palettes.Count];

        for (int i = 0; i < palettes.Count; i++)
        {
          result.Transitions[i].Colour = palettes[i].Color;
          result.Transitions[i].Value = palettes[i].Value;
        }
      }

      return result;
    }


    // RGBToColor comes from WebMapCache.cs
    private static int RGBToColor(int r, int g, int b)
    {
      return r << 16 | g << 8 | b << 0;
    }

    // ElevationPalette comes from WebMapCache.cs
    public static List<int> ElevationPalette()
    {
      return new List<int>
      {
        RGBToColor(255, 0, 0),
        RGBToColor(225, 60, 0),
        RGBToColor(255, 90, 0),
        RGBToColor(255, 130, 0),
        RGBToColor(255, 170, 0),
        RGBToColor(255, 200, 0),
        RGBToColor(255, 220, 0),
        RGBToColor(250, 230, 0),
        RGBToColor(220, 230, 0),
        RGBToColor(210, 230, 0),
        RGBToColor(200, 230, 0),
        RGBToColor(180, 230, 0),
        RGBToColor(150, 230, 0),
        RGBToColor(130, 230, 0),
        RGBToColor(100, 240, 0),
        RGBToColor(0, 255, 0),
        RGBToColor(0, 240, 100),
        RGBToColor(0, 230, 130),
        RGBToColor(0, 230, 150),
        RGBToColor(0, 230, 180),
        RGBToColor(0, 230, 200),
        RGBToColor(0, 230, 210),
        RGBToColor(0, 220, 220),
        RGBToColor(0, 200, 230),
        RGBToColor(0, 180, 240),
        RGBToColor(0, 150, 245),
        RGBToColor(0, 120, 250),
        RGBToColor(0, 90, 255),
        RGBToColor(0, 70, 255),
        RGBToColor(0, 0, 255)
      };
    }

    private static List<ColorPalette> defaultColorPalette(DisplayMode mode)
    {
      ColorSettings cs = ColorSettings.Default;
      List<ColorPalette> palettes = null;
      switch (mode)
      {
        case DisplayMode.Design3D:
        case DisplayMode.Height:
          int numberOfColors = 30;

          double step = (cs.elevationMaximum.value - cs.elevationMinimum.value) / (numberOfColors - 1);
          List<int> colors = ElevationPalette();

          List<ColorPalette> paletteList = new List<ColorPalette>();
          paletteList.Add(new ColorPalette(cs.elevationBelowColor, -1));
          for (int i = 0; i < colors.Count; i++)
          {
            paletteList.Add(new ColorPalette((uint) colors[i], cs.elevationMinimum.value + i * step));
          }

          paletteList.Add(new ColorPalette(cs.elevationAboveColor, -1));

          palettes = paletteList;
          break;

        case DisplayMode.CCV:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.cmvMinimum.color, cs.cmvMinimum.value),
            new ColorPalette(cs.cmvTarget.color, 0.9 * cs.cmvTarget.value),
            new ColorPalette(cs.cmvTarget.color, 1.1 * cs.cmvTarget.value),
            new ColorPalette(cs.cmvMaximum.color, cs.cmvMaximum.value)
          };
          break;

        case DisplayMode.CCVPercentChange:

        case DisplayMode.CCVPercent:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.cmvPercentMinimum.color, cs.cmvPercentMinimum.value),
            new ColorPalette(cs.cmvPercentTarget.color, 0.9 * cs.cmvPercentTarget.value),
            new ColorPalette(cs.cmvPercentTarget.color, 1.1 * cs.cmvPercentTarget.value),
            new ColorPalette(cs.cmvPercentMaximum.color, cs.cmvPercentMaximum.value)
          };
          break;

        case DisplayMode.CMVChange:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(0, 0),
            new ColorPalette(65280, 10),
            new ColorPalette(16776960, 20),
            new ColorPalette(16744192, 40),
            new ColorPalette(16711935, 80),
            new ColorPalette(16711680, double.MaxValue)
          };
          break;

        case DisplayMode.Latency:
          break;

        case DisplayMode.PassCount:
          palettes = new List<ColorPalette>();

          for (int i = cs.passCountDetailColors.Count - 1; i >= 0; i--)
            palettes.Insert(cs.passCountDetailColors.Count - i - 1,
              new ColorPalette(cs.passCountDetailColors[i].color, cs.passCountDetailColors[i].value));
          break;

        case DisplayMode.PassCountSummary:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.passCountMinimum.color, cs.passCountMinimum.value),
            new ColorPalette(cs.passCountTarget.color, cs.passCountTarget.value),
            new ColorPalette(cs.passCountMaximum.color, cs.passCountMaximum.value)
          };
          break;

        case DisplayMode.RMV:
          break;
        case DisplayMode.Frequency:
          break;
        case DisplayMode.Amplitude:
          break;

        case DisplayMode.CutFill:
          // TODO This needs to be completed to define the sets of cut/fill shells defined in the settings.
          palettes = new List<ColorPalette>();
          for (int i = 0; i < cs.cutFillColors.Count; i++)
            palettes.Add(new ColorPalette(cs.cutFillColors[i].color, cs.cutFillColors[i].value));
          break;

        case DisplayMode.Moisture:
          break;
        case DisplayMode.TemperatureSummary:
          // ajr14976

          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.temperatureMinimumColor, 0),
            new ColorPalette(cs.temperatureTargetColor, 1),
            new ColorPalette(cs.temperatureMaximumColor, 2)
          };

          break;
        case DisplayMode.GPSMode:
          break;
        case DisplayMode.CCVSummary:
        case DisplayMode.CCVPercentSummary:
          // Hard code the summary Colors into a transitions palette for now to push it through the current pallete transfer machanism in 
          // the tile requests. The tile processor will unpack it into an appropriate structure on the Raptor side.
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.ccvSummaryCompleteLayerColor, 0),
            new ColorPalette(cs.ccvSummaryWorkInProgressLayerColor, 1),
            new ColorPalette(cs.ccvSummaryUndercompactedLayerColor, 2),
            new ColorPalette(cs.ccvSummaryOvercompactedLayerColor, 3),
            new ColorPalette(cs.ccvSummaryTooThickLayerColor, 4),
            new ColorPalette(cs.ccvSummaryApprovedLayerColor, 5)
          };
          break;
        case DisplayMode.CompactionCoverage:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.coverageColor, 0),
            new ColorPalette(cs.surveyedSurfaceColor, 1)
          };
          break;
        case DisplayMode.TargetThicknessSummary:
        case DisplayMode.VolumeCoverage:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.volumeSummaryCoverageColor, 0),
            new ColorPalette(cs.volumeSummaryVolumeColor, 1),
            new ColorPalette(cs.volumeSummaryNoChangeColor, 2)
          };
          break;
        case DisplayMode.MDP:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.mdpMinimum.color, cs.mdpMinimum.value),
            new ColorPalette(cs.mdpTarget.color, 0.9 * cs.mdpTarget.value),
            new ColorPalette(cs.mdpTarget.color, 1.1 * cs.mdpTarget.value),
            new ColorPalette(cs.mdpMaximum.color, cs.mdpMaximum.value)
          };
          break;
        case DisplayMode.MDPPercent:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.mdpPercentMinimum.color, cs.mdpPercentMinimum.value),
            new ColorPalette(cs.mdpPercentTarget.color, 0.9 * cs.mdpPercentTarget.value),
            new ColorPalette(cs.mdpPercentTarget.color, 1.1 * cs.mdpPercentTarget.value),
            new ColorPalette(cs.mdpPercentMaximum.color, cs.mdpPercentMaximum.value)
          };
          break;
        case DisplayMode.MDPSummary:
        case DisplayMode.MDPPercentSummary:
          // Hard code the summary Colors into a transitions palette for now to push it through the current pallete transfer machanism in 
          // the tile requests. The tile processor will unpack it into an appropriate structure on the Raptor side.
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.mdpSummaryCompleteLayerColor, 0),
            new ColorPalette(cs.mdpSummaryWorkInProgressLayerColor, 1),
            new ColorPalette(cs.mdpSummaryUndercompactedLayerColor, 2),
            new ColorPalette(cs.mdpSummaryOvercompactedLayerColor, 3),
            new ColorPalette(cs.mdpSummaryTooThickLayerColor, 4),
            new ColorPalette(cs.mdpSummaryApprovedLayerColor, 5)
          };
          break;
        case DisplayMode.MachineSpeed:
          palettes = new List<ColorPalette>();

          for (int i = cs.machineSpeedColors.Count - 1; i >= 0; i--)
            palettes.Insert(cs.machineSpeedColors.Count - i - 1,
              new ColorPalette(cs.machineSpeedColors[i].color, cs.machineSpeedColors[i].value));
          break;
        case DisplayMode.TargetSpeedSummary:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(cs.machineSpeedMinimumColor, 0),
            new ColorPalette(cs.machineSpeedTargetColor, 1),
            new ColorPalette(cs.machineSpeedMaximumColor, 2)
          };
          break;
        case DisplayMode.TemperatureDetail:
          palettes = new List<ColorPalette>
          {
            new ColorPalette(0x01579B, 0),
            new ColorPalette(0x039BE5, 500),
            new ColorPalette(0xB3E5FC, 1000),
            new ColorPalette(0x8BC34A, 1500),
            new ColorPalette(0xFFCDD2, 2000),
            new ColorPalette(0xE57373, 2500),
            new ColorPalette(0xD50000, 3000)
          };
          break;
      }

      return palettes;
    }


    public static IEnumerable<MachineDetails> converMachineDetails(TMachineDetail[] machineIDs)
    {
      foreach (TMachineDetail machineDetail in machineIDs)
        yield return
          new MachineDetails(machineDetail.ID, machineDetail.Name, machineDetail.IsJohnDoeMachine);
    }

    public static BoundingBox3DGrid ConvertExtents(T3DBoundingWorldExtent extents)
    {
      return new BoundingBox3DGrid(
        extents.MinX,
        extents.MinY,
        extents.MinZ,
        extents.MaxX,
        extents.MaxY,
        extents.MaxZ
      );
    }

    public static DisplayMode convertDisplayMode(TICDisplayMode mode)
    {
      switch (mode)
      {
        case TICDisplayMode.icdmHeight: return DisplayMode.Height;
        case TICDisplayMode.icdmCCV: return DisplayMode.CCV;
        case TICDisplayMode.icdmCCVPercent: return DisplayMode.CCVPercent;
        case TICDisplayMode.icdmLatency: return DisplayMode.Latency;
        case TICDisplayMode.icdmPassCount: return DisplayMode.PassCount;
        case TICDisplayMode.icdmRMV: return DisplayMode.RMV;
        case TICDisplayMode.icdmFrequency: return DisplayMode.Frequency;
        case TICDisplayMode.icdmAmplitude: return DisplayMode.Amplitude;
        case TICDisplayMode.icdmCutFill: return DisplayMode.CutFill;
        case TICDisplayMode.icdmMoisture: return DisplayMode.Moisture;
        case TICDisplayMode.icdmTemperatureSummary: return DisplayMode.TemperatureSummary;
        case TICDisplayMode.icdmGPSMode: return DisplayMode.GPSMode;
        case TICDisplayMode.icdmCCVSummary: return DisplayMode.CCVSummary;
        case TICDisplayMode.icdmCCVPercentSummary:
          return DisplayMode.CCVPercentSummary; // This is a synthetic display mode for CCV summary
        case TICDisplayMode.icdmPassCountSummary:
          return DisplayMode.PassCountSummary; // This is a synthetic display mode for Pass Count summary
        case TICDisplayMode.icdmCompactionCoverage:
          return DisplayMode.CompactionCoverage; // This ia a synthetic display mode for Compaction Coverage
        case TICDisplayMode.icdmVolumeCoverage:
          return DisplayMode.VolumeCoverage; // This is a synthetic display mode for Volumes Coverage
        case TICDisplayMode.icdmMDP: return DisplayMode.MDP;
        case TICDisplayMode.icdmMDPSummary: return DisplayMode.MDPSummary;
        case TICDisplayMode.icdmMDPPercent: return DisplayMode.MDPPercent;
        case TICDisplayMode.icdmMDPPercentSummary:
          return DisplayMode.MDPPercentSummary; // This is a synthetic display mode for MDP summary
        case TICDisplayMode.icdmCellProfile: return DisplayMode.CellProfile;
        case TICDisplayMode.icdmCellPasses: return DisplayMode.CellPasses;
        case TICDisplayMode.icdmMachineSpeed: return DisplayMode.MachineSpeed;
        case TICDisplayMode.icdmCCVPercentChange: return DisplayMode.CCVPercentChange;
        case TICDisplayMode.icdmTargetThicknessSummary: return DisplayMode.TargetThicknessSummary;
        case TICDisplayMode.icdmTargetSpeedSummary: return DisplayMode.TargetSpeedSummary;
        case TICDisplayMode.icdmCCVChange: return DisplayMode.CMVChange;
        case TICDisplayMode.icdmCCA: return DisplayMode.CCA;
        case TICDisplayMode.icdmCCASummary: return DisplayMode.CCASummary;
        case TICDisplayMode.icdmTemperatureDetail: return DisplayMode.TemperatureDetail;
        case TICDisplayMode.icdm3DTerrain: return DisplayMode.Terrain3D;
        case TICDisplayMode.icdm3DDesign: return DisplayMode.Design3D;
        default: throw new Exception($"Unknown TICDisplayMode {Convert.ToInt16(mode)}");
      }
    }

    public static TICDisplayMode convertDisplayMode(DisplayMode mode)
    {
      switch (mode)
      {
        case DisplayMode.Height: return TICDisplayMode.icdmHeight;
        case DisplayMode.CCV: return TICDisplayMode.icdmCCV;
        case DisplayMode.CCVPercent: return TICDisplayMode.icdmCCVPercent;
        case DisplayMode.Latency: return TICDisplayMode.icdmLatency;
        case DisplayMode.PassCount: return TICDisplayMode.icdmPassCount;
        case DisplayMode.RMV: return TICDisplayMode.icdmRMV;
        case DisplayMode.Frequency: return TICDisplayMode.icdmFrequency;
        case DisplayMode.Amplitude: return TICDisplayMode.icdmAmplitude;
        case DisplayMode.CutFill: return TICDisplayMode.icdmCutFill;
        case DisplayMode.Moisture: return TICDisplayMode.icdmMoisture;
        case DisplayMode.TemperatureSummary: return TICDisplayMode.icdmTemperatureSummary;
        case DisplayMode.GPSMode: return TICDisplayMode.icdmGPSMode;
        case DisplayMode.CCVSummary: return TICDisplayMode.icdmCCVSummary;
        case DisplayMode.CCVPercentSummary:
          return TICDisplayMode.icdmCCVPercentSummary; // This is a synthetic display mode for CCV summary
        case DisplayMode.PassCountSummary:
          return TICDisplayMode.icdmPassCountSummary; // This is a synthetic display mode for Pass Count summary
        case DisplayMode.CompactionCoverage:
          return TICDisplayMode.icdmCompactionCoverage; // This ia a synthetic display mode for Compaction Coverage
        case DisplayMode.VolumeCoverage:
          return TICDisplayMode.icdmVolumeCoverage; // This is a synthetic display mode for Volumes Coverage
        case DisplayMode.MDP: return TICDisplayMode.icdmMDP;
        case DisplayMode.MDPSummary: return TICDisplayMode.icdmMDPSummary;
        case DisplayMode.MDPPercent: return TICDisplayMode.icdmMDPPercent;
        case DisplayMode.MDPPercentSummary:
          return TICDisplayMode.icdmMDPPercentSummary; // This is a synthetic display mode for MDP summary
        case DisplayMode.CellProfile: return TICDisplayMode.icdmCellProfile;
        case DisplayMode.CellPasses: return TICDisplayMode.icdmCellPasses;
        case DisplayMode.MachineSpeed: return TICDisplayMode.icdmMachineSpeed;
        case DisplayMode.CCVPercentChange: return TICDisplayMode.icdmCCVPercentChange;
        case DisplayMode.TargetThicknessSummary: return TICDisplayMode.icdmTargetThicknessSummary;
        case DisplayMode.TargetSpeedSummary: return TICDisplayMode.icdmTargetSpeedSummary;
        case DisplayMode.CMVChange: return TICDisplayMode.icdmCCVChange;
        case DisplayMode.CCA: return TICDisplayMode.icdmCCA;
        case DisplayMode.CCASummary: return TICDisplayMode.icdmCCASummary;
        case DisplayMode.TemperatureDetail: return TICDisplayMode.icdmTemperatureDetail;
        case DisplayMode.Terrain3D: return TICDisplayMode.icdm3DTerrain;
        case DisplayMode.Design3D: return TICDisplayMode.icdm3DDesign;
        default: throw new Exception($"Unknown DisplayMode {Convert.ToInt16(mode)}");
      }
    }


    public static TWGS84Point ConvertWGSPoint(WGSPoint point)
    {
      return new TWGS84Point
      {
        Lat = point.Lat,
        Lon = point.Lon
      };
    }

    /// <summary>
    /// Convert <see cref="WGS84Fence"/> fence to <see cref="TWGS84FenceContainer"/> container.
    /// </summary>
    public static TWGS84FenceContainer ConvertWGS84Fence(WGS84Fence fence)
    {
      return new TWGS84FenceContainer
      {
        FencePoints = fence.Points.ToList().ConvertAll(p => new TWGS84Point
        {
          Lat = p.Lat,
          Lon = p.Lon
        }).ToArray()
      };
    }

    private static TICFilterSettings DefaultRaptorFilter => new TICFilterSettings
      {LayerMethod = TFilterLayerMethod.flmAutoMapReset};

    /// <summary>
    /// Convert <see cref="FilterResult"/> filter object to a Raptor compatible <see cref="TICFilterSettings"/> filter.
    /// </summary>
    public static TICFilterSettings ConvertFilter(FilterResult filterResult, long? legacyProjectId = null, IASNodeClient raptorClient = null, DateTime? overrideStartUTC = null,
      DateTime? overrideEndUTC = null, List<long> overrideAssetIds = null, string fileSpaceName = null)
    {
      if (filterResult == null) return DefaultRaptorFilter;

      TICFilterSettings filter = DefaultRaptorFilter;
      List<TMachineDetail> assetList = null;

      if (overrideStartUTC.HasValue)
      {
        filter.StartTime = overrideStartUTC.Value;
        filter.SetTimeCellpassState(true);
      }
      else if (filterResult.StartUtc.HasValue)
      {
        filter.StartTime = filterResult.StartUtc.Value;
        filter.SetTimeCellpassState(true);
      }

      if (overrideEndUTC.HasValue)
      {
        filter.EndTime = overrideEndUTC.Value;
        filter.SetTimeCellpassState(true);
      }
      else if (filterResult.EndUtc.HasValue)
      {
        filter.EndTime = filterResult.EndUtc.Value;
        filter.SetTimeCellpassState(true);
      }

      // Currently the Raptor code only supports filtering on a single Machine Design
      if (!string.IsNullOrEmpty(filterResult.OnMachineDesignName))
      {
        filter.DesignNameID = GetRaptorDesignId(raptorClient, legacyProjectId, filterResult.OnMachineDesignId, filterResult.OnMachineDesignName);
        filter.SetDesignNameCellpassState(true);
      }
      else
        if (filterResult.OnMachineDesignId.HasValue)
        {
          filter.DesignNameID =
            (int)filterResult.OnMachineDesignId
              .Value; // (Aaron) Possible mismatch here, OnMachineDesignId is a long?. Won't fit into int...
          filter.SetDesignNameCellpassState(true);
        }

      if (filterResult.AssetIDs != null && filterResult.AssetIDs.Count > 0)
      {
        assetList = (from a in filterResult.AssetIDs
          select new TMachineDetail
          {
            Name = string.Empty,
            ID = a,
            IsJohnDoeMachine = false
          }).ToList();
      }

      if (filterResult.ContributingMachines != null && filterResult.ContributingMachines.Count > 0)
      {
        var machineList = (from c in filterResult.ContributingMachines
          select new TMachineDetail
          {
            Name = c.MachineName,
            ID = c.AssetId,
            IsJohnDoeMachine = c.IsJohnDoe
          }).ToList();
        if (assetList == null)
          assetList = machineList;
        else
          assetList.AddRange(machineList);
      }

      if (overrideAssetIds != null && overrideAssetIds.Count > 0)
      {
        if (assetList == null)
        {
          assetList = (from a in overrideAssetIds
            select new TMachineDetail
            {
              Name = string.Empty,
              ID = a,
              IsJohnDoeMachine = false
            }).ToList();
        }
        else
        {
          //Both project filter and report have assets selected so use intersection
          assetList = (from a in assetList where overrideAssetIds.Contains(a.ID) select a).ToList();
        }
      }

      if (filterResult.CompactorDataOnly.HasValue)
      {
        filter.SetCompactionMachinesOnlyState(filterResult.CompactorDataOnly.Value);
      }

      if (filterResult.VibeStateOn.HasValue)
      {
        filter.VibeState = filterResult.VibeStateOn.Value ? TICVibrationState.vsOn : TICVibrationState.vsOff;
        filter.SetVibeStateCellpassState(true);
      }

      if (filterResult.ElevationType.HasValue)
      {
        filter.ElevationType = ConvertElevationType(filterResult.ElevationType.Value);
        filter.SetElevationTypeCellpassState(true);
      }

      //Note: the SiteID is only used for the UI. The points of the site or user-defined polygon are in Polygon.
      if (filterResult.PolygonLL != null && filterResult.PolygonLL.Count > 0)
      {
        //NOTE: There is an inconsistency inherited from VL where the filter is passed to Raptor with decimal degrees.
        //All other lat/lngs in Shim calls are passed to Raptor as radians. Since we now have consistency in the Raptor
        //services where everything is radians we need to convert to decimal degrees here for the filter to match VL.
        foreach (WGSPoint p in filterResult.PolygonLL)
        {
          filter.Fence.Add(new TFencePoint(p.Lon * Coordinates.RADIANS_TO_DEGREES,
            p.Lat * Coordinates.RADIANS_TO_DEGREES, 0));
        }

        filter.SetPositionalCellSpatialSelectionState(true);
        filter.CoordsAreGrid = false;
      }
      else
      {
        if (filterResult.PolygonGrid != null && filterResult.PolygonGrid.Count > 0)
        {
          foreach (Point p in filterResult.PolygonGrid)
          {
            filter.Fence.Add(new TFencePoint(p.x, p.y, 0));
          }

          filter.SetPositionalCellSpatialSelectionState(true);
          filter.CoordsAreGrid = true;
        }
      }

      if (filterResult.ForwardDirection.HasValue)
      {
        filter.MachineDirection = filterResult.ForwardDirection.Value
          ? TICMachineDirection.mdForward
          : TICMachineDirection.mdReverse;
        filter.SetMachineDirectionCellpassState(true);
      }

      if (filterResult.AlignmentFile != null && filterResult.StartStation.HasValue &&
          filterResult.EndStation.HasValue && filterResult.LeftOffset.HasValue && filterResult.RightOffset.HasValue)
      {
        filter.ReferenceDesign = DesignDescriptor(filterResult.AlignmentFile);
        filter.StartStation = filterResult.StartStation.Value;
        filter.EndStation = filterResult.EndStation.Value;
        filter.LeftOffset = filterResult.LeftOffset.Value;
        filter.RightOffset = filterResult.RightOffset.Value;

        filter.SetDesignMaskCellSelectionState(true);
      }

      // Layer Analysis
      if (filterResult.LayerType.HasValue)
      {
        filter.LayerMethod = ConvertLayerMethod(filterResult.LayerType.Value);
        filter.LayerState = TICLayerState.lsOn;

        if (filter.LayerMethod == TFilterLayerMethod.flmOffsetFromDesign ||
            filter.LayerMethod == TFilterLayerMethod.flmOffsetFromBench ||
            filter.LayerMethod == TFilterLayerMethod.flmOffsetFromProfile)
        {
          if (filter.LayerMethod == TFilterLayerMethod.flmOffsetFromBench)
          {
            filter.ElevationRangeLevel = filterResult.BenchElevation.HasValue ? filterResult.BenchElevation.Value : 0;
          }
          else
          {
            filter.ElevationRangeDesign = DesignDescriptor(filterResult.LayerDesignOrAlignmentFile);
          }

          if (filterResult.LayerNumber.HasValue && filterResult.LayerThickness.HasValue)
          {
            int layerNumber = filterResult.LayerNumber.Value < 0
              ? filterResult.LayerNumber.Value + 1
              : filterResult.LayerNumber.Value;
            filter.ElevationRangeOffset = layerNumber * filterResult.LayerThickness.Value;
            filter.ElevationRangeThickness = filterResult.LayerThickness.Value;
          }
          else
          {
            filter.ElevationRangeOffset = 0;
            filter.ElevationRangeThickness = 0;
          }

          filter.SetElevationRangeCellPassState(true);
        }
        else if (filter.LayerMethod == TFilterLayerMethod.flmTagfileLayerNumber)
        {
          filter.LayerID = filterResult.LayerNumber.Value;
          filter.PassFilterSelections = filter.PassFilterSelections.Set(TICFilterPassSelection.icfsLayerID);

        }
      }
      else
      {
        filter.LayerState = TICLayerState.lsOff;
      }

      if (filterResult.GpsAccuracy.HasValue)
      {
        //TODO Do safe casting here
        filter.GPSAccuracy = ((TICGPSAccuracy) filterResult.GpsAccuracy);
        filter.GPSAccuracyIsInclusive = filterResult.GpsAccuracyIsInclusive ?? false;
        filter.PassFilterSelections = filter.PassFilterSelections.Set(TICFilterPassSelection.icfsGPSAccuracy);
      }

      if (filterResult.BladeOnGround.HasValue && filterResult.BladeOnGround.Value)
      {
        filter.SetPassTypeState(true);
        filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptFront);
        filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptRear);
      }

      if (filterResult.TrackMapping.HasValue && filterResult.TrackMapping.Value)
      {
        filter.SetPassTypeState(true);
        filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptTrack);
      }

      if (filterResult.WheelTracking.HasValue && filterResult.WheelTracking.Value)
      {
        filter.SetPassTypeState(true);
        filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptWheel);
      }

      if (filterResult.DesignFile != null)
      {
        filter.DesignFilter = VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(
          filterResult.DesignFile.Id,
          fileSpaceName,
          filterResult.DesignFile.File.FilespaceId,
          filterResult.DesignFile.File.Path,
          filterResult.DesignFile.File.FileName,
          filterResult.DesignFile.Offset);

        filter.SetDesignFilterMaskCellSelectionState(true);
      }

      if ((filterResult != null) && (filterResult.SurveyedSurfaceExclusionList != null))
      {
        filter.SurveyedSurfaceExclusionList = (from a in filterResult.SurveyedSurfaceExclusionList
          select new TSurveyedSurfaceID {SurveyedSurfaceID = a}).ToArray();
      }

      if (assetList != null)
      {
        filter.Machines = assetList.ToArray();
        filter.SetDesignMachineCellpassState(true);
      }

      filter.ReturnEarliestFilteredCellPass = (filterResult != null) && filterResult.ReturnEarliest.HasValue &&
                                              filterResult.ReturnEarliest.Value;

      if (filterResult.AutomaticsType.HasValue)
      {
        filter.GCSGuidanceMode = (TGCSAutomaticsMode) filterResult.AutomaticsType.Value;
        filter.SetGCSGuidanceModeCellpassState(true);
      }

      if (filterResult.TemperatureRangeMin.HasValue && filterResult.TemperatureRangeMax.HasValue)
      {
        filter.TemperatureRangeMin = (ushort) (filterResult.TemperatureRangeMin.Value * 10);
        filter.TemperatureRangeMax = (ushort) (filterResult.TemperatureRangeMax.Value * 10);
        filter.SetTemperatureRangeState(true);
      }

      if (filterResult.PassCountRangeMin.HasValue && filterResult.PassCountRangeMax.HasValue)
      {
        filter.PassCountRangeMin = filterResult.PassCountRangeMin.Value;
        filter.PassCountRangeMax = filterResult.PassCountRangeMax.Value;
        filter.SetPassCountRangeState(true);
      }

      return filter;
    }

    public static TICLiftBuildSettings ConvertLift(LiftBuildSettings settings, TFilterLayerMethod layerMethod)
    {
      var result = settings == null
        ? new TSVOICOptions().GetLiftBuildSettings(layerMethod)
        : new TICLiftBuildSettings
        {
          CCVRange = ConvertCCVRange(settings.CCVRange),
          CCVSummarizeTopLayerOnly = settings.CCVSummarizeTopLayerOnly,
          DeadBandLowerBoundary = settings.DeadBandLowerBoundary,
          DeadBandUpperBoundary = settings.DeadBandUpperBoundary,
          FirstPassThickness = settings.FirstPassThickness,
          LiftDetectionType = ConvertLiftDetectionType(settings.LiftDetectionType),
          LiftThicknessType = ConvertLiftThicknessType(settings.LiftThicknessType),
          MDPRange = ConvertMDPRange(settings.MDPRange),
          MDPSummarizeTopLayerOnly = settings.MDPSummarizeTopLayerOnly,
          OverrideMachineCCV = settings.OverridingMachineCCV.HasValue,
          OverrideMachineMDP = settings.OverridingMachineMDP.HasValue,
          OverrideTargetPassCount = settings.OverridingTargetPassCountRange != null,
          OverrideTemperatureWarningLevels = settings.OverridingTemperatureWarningLevels != null,
          OverridingLiftThickness = settings.OverridingLiftThickness ?? 0f,
          OverridingMachineCCV = settings.OverridingMachineCCV ?? 0,
          OverridingMachineMDP = settings.OverridingMachineMDP ?? 0,
          OverridingTargetPassCountRange = ConvertTargetPassCountRange(settings.OverridingTargetPassCountRange),
          OverridingTemperatureWarningLevels =
            ConvertTemperatureWarningLevels(settings.OverridingTemperatureWarningLevels),
          IncludeSuperseded = settings.IncludeSupersededLifts ?? false,
          TargetLiftThickness = settings.LiftThicknessTarget?.TargetLiftThickness ?? SVOICDecls.__Global.kICNullHeight,
          AboveToleranceLiftThickness = settings.LiftThicknessTarget?.AboveToleranceLiftThickness ?? 0,
          BelowToleranceLiftThickness = settings.LiftThicknessTarget?.BelowToleranceLiftThickness ?? 0,
          TargetMaxMachineSpeed = settings.MachineSpeedTarget?.MaxTargetMachineSpeed ?? 0,
          TargetMinMachineSpeed = settings.MachineSpeedTarget?.MinTargetMachineSpeed ?? 0,
        };

      if (settings?.CCvSummaryType != null)
        result.CCVSummaryTypes = result.CCVSummaryTypes.Set((int) settings.CCvSummaryType);

      return result;
    }


    public static TCCVRangePercentage ConvertCCVRange(CCVRangePercentage range)
    {
      return range == null
        ? new TCCVRangePercentage
        {
          Min = 0,
          Max = 100
        }
        : new TCCVRangePercentage
        {
          Min = range.Min,
          Max = range.Max
        };
    }

    public static TMDPRangePercentage ConvertMDPRange(MDPRangePercentage range)
    {
      return range == null
        ? new TMDPRangePercentage
        {
          Min = 0,
          Max = 100
        }
        : new TMDPRangePercentage
        {
          Min = range.Min,
          Max = range.Max
        };
    }

    public static TTargetPassCountRange ConvertTargetPassCountRange(TargetPassCountRange range)
    {
      return range == null
        ? new TTargetPassCountRange
        {
          Min = 1,
          Max = ushort.MaxValue
        }
        : new TTargetPassCountRange
        {
          Min = range.Min,
          Max = range.Max
        };
    }

    public static TICLiftDetectionType ConvertLiftDetectionType(LiftDetectionType type)
    {
      switch (type)
      {
        case LiftDetectionType.None: return TICLiftDetectionType.icldtNone;
        case LiftDetectionType.Automatic: return TICLiftDetectionType.icldtAutomatic;
        case LiftDetectionType.MapReset: return TICLiftDetectionType.icldtMapReset;
        case LiftDetectionType.AutoMapReset: return TICLiftDetectionType.icldtAutoMapReset;
        case LiftDetectionType.Tagfile: return TICLiftDetectionType.icldtTagfile;
        default: throw new Exception($"Unknown lift detection type: {Convert.ToInt32(type)}");
      }
    }

    public static int ConvertLiftThicknessType(LiftThicknessType type)
    {
      switch (type)
      {
        case LiftThicknessType.Compacted: return __Global.lttCompacted;
        case LiftThicknessType.Uncompacted: return __Global.lttUncompacted;
        default: throw new Exception($"Unknown lift thickness type: {Convert.ToInt32(type)}");
      }
    }

    public static TTemperatureWarningLevels ConvertTemperatureWarningLevels(TemperatureWarningLevels levels)
    {
      return levels == null
        ? new TTemperatureWarningLevels
        {
          Min = 0,
          Max = 100
        }
        : new TTemperatureWarningLevels
        {
          Min = levels.Min,
          Max = levels.Max
        };
    }

    public static TICElevationType ConvertElevationType(ElevationType type)
    {
      switch (type)
      {
        case ElevationType.First: return TICElevationType.etFirst;
        case ElevationType.Last: return TICElevationType.etLast;
        case ElevationType.Highest: return TICElevationType.etHighest;
        case ElevationType.Lowest: return TICElevationType.etLowest;
        default: throw new Exception($"Unknown elevation type: {Convert.ToInt32(type)}");
      }
    }

    public static TFilterLayerMethod ConvertLayerMethod(FilterLayerMethod method)
    {
      switch (method)
      {
        case FilterLayerMethod.Invalid: return TFilterLayerMethod.flmInvalid;
        case FilterLayerMethod.None: return TFilterLayerMethod.flmNone;
        case FilterLayerMethod.AutoMapReset: return TFilterLayerMethod.flmAutoMapReset;
        case FilterLayerMethod.Automatic: return TFilterLayerMethod.flmAutomatic;
        case FilterLayerMethod.MapReset: return TFilterLayerMethod.flmMapReset;
        case FilterLayerMethod.OffsetFromBench: return TFilterLayerMethod.flmOffsetFromBench;
        case FilterLayerMethod.OffsetFromDesign: return TFilterLayerMethod.flmOffsetFromDesign;
        case FilterLayerMethod.OffsetFromProfile: return TFilterLayerMethod.flmOffsetFromProfile;
        case FilterLayerMethod.TagfileLayerNumber: return TFilterLayerMethod.flmTagfileLayerNumber;
        default: throw new Exception($"Unknown layer method: {Convert.ToInt32(method)}");
      }
    }

    public static TVLPDDesignDescriptor DesignDescriptor(long designID, string filespaceId, string path,
      string fileName, double offset)
    {
      return VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(designID, "RaptorServices", filespaceId, path, fileName,
        offset);
    }

    public static TVLPDDesignDescriptor EmptyDesignDescriptor =>
      DesignDescriptor(0, string.Empty, string.Empty, string.Empty, 0);

    public static TVLPDDesignDescriptor DesignDescriptor(DesignDescriptor dd)
    {
      if (dd == null)
      {
        return EmptyDesignDescriptor;
      }

      return dd.File == null
        ? DesignDescriptor(dd.Id, string.Empty, string.Empty, string.Empty, dd.Offset)
        : DesignDescriptor(dd.Id, dd.File.FilespaceId, dd.File.Path, dd.File.FileName, dd.Offset);
    }

    /// <summary>
    /// ConvertVolumesType
    /// </summary>
    /// <param name="volumesType"></param>
    /// <returns></returns>
    public static TComputeICVolumesType ConvertVolumesType(VolumesType volumesType)
    {
      switch (volumesType)
      {
        case VolumesType.None: return TComputeICVolumesType.ic_cvtNone;
        case VolumesType.AboveLevel: return TComputeICVolumesType.ic_cvtAboveLevel;
        case VolumesType.Between2Levels: return TComputeICVolumesType.ic_cvtBetween2Levels;
        case VolumesType.AboveFilter: return TComputeICVolumesType.ic_cvtAboveFilter;
        case VolumesType.Between2Filters: return TComputeICVolumesType.ic_cvtBetween2Filters;
        case VolumesType.BetweenFilterAndDesign: return TComputeICVolumesType.ic_cvtBetweenFilterAndDesign;
        case VolumesType.BetweenDesignAndFilter: return TComputeICVolumesType.ic_cvtBetweenDesignAndFilter;
        default: throw new Exception($"Unknown VolumesType {Convert.ToInt16(volumesType)}");
      }
    }

    /// <summary>
    /// convertSurveyedSurfaceExlusionList
    /// </summary>
    /// <param name="exclusions"></param>
    /// <returns></returns>
    public static TSurveyedSurfaceID[] convertSurveyedSurfaceExlusionList(long[] exclusions)
    {
      if (exclusions == null) return new TSurveyedSurfaceID[0];
      TSurveyedSurfaceID[] result = new TSurveyedSurfaceID[exclusions.Length];
      for (int i = 0; i < exclusions.Length; i++)
        result[i].SurveyedSurfaceID = exclusions[i];

      return result;
    }

    /// <summary>
    /// convertSurveyedSurfaceExlusionList
    /// </summary>
    /// <param name="exclusions"></param>
    /// <returns></returns>
    public static TSurveyedSurfaceID[] convertSurveyedSurfaceExlusionList(List<long> exclusions)
    {
      if (exclusions == null) return new TSurveyedSurfaceID[0];
      TSurveyedSurfaceID[] result = new TSurveyedSurfaceID[exclusions.Count];
      for (int i = 0; i < exclusions.Count; i++)
        result[i].SurveyedSurfaceID = exclusions[i];

      return result;
    }

    /// <summary>
    /// convertCellAddress
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static CellAddress convertCellAddress(TSubGridCellAddress address)
    {
      return CellAddress.CreateCellAddress(address.X, address.Y);
    }

    /// <summary>
    /// convertCellAddress
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static TSubGridCellAddress convertCellAddress(CellAddress address)
    {
      return new TSubGridCellAddress
      {
        X = address.x,
        Y = address.y
      };
    }

    public static TSVOICOptions convertOptions(ColorSettingsFlags colorSettingsFlags,
      LiftBuildSettings liftSettings,
      double volumesNoChangeTolerance,
      FilterLayerMethod filterLayerMethod,
      DisplayMode mode,
      bool setSummaryDataLayersVisibility)
    {
      TSVOICOptions options = new TSVOICOptions();

      if (colorSettingsFlags == null)
      {
        colorSettingsFlags = new ColorSettingsFlags();
        if (setSummaryDataLayersVisibility)
        {
          colorSettingsFlags.ccvSummaryWorkInProgressLayerVisible = true;
          colorSettingsFlags.ccvSummaryTooThickLayerVisible = true;
          colorSettingsFlags.mdpSummaryWorkInProgressLayerVisible = true;
          colorSettingsFlags.mdpSummaryTooThickLayerVisible = true;
        }
      }

      ;

      if (colorSettingsFlags != null && liftSettings != null)
      {
        // Important Note! due to some values been used in a 10th precision in the backend raptor
        // e.g. a user cmv override target setting value of 100 is actually has a value of 1000 in raptor
        // therefore some settings get multiplied by 10. Ideally it would have been stored in that precision but it's too late now

        options.AbsoluteTargetCCV = liftSettings.OverridingMachineCCV.HasValue
          ? liftSettings.OverridingMachineCCV.Value
          : (short) 0;
        ////(short)Math.Round(liftSettings.overridingMachineCCV.Value * 10);

        options.CCVDecouplingColour = (int) Colors.Black;
        options.CCVRange = ConvertCCVRange(liftSettings.CCVRange);
        options.CCVSummarizeTopLayerOnly = liftSettings.CCVSummarizeTopLayerOnly;
        options.CCVTargetColour = (int) Colors.Green;
        options.FirstPassThickness = (float) liftSettings.FirstPassThickness;
        switch (filterLayerMethod)
        {
          case FilterLayerMethod.None:
            options.LiftDetectionType = TICLiftDetectionType.icldtNone;
            break;
          case FilterLayerMethod.Automatic:
            options.LiftDetectionType = TICLiftDetectionType.icldtAutomatic;
            break;
          case FilterLayerMethod.MapReset:
            options.LiftDetectionType = TICLiftDetectionType.icldtMapReset;
            break;
          case FilterLayerMethod.TagfileLayerNumber:
            options.LiftDetectionType = TICLiftDetectionType.icldtTagfile;
            break;
          case FilterLayerMethod.AutoMapReset:
          default:
            options.LiftDetectionType = TICLiftDetectionType.icldtAutoMapReset;
            break;
        }

        options.LiftThicknessMachine = !liftSettings.OverridingLiftThickness.HasValue;
        options.LiftThicknessType = ConvertLiftThicknessType(liftSettings.LiftThicknessType);
        options.MaximumLiftThickness = liftSettings.OverridingLiftThickness.HasValue
          ? liftSettings.OverridingLiftThickness.Value
          : 0f;

        options.PassTargetColour = (int) Colors.Green;
        options.TargetPassCountRange = ConvertTargetPassCountRange(liftSettings.OverridingTargetPassCountRange);
        options.UseMachineTargetCCV = !liftSettings.OverridingMachineCCV.HasValue;
        options.UseMachineTargetPass = liftSettings.OverridingTargetPassCountRange == null;

        options.SetCCVSummaryTypeWIP(colorSettingsFlags.ccvSummaryWorkInProgressLayerVisible);
        options.SetCCVSummaryTypeThickness(colorSettingsFlags.ccvSummaryTooThickLayerVisible);

        if (mode == DisplayMode.TargetThicknessSummary)
        {
          options.TargetLiftThickness = liftSettings.LiftThicknessTarget.TargetLiftThickness;
          options.AboveToleranceLiftThickness = liftSettings.LiftThicknessTarget.AboveToleranceLiftThickness;
          options.BelowToleranceLiftThickness = liftSettings.LiftThicknessTarget.BelowToleranceLiftThickness;
        }
        else
          options.TargetLiftThickness = SVOICDecls.__Global.kICNullHeight;

        options.MinSpeedTarget = mode == DisplayMode.TargetSpeedSummary
          ? liftSettings.MachineSpeedTarget.MinTargetMachineSpeed
          : SVOICDecls.__Global.kICNullMachineSpeed;
        options.MaxSpeedTarget = mode == DisplayMode.TargetSpeedSummary
          ? liftSettings.MachineSpeedTarget.MaxTargetMachineSpeed
          : SVOICDecls.__Global.kICNullMachineSpeed;

        switch (mode) // for summary modes make sure at least compaction is set
        {
          case DisplayMode.CCVSummary:
          case DisplayMode.CCVPercentSummary:
            options.SetCCVSummaryTypeCompaction(true); // always set
            break;

          case DisplayMode.MDPSummary:
          case DisplayMode.MDPPercentSummary:
            options.SetMDPSummaryTypeCompaction(true); // always set
            break;
        }

        ;

        options.NoChangeVolumeTolerance = (float) volumesNoChangeTolerance;

        options.UseMachineTempWarningLevels = liftSettings.OverridingTemperatureWarningLevels == null;
        if (liftSettings.OverridingTemperatureWarningLevels != null)
        {
          options.TemperatureWarningLevels =
            ConvertTemperatureWarningLevels(liftSettings.OverridingTemperatureWarningLevels);
        }

        options.AbsoluteTargetMDP = liftSettings.OverridingMachineMDP.HasValue
          ? liftSettings.OverridingMachineMDP.Value
          : (short) 0;
        //(short)Math.Round(liftSettings.overrideTargetMDPValue.Value * 10);
        options.MDPRange = ConvertMDPRange(liftSettings.MDPRange);
        options.MDPSummarizeTopLayerOnly = liftSettings.MDPSummarizeTopLayerOnly;
        options.MDPTargetColour = (int) Colors.Green;
        options.UseMachineTargetMDP = !liftSettings.OverridingMachineMDP.HasValue;
        options.SetMDPSummaryTypeWIP(colorSettingsFlags.mdpSummaryWorkInProgressLayerVisible);
        options.SetMDPSummaryTypeThickness(colorSettingsFlags.mdpSummaryTooThickLayerVisible);

        options.DeadBandLowerBoundary = liftSettings.DeadBandLowerBoundary;
        options.DeadBandUpperBoundary = liftSettings.DeadBandUpperBoundary;

        options.IncludeSupersededLayers = liftSettings.IncludeSupersededLifts ?? false;
      }

      return options;
    }

    public static void convertGridOrLLBoundingBox(BoundingBox2DGrid grid, BoundingBox2DLatLon ll, out TWGS84Point bl,
      out TWGS84Point tr, out bool coordsAreGrid)
    {
      coordsAreGrid = grid != null;
      if (coordsAreGrid)
      {
        bl = TWGS84Point.PointXY(grid.BottomLeftX, grid.BottomleftY);
        tr = TWGS84Point.PointXY(grid.TopRightX, grid.TopRightY);
      }
      else if (ll != null)
      {
        bl = TWGS84Point.Point(ll.BottomLeftLon, ll.BottomLeftLat);
        tr = TWGS84Point.Point(ll.TopRightLon, ll.TopRightLat);
      }
      else
      {
        bl = TWGS84Point.Point(0, 0);
        tr = TWGS84Point.Point(0, 0);
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter for certain operations that involve design surfaces for tile rendering operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings topFilter,
      DisplayMode mode,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations that effect Cut/Fill, Volume and Thickness requests. 
      // If these requests invovle a design through the appropriate volume computation modes, the topFilter has no effect
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a top filter indication one of the comparative surfaces used by these requests
      if (((mode == DisplayMode.CutFill) || (mode == DisplayMode.VolumeCoverage) ||
           (mode == DisplayMode.TargetThicknessSummary))
          &&
          ((computeVolType == VolumesType.BetweenDesignAndFilter) ||
           (computeVolType == VolumesType.BetweenFilterAndDesign)))
      {
        // Force topfilter (which is filter2) to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        topFilter = new TICFilterSettings();
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter for certain operations that involve design surfaces for volume computation operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings topFilter,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume computations with respect to designs
      // If these requests invovle a design through the appropriate volume computation modes, the topFilter has no effect
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a top filter indication one of the comparative surfaces used by these requests
      if ((computeVolType == VolumesType.BetweenDesignAndFilter) ||
          (computeVolType == VolumesType.BetweenFilterAndDesign))
      {
        // Force topfilter (which is filter2) to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        topFilter = new TICFilterSettings();
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured filter2 for certain operations that involve design surfaces for tile rendering operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings filter1,
      ref TICFilterSettings filter2,
      DisplayMode mode,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations that effect Cut/Fill, Volume and Thickness requests. 
      // If these requests invovle a design through the appropriate volume computation modes, either the topFilter or the baseFilter
      // has no effect depending on the style of filter/design and design/filter chosen 
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a filter used by these requests
      if (((mode == DisplayMode.CutFill) || (mode == DisplayMode.VolumeCoverage) ||
           (mode == DisplayMode.TargetThicknessSummary)))
      {
        if (computeVolType == VolumesType.BetweenDesignAndFilter)
        {
          // Force topfilter to be a plain empty filter to remove any default
          // setting such as the LayerType to percolate through into the request.
          filter2 = new TICFilterSettings();
        }

        if (computeVolType == VolumesType.BetweenFilterAndDesign)
        {
          // Force basefilter to be a plain empty filter to remove any default
          // setting such as the LayerType to percolate through into the request.
          filter2 = new TICFilterSettings();
        }
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter or baseFilter for certain operations that involve design surfaces for volume computation operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings baseFilter,
      ref TICFilterSettings topFilter,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations respect to designs. 
      // If these requests invovle a design through the appropriate volume computation modes, either the topFilter or the baseFilter
      // has no effect depending on the style of filter/design and design/filter chosen 
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a filter used by these requests

      if (computeVolType == VolumesType.BetweenDesignAndFilter)
      {
        // Force topfilter to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        baseFilter = new TICFilterSettings();
      }

      if (computeVolType == VolumesType.BetweenFilterAndDesign)
      {
        // Force basefilter to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        topFilter = new TICFilterSettings();
      }
    }
#if RAPTOR
    public static CoordinateSystemDatumMethodType convertCoordinateSystemDatumMethodType(
      TCoordinateSystemDatumMethod type)
    {
      switch (type)
      {
        case TCoordinateSystemDatumMethod.csdmUnknown: return CoordinateSystemDatumMethodType.Unknown;
        case TCoordinateSystemDatumMethod.csdmWGS84Datum: return CoordinateSystemDatumMethodType.WGS84Datum;
        case TCoordinateSystemDatumMethod.csdmMolodenskyDatum: return CoordinateSystemDatumMethodType.MolodenskyDatum;
        case TCoordinateSystemDatumMethod.csdmMultipleRegressionDatum:
          return CoordinateSystemDatumMethodType.MultipleRegressionDatum;
        case TCoordinateSystemDatumMethod.csdmSevenParameterDatum:
          return CoordinateSystemDatumMethodType.SevenParameterDatum;
        case TCoordinateSystemDatumMethod.csdmGridDatum: return CoordinateSystemDatumMethodType.GridDatum;
        default: throw new Exception($"Unknown TCoordinateSystemDatumMethod type: {Convert.ToInt32(type)}");
      }
    }

    public static CoordinateSystemGeoidMethodType convertCoordinateSystemGeoidMethodType(
      TCoordinateSystemGeoidMethod type)
    {
      switch (type)
      {
        case TCoordinateSystemGeoidMethod.csgmUnknown: return CoordinateSystemGeoidMethodType.Unknown;
        case TCoordinateSystemGeoidMethod.csgmGridGeoid: return CoordinateSystemGeoidMethodType.GridGeoid;
        case TCoordinateSystemGeoidMethod.csgmConstantSeparationGeoid:
          return CoordinateSystemGeoidMethodType.ConstantSeparationGeoid;
        case TCoordinateSystemGeoidMethod.csgmSiteCalibratedGeoidRecord:
          return CoordinateSystemGeoidMethodType.SiteCalibratedGeoidRecord;
        default: throw new Exception($"Unknown TCoordinateSystemGeoidMethod type: {Convert.ToInt32(type)}");
      }
    }

    public static ProjectionParameter[] convertCoordinateSystemProjectionParameters(TProjectionParameters parameters)
    {
      return parameters.ProjectionParameters?.Select(pp => new ProjectionParameter()
      {
        Name = pp.Name,
        Value = pp.Value
      }).ToArray();
    }

    public static AutoStateType convertAutoStateType(TICAutoState type)
    {
      switch (type)
      {
        case TICAutoState.asOff: return AutoStateType.Off;
        case TICAutoState.asAuto: return AutoStateType.Auto;
        case TICAutoState.asManual: return AutoStateType.Manual;
        case TICAutoState.asUnknown: return AutoStateType.Unknown;
        default: throw new Exception($"Unknown TICAutoState type: {Convert.ToInt32(type)}");
      }
    }

    public static GCSAutomaticsModeType convertGCSAutomaticsModeType(TGCSAutomaticsMode type)
    {
      switch (type)
      {
        case TGCSAutomaticsMode.amUnknown: return GCSAutomaticsModeType.Unknown;
        case TGCSAutomaticsMode.amManual: return GCSAutomaticsModeType.Manual;
        case TGCSAutomaticsMode.amAutomatics: return GCSAutomaticsModeType.Automatic;
        default: throw new Exception($"Unknown TGCSAutomaticsMode type: {Convert.ToInt32(type)}");
      }
    }

    public static MachineGearType convertMachineGearType(TICMachineGear type)
    {
      switch (type)
      {
        case TICMachineGear.mgNeutral: return MachineGearType.Neutral;
        case TICMachineGear.mgForward: return MachineGearType.Forward;
        case TICMachineGear.mgReverse: return MachineGearType.Reverse;
        case TICMachineGear.mgSensorFailedDeprecated: return MachineGearType.SensorFailedDeprecated;
        case TICMachineGear.mgForward2: return MachineGearType.Forward2;
        case TICMachineGear.mgForward3: return MachineGearType.Forward3;
        case TICMachineGear.mgForward4: return MachineGearType.Forward4;
        case TICMachineGear.mgForward5: return MachineGearType.Forward5;
        case TICMachineGear.mgReverse2: return MachineGearType.Reverse2;
        case TICMachineGear.mgReverse3: return MachineGearType.Reverse3;
        case TICMachineGear.mgReverse4: return MachineGearType.Reverse4;
        case TICMachineGear.mgReverse5: return MachineGearType.Reverse5;
        case TICMachineGear.mgPark: return MachineGearType.Park;
        case TICMachineGear.mgUnknown: return MachineGearType.Unknown;
        case TICMachineGear.mgNull: return MachineGearType.Null;
        default: throw new Exception($"Unknown TICMachineGear type: {Convert.ToInt32(type)}");
      }
    }

    public static OnGroundStateType convertOnGroundStateType(TICOnGroundState type)
    {
      switch (type)
      {
        case TICOnGroundState.ogNo: return OnGroundStateType.No;
        case TICOnGroundState.ogYesLegacy: return OnGroundStateType.YesLegacy;
        case TICOnGroundState.ogYesMachineConfig: return OnGroundStateType.YesMachineConfig;
        case TICOnGroundState.ogYesMachineHardware: return OnGroundStateType.YesMachineHardware;
        case TICOnGroundState.ogYesMachineSoftware: return OnGroundStateType.YesMachineSoftware;
        case TICOnGroundState.ogYesRemoteSwitch: return OnGroundStateType.YesRemoteSwitch;
        case TICOnGroundState.ogUnknown: return OnGroundStateType.Unknown;
        default: throw new Exception($"Unknown TICOnGroundState type: {Convert.ToInt32(type)}");
      }
    }

    public static VibrationStateType convertVibrationStateType(TICVibrationState type)
    {
      switch (type)
      {
        case TICVibrationState.vsOff: return VibrationStateType.Off;
        case TICVibrationState.vsOn: return VibrationStateType.On;
        case TICVibrationState.vsInvalid: return VibrationStateType.Invalid;
        default: throw new Exception($"Unknown TICVibrationState type: {Convert.ToInt32(type)}");
      }
    }

    public static GPSAccuracyType convertGPSAccuracyType(TICGPSAccuracy type)
    {
      switch (type)
      {
        case TICGPSAccuracy.gpsaFine: return GPSAccuracyType.Fine;
        case TICGPSAccuracy.gpsaMedium: return GPSAccuracyType.Medium;
        case TICGPSAccuracy.gpsaCoarse: return GPSAccuracyType.Coarse;
        case TICGPSAccuracy.gpsaUnknown: return GPSAccuracyType.Unknown;
        default: throw new Exception($"Unknown TICGPSAccuracy type: {Convert.ToInt32(type)}");
      }
    }

    public static PositioningTechType convertPositioningTechType(TICPositioningTech type)
    {
      switch (type)
      {
        case TICPositioningTech.ptGPS: return PositioningTechType.GPS;
        case TICPositioningTech.ptUTS: return PositioningTechType.UTS;
        case TICPositioningTech.ptUnknown: return PositioningTechType.Unknown;
        default: throw new Exception($"Unknown TICPositioningTech type: {Convert.ToInt32(type)}");
      }
    }

    public static ColorPalette[] convertColorPalettes(TColourPalette[] colorPalettes)
    {
      return colorPalettes?.Select(cp => new ColorPalette(cp.Colour, cp.Value)).ToArray();
    }

    public static TASNodeUserPreferences convertToRaptorUserPreferences(UserPreferences userPreferences)
    {
      return ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences(
        userPreferences.TimeZone,
        userPreferences.DateSeparator,
        userPreferences.TimeSeparator,
        userPreferences.ThousandsSeparator,
        userPreferences.DecimalSeparator,
        userPreferences.TimeZoneOffset,
        userPreferences.Language,
        userPreferences.Units,
        userPreferences.DateTimeFormat,
        userPreferences.NumberFormat,
        userPreferences.TemperatureUnits,
        userPreferences.AssetLabelTypeID);
    }

    public static TMachine[] convertToRaptorMachines(Machine[] machines)
    {
      return machines?.Select(m => new TMachine() { AssetID = m.AssetID, MachineName = m.MachineName,SerialNo = m.SerialNo}).ToArray();
    }

    public static Machine[] convertMachines(TMachine[] machines)
    {
      return machines?.Select(m => new Machine() { AssetID = m.AssetID, MachineName = m.MachineName, SerialNo = m.SerialNo }).ToArray();
    }

    public static TTranslation[] convertToRaptorTranslations(TranslationDescriptor[] translations)
    {
      return translations?.Select(tr => new TTranslation(){ ID = tr.ID, Translation = tr.Translation }).ToArray();
    }
    public static T3DBoundingWorldExtent convertToRaptorProjectExtents(BoundingExtents3D extents)
    {
      return extents != null 
        ? new T3DBoundingWorldExtent(extents.MinX, extents.MinY, extents.MaxX, extents.MaxY, extents.MinZ, extents.MaxZ) 
        : new T3DBoundingWorldExtent(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
    }

    public static BoundingExtents3D convertProjectExtents(T3DBoundingWorldExtent extents)
    {
      return new BoundingExtents3D(extents.MinX, extents.MinY, extents.MinZ, extents.MaxX, extents.MaxY, extents.MaxZ);
    }

#endif
  }
}
