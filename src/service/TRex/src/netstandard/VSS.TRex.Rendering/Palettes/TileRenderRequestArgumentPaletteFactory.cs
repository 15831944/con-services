﻿using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Palettes
{
  public static class TileRenderRequestArgumentPaletteFactory
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger("TileRenderRequestArgumentPaletteFactory");

    public static IPlanViewPalette GetPalette(DisplayMode mode)
    {
      switch (mode)
      {
        case DisplayMode.CCA:
          return new CCAPalette();
        case DisplayMode.CCASummary:
          return new CCASummaryPalette();
        case DisplayMode.CCV:
          return new CMVPalette();
        case DisplayMode.CCVPercent:
        case DisplayMode.CCVPercentSummary:
        case DisplayMode.CCVPercentChange:
          return new CCVPercentPalette();
        case DisplayMode.CMVChange:
          return new CMVChangePalette();
        case DisplayMode.CutFill:
          return new CutFillPalette();
        case DisplayMode.Height:
          return new HeightPalette();
        case DisplayMode.MDP:
          return new MDPPalette();
        case DisplayMode.MDPPercentSummary:
          return new MDPSummaryPalette();
        case DisplayMode.PassCount:
          return new PassCountPalette();
        case DisplayMode.PassCountSummary:
          return new PassCountSummaryPalette();
        case DisplayMode.MachineSpeed:
          return new SpeedPalette();
        case DisplayMode.TargetSpeedSummary:
          return new SpeedSummaryPalette();
        case DisplayMode.TemperatureDetail:
          return new TemperaturePalette();
        case DisplayMode.TemperatureSummary:
          return new TemperatureSummaryPalette();
        case DisplayMode.CompactionCoverage: 
          return new CompactionCoveragePalette();
        default:
          _log.LogError($"No implemented colour palette for this mode ({mode})");
          throw new TRexException($"No implemented colour palette for this mode ({mode})");
      }
    }

  }
}
