﻿using System.Collections.Generic;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Default settings for compaction end points. For consistency all compaction end points should use these settings.
  /// They should be passed to Raptor for tiles and for retrieving data and also returned to the client UI (albeit in a
  /// simplfied form).
  /// </summary>
  public class CompactionSettingsManager : ICompactionSettingsManager
  {
    public LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
    }

    public CMVSettings CompactionCmvSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
    }

    public CMVSettingsEx CompactionCmvSettingsEx(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CMVSettingsEx>(ps);
    }

    public MDPSettings CompactionMdpSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<MDPSettings>(ps);
    }

    public TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings ps, bool nativeValues = true)
    {
      return AutoMapperUtility.Automapper.Map<TemperatureSettings>(ps);
    }

    public double[] CompactionTemperatureDetailsSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<TemperatureDetailsSettings>(ps).CustomTemperatureDetailsTargets;
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

    public List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents,
      CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors)
    {
      var palette = new List<ColorPalette>();

      bool useDefaultValue;
      uint underColor;
      uint onColor;
      uint overColor;

      switch (mode)
      {
        case DisplayMode.Design3D:
        case DisplayMode.Height:
          {
            if (elevExtents == null)
            {
              palette = null;
            }
            else
            {
              //Compaction elevation palette has 31 colors, original Raptor one had 30 colors
              var colors = projectSettingsColors.useDefaultElevationColors.HasValue &&
                           projectSettingsColors.useDefaultElevationColors.Value
                ? CompactionProjectSettingsColors.DefaultSettings.elevationColors
                : projectSettingsColors.elevationColors;
              var step = (elevExtents.MaxElevation - elevExtents.MinElevation) / (colors.Count - 1);

              for (var i = 0; i < colors.Count; i++)
              {
                palette.Add(new ColorPalette(colors[i], elevExtents.MinElevation + i * step));
              }
            }

            break;
          }
        case DisplayMode.CCV:
          {
            var cmvDetailsSettings = CompactionCmvSettingsEx(projectSettings);
            var cmvColors = projectSettingsColors.useDefaultCMVDetailsColors.HasValue &&
                            projectSettingsColors.useDefaultCMVDetailsColors.Value
              ? CompactionProjectSettingsColors.DefaultSettings.cmvDetailsColors
              : projectSettingsColors.cmvDetailsColors;

            for (var i = 0; i < cmvDetailsSettings.CustomCMVDetailTargets.Length; i++)
            {
              //The last color and value are for above...
              palette.Add(new ColorPalette(cmvColors[i], cmvDetailsSettings.CustomCMVDetailTargets[i]));
            }
            break;
          }
        case DisplayMode.PassCount:
          {
            var passCountSettings = CompactionPassCountSettings(projectSettings);
            var passCountDetailColors = projectSettingsColors.useDefaultPassCountDetailsColors.HasValue &&
                                        projectSettingsColors.useDefaultPassCountDetailsColors.Value
              ? CompactionProjectSettingsColors.DefaultSettings.passCountDetailsColors
              : projectSettingsColors.passCountDetailsColors;

            for (var i = 0; i < passCountSettings.passCounts.Length; i++)
            {
              //The colors and values for 1-8
              palette.Add(new ColorPalette(passCountDetailColors[i], passCountSettings.passCounts[i]));
            }
            //The 9th color and value (for above)
            palette.Add(new ColorPalette(passCountDetailColors[8], passCountSettings.passCounts[7] + 1));
            break;
          }
        case DisplayMode.PassCountSummary:
          {
            //Values don't matter here as no machine override for compaction
            useDefaultValue = projectSettingsColors.useDefaultPassCountSummaryColors.HasValue &&
                              projectSettingsColors.useDefaultPassCountSummaryColors.Value;

            underColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.passCountUnderTargetColor.Value
              : projectSettingsColors.passCountUnderTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.passCountUnderTargetColor.Value;

            onColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.passCountOnTargetColor.Value
              : projectSettingsColors.passCountOnTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.passCountOnTargetColor.Value;

            overColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.passCountOverTargetColor.Value
              : projectSettingsColors.passCountOverTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.passCountOverTargetColor.Value;

            palette.Add(new ColorPalette(underColor, ColorSettings.Default.passCountMinimum.value));
            palette.Add(new ColorPalette(onColor, ColorSettings.Default.passCountTarget.value));
            palette.Add(new ColorPalette(overColor, ColorSettings.Default.passCountMaximum.value));
            break;
          }
        case DisplayMode.CutFill:
          {
            //Note: cut-fill also requires a design for tile requests 
            var cutFillTolerances = CompactionCutFillSettings(projectSettings);
            var cutFillColors = projectSettingsColors.useDefaultCutFillColors.HasValue &&
                                projectSettingsColors.useDefaultCutFillColors.Value
              ? CompactionProjectSettingsColors.DefaultSettings.cutFillColors
              : projectSettingsColors.cutFillColors;

            for (var i = 0; i < cutFillColors.Count; i++)
            {
              palette.Add(new ColorPalette(cutFillColors[i], cutFillTolerances[i]));
            }
            break;
          }
        case DisplayMode.TemperatureSummary:
          {
            useDefaultValue = projectSettingsColors.useDefaultTemperatureSummaryColors.HasValue &&
                              projectSettingsColors.useDefaultTemperatureSummaryColors.Value;

            underColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.temperatureUnderTargetColor.Value
              : projectSettingsColors.temperatureUnderTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.temperatureUnderTargetColor.Value;

            onColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.temperatureOnTargetColor.Value
              : projectSettingsColors.temperatureOnTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.temperatureOnTargetColor.Value;

            overColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.temperatureOverTargetColor.Value
              : projectSettingsColors.temperatureOverTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.temperatureOverTargetColor.Value;

            palette.Add(new ColorPalette(underColor, 0));
            palette.Add(new ColorPalette(onColor, 1));
            palette.Add(new ColorPalette(overColor, 2));
            break;
          }
        case DisplayMode.CCVPercentSummary:
          {
            useDefaultValue = projectSettingsColors.useDefaultCMVSummaryColors.HasValue &&
                              projectSettingsColors.useDefaultCMVSummaryColors.Value;

            underColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.cmvUnderTargetColor.Value
              : projectSettingsColors.cmvUnderTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.cmvUnderTargetColor.Value;

            onColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.cmvOnTargetColor.Value
              : projectSettingsColors.cmvOnTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.cmvOnTargetColor.Value;

            overColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.cmvOverTargetColor.Value
              : projectSettingsColors.cmvOverTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.cmvOverTargetColor.Value;

            palette.Add(new ColorPalette(onColor, 0));
            palette.Add(new ColorPalette(ColorSettings.Default.ccvSummaryWorkInProgressLayerColor, 1));
            palette.Add(new ColorPalette(underColor, 2));
            palette.Add(new ColorPalette(overColor, 3));
            palette.Add(new ColorPalette(ColorSettings.Default.ccvSummaryTooThickLayerColor, 4));
            palette.Add(new ColorPalette(ColorSettings.Default.ccvSummaryApprovedLayerColor, 5));
            break;
          }
        case DisplayMode.MDPPercentSummary:
          {
            useDefaultValue = projectSettingsColors.useDefaultMDPSummaryColors.HasValue &&
                              projectSettingsColors.useDefaultMDPSummaryColors.Value;

            underColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.mdpUnderTargetColor.Value
              : projectSettingsColors.mdpUnderTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.mdpUnderTargetColor.Value;

            onColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.mdpOnTargetColor.Value
              : projectSettingsColors.mdpOnTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.mdpOnTargetColor.Value;

            overColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.mdpOverTargetColor.Value
              : projectSettingsColors.mdpOverTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.mdpOverTargetColor.Value;

            palette.Add(new ColorPalette(onColor, 0));
            palette.Add(new ColorPalette(ColorSettings.Default.mdpSummaryWorkInProgressLayerColor, 1));
            palette.Add(new ColorPalette(underColor, 2));
            palette.Add(new ColorPalette(overColor, 3));
            palette.Add(new ColorPalette(ColorSettings.Default.mdpSummaryTooThickLayerColor, 4));
            palette.Add(new ColorPalette(ColorSettings.Default.mdpSummaryApprovedLayerColor, 5));
            break;
          }
        case DisplayMode.TargetSpeedSummary:
          {
            useDefaultValue = projectSettingsColors.useDefaultSpeedSummaryColors.HasValue &&
                              projectSettingsColors.useDefaultSpeedSummaryColors.Value;

            underColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.speedUnderTargetColor.Value
              : projectSettingsColors.speedUnderTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.speedUnderTargetColor.Value;

            onColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.speedOnTargetColor.Value
              : projectSettingsColors.speedOnTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.speedOnTargetColor.Value;

            overColor = useDefaultValue
              ? CompactionProjectSettingsColors.DefaultSettings.speedOverTargetColor.Value
              : projectSettingsColors.speedOverTargetColor ??
                CompactionProjectSettingsColors.DefaultSettings.speedOverTargetColor.Value;

            palette.Add(new ColorPalette(underColor, 0));
            palette.Add(new ColorPalette(onColor, 1));
            palette.Add(new ColorPalette(overColor, 2));
            break;
          }
        case DisplayMode.CMVChange:
          {
            var cmvPercentChangeSettings = CompactionCmvPercentChangeSettings(projectSettings);
            var cmvPercentChangeColors = projectSettingsColors.useDefaultCMVPercentColors.HasValue &&
                                         projectSettingsColors.useDefaultCMVPercentColors.Value
              ? CompactionProjectSettingsColors.DefaultSettings.cmvPercentColors
              : projectSettingsColors.cmvPercentColors;

            palette.Add(new ColorPalette(cmvPercentChangeColors[0], double.MinValue));

            for (var i = 0; i < cmvPercentChangeSettings.Length; i++)
              palette.Add(new ColorPalette(cmvPercentChangeColors[i + 1], cmvPercentChangeSettings[i]));

            break;
          }
        case DisplayMode.TemperatureDetail:
          {
            var temperatureDetailsSettings = CompactionTemperatureDetailsSettings(projectSettings);
            var temperatureColors = projectSettingsColors.useDefaultTemperatureDetailsColors.HasValue &&
                                    projectSettingsColors.useDefaultTemperatureDetailsColors.Value
              ? CompactionProjectSettingsColors.DefaultSettings.temperatureDetailsColors
              : projectSettingsColors.temperatureDetailsColors;

            for (var i = 0; i < temperatureDetailsSettings.Length; i++)
            {
              palette.Add(new ColorPalette(temperatureColors[i], temperatureDetailsSettings[i]));
            }
            break;
          }
      }
      return palette;
    }
  }
}
