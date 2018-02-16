﻿using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.Common.ResultHandling;
  
namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Default settings for compaction end points. For consistency all compaction end points should use these settings.
  /// They should be passed to Raptor for tiles and for retrieving data and also returned to the client UI (albeit in a simplfied form).
  /// </summary>
  public class CompactionSettingsManager : ICompactionSettingsManager
  {
    public CompactionSettingsManager()
    {
    }

    public LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
    }

    public CMVSettings CompactionCmvSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
    }

    public MDPSettings CompactionMdpSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<MDPSettings>(ps);
    }

    public TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings ps, bool nativeValues = true)
    {
      return AutoMapperUtility.Automapper.Map<TemperatureSettings>(ps);
    }

    public double[] CompactionCmvPercentChangeSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CmvPercentChangeSettings>(ps).percents;
    }

    public PassCountSettings CompactionPassCountSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<PassCountSettings>(ps);
    }

    public double[] CompactionCutFillSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CutFillSettings>(ps).percents;
    }

    public List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents, CompactionProjectSettings projectSettings)
    {
      const uint OVER_COLOR = 0xD50000;
      const uint ON_COLOR = 0x8BC34A;
      const uint UNDER_COLOR = 0x1579B;

      List<ColorPalette> palette = new List<ColorPalette>();
      switch (mode)
      {
        case DisplayMode.Height:

          if (elevExtents == null)
          {
            palette = null;
          }
          else
          {
            //Compaction elevation palette has 31 colors, original Raptor one had 30 colors
            List<int> colors = ElevationPalette();
            double step = (elevExtents.MaxElevation - elevExtents.MinElevation) / (colors.Count - 1);

            palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.elevationBelowColor, -1));
            for (int i = 0; i < colors.Count; i++)
            {
              palette.Add(ColorPalette.CreateColorPalette((uint)colors[i], elevExtents.MinElevation + i * step));
            }
            palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.elevationAboveColor, -1));
          }

          break;
        case DisplayMode.CCV:
          const int STEP = 100;
          // Decimal values: 87963, 2388910, 4755393, 7056597, 9423080, 11789820, 14413000, 10079077, 6594104, 2975773, 16764114, 16163752, 15628926, 15028564, 14493738, 13959168
          List<uint> cmvColors = new List<uint> { 0x01579B, 0x2473AE, 0x488FC1, 0x6BACD5, 0x8FC8E8, 0xB3E5FC, 0xDBECC8, 0x99CB65, 0x649E38, 0x2D681D, 0xFFCCD2, 0xF6A3A8, 0xEE7A7E, 0xE55154, 0xDD282A, 0xD50000 };

          for (int i = 0; i < cmvColors.Count; i++)
          {
            //The last 16th color and value are for above...
            palette.Add(ColorPalette.CreateColorPalette(cmvColors[i], i * STEP));
          }
          break;
        case DisplayMode.PassCount:
          PassCountSettings passCountSettings = CompactionPassCountSettings(projectSettings);
          List<uint> passCountDetailColors = new List<uint> { 0x2D5783, 0x439BDC, 0xBEDFF1, 0x9DCE67, 0x6BA03E, 0x3A6B25, 0xF6CED3, 0xD57A7C, 0xC13037 };
          for (int i = 0; i < passCountSettings.passCounts.Length; i++)
          {
            //The colors and values for 1-8
            palette.Add(ColorPalette.CreateColorPalette(passCountDetailColors[i], passCountSettings.passCounts[i]));
          }
          //The 9th color and value (for above)
          palette.Add(ColorPalette.CreateColorPalette(passCountDetailColors[8], passCountSettings.passCounts[7] + 1));
          break;
        case DisplayMode.PassCountSummary:
          //Values don't matter here as no machine override for compaction
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, ColorSettings.Default.passCountMinimum.value));
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, ColorSettings.Default.passCountTarget.value));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, ColorSettings.Default.passCountMaximum.value));
          break;
        case DisplayMode.CutFill:
          //Note: cut-fill also requires a design for tile requests 
          var cutFillTolerances = CompactionCutFillSettings(projectSettings);
          List<uint> cutFillColors = new List<uint> { 0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x039BE5,  0x01579B };
          for (int i = 0; i < cutFillColors.Count; i++)
          {
            palette.Add(ColorPalette.CreateColorPalette(cutFillColors[i], cutFillTolerances[i]));
          }
          break;
        case DisplayMode.TemperatureSummary:
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 1));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 2));
          break;
        case DisplayMode.CCVPercentSummary:
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryWorkInProgressLayerColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 2));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 3));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryTooThickLayerColor, 4));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryApprovedLayerColor, 5));
          break;
        case DisplayMode.MDPPercentSummary:
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryWorkInProgressLayerColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 2));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 3));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryTooThickLayerColor, 4));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryApprovedLayerColor, 5));
          break;
        case DisplayMode.TargetSpeedSummary:
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 1));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 2));
          break;
        case DisplayMode.CMVChange:
          var cmvPercentChangeSettings = CompactionCmvPercentChangeSettings(projectSettings);
          List<uint> cmvPercentChangeColors = new List<uint> { 0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x4FC3F7, 0x039BE5, 0x01579B };
          palette.Add(ColorPalette.CreateColorPalette(Colors.None, double.MinValue));
          for (int i = 0; i < cmvPercentChangeSettings.Length; i++)
          {
            palette.Add(ColorPalette.CreateColorPalette(cmvPercentChangeColors[i], cmvPercentChangeSettings[i]));
          }
          palette.Add(ColorPalette.CreateColorPalette(cmvPercentChangeColors[cmvPercentChangeColors.Count-1], NO_CCV));
          break;
      }
      return palette;
    }

    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;

    private int RGBToColor(int r, int g, int b)
    {
      return r << 16 | g << 8 | b << 0;
    }

    private List<int> ElevationPalette()
    {
      return new List<int> {
        RGBToColor(200,0,0),
        RGBToColor(255,0,0),
        RGBToColor(225,60,0),
        RGBToColor(255,90,0),
        RGBToColor(255,130,0),
        RGBToColor(255,170,0),
        RGBToColor(255,200,0),
        RGBToColor(255,220,0),
        RGBToColor(250,230,0),
        RGBToColor(220,230,0),
        RGBToColor(210,230,0),
        RGBToColor(200,230,0),
        RGBToColor(180,230,0),
        RGBToColor(150,230,0),
        RGBToColor(130,230,0),
        RGBToColor(100,240,0),
        RGBToColor(0,255,0),
        RGBToColor(0,240,100),
        RGBToColor(0,230,130),
        RGBToColor(0,230,150),
        RGBToColor(0,230,180),
        RGBToColor(0,230,200),
        RGBToColor(0,230,210),
        RGBToColor(0,220,220),
        RGBToColor(0,200,230),
        RGBToColor(0,180,240),
        RGBToColor(0,150,245),
        RGBToColor(0,120,250),
        RGBToColor(0,90,255),
        RGBToColor(0,70,255),
        RGBToColor(0,0,255)
      };
    }

    private const short MIN_CMV_MDP_VALUE = 0;
    private const short MAX_CMV_MDP_VALUE = 2000;

  }
}
