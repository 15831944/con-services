﻿using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Types
{
  public static class GridDataFromModeConverter
  {
    public static GridDataType Convert(DisplayMode Mode)
    {
      switch (Mode)
      {
        // Note: The internal sub grid requestor will typically retrieve height and time internally to satisfy
        // a request for Height, and will return only the height component. Contexts that definitely want to receive
        // height and time should ask for that explicitly
        case DisplayMode.Height: return GridDataType.Height;

        case DisplayMode.CCV: return GridDataType.CCV;
        case DisplayMode.CCVPercent: return GridDataType.CCV;
        case DisplayMode.CCVSummary: return GridDataType.CCV;
        case DisplayMode.CCVPercentSummary: return GridDataType.CCV;
        case DisplayMode.Latency: return GridDataType.Latency;
        case DisplayMode.PassCount: return GridDataType.PassCount;
        case DisplayMode.PassCountSummary: return GridDataType.PassCount;
        case DisplayMode.Frequency: return GridDataType.Frequency;
        case DisplayMode.Amplitude: return GridDataType.Amplitude;
        case DisplayMode.RMV: return GridDataType.RMV;
        case DisplayMode.Moisture: return GridDataType.Moisture;
        case DisplayMode.TemperatureSummary: return GridDataType.Temperature;
        case DisplayMode.CutFill: return GridDataType.CutFill; 
        case DisplayMode.GPSMode: return GridDataType.GPSMode;
        case DisplayMode.CompactionCoverage: return GridDataType.CCV;
        case DisplayMode.VolumeCoverage: return GridDataType.SimpleVolumeOverlay;
        case DisplayMode.MDP: return GridDataType.MDP;
        case DisplayMode.MDPSummary: return GridDataType.MDP;
        case DisplayMode.MDPPercent: return GridDataType.MDP;
        case DisplayMode.MDPPercentSummary: return GridDataType.MDP;
        case DisplayMode.CellProfile: return GridDataType.CellProfile;
        case DisplayMode.CellPasses: return GridDataType.CellPasses;
        case DisplayMode.MachineSpeed: return GridDataType.MachineSpeed;
        case DisplayMode.CCVPercentChange: return GridDataType.CCVPercentChange;
        case DisplayMode.TargetThicknessSummary: return GridDataType.SimpleVolumeOverlay;
        case DisplayMode.TargetSpeedSummary: return GridDataType.MachineSpeedTarget;
        case DisplayMode.CMVChange: return GridDataType.CCVPercentChangeIgnoredTopNullValue;
        case DisplayMode.CCA: return GridDataType.CCA;
        case DisplayMode.CCASummary: return GridDataType.CCA;
        case DisplayMode.TemperatureDetail: return GridDataType.Temperature;
        case DisplayMode.Terrain3D: return GridDataType.Height;
        case DisplayMode.Design3D: return GridDataType.DesignHeight;

        default:
          throw new TRexException($"Unknown mode ({Mode})");
      }
    }
  }
}
