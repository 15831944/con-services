﻿using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Common.Interfaces
{
  public interface ICompactionSettingsManager
  {
    LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings projectSettings);
    
    CMVSettings CompactionCmvSettings(CompactionProjectSettings projectSettings);

    CMVSettingsEx CompactionCmvSettingsEx(CompactionProjectSettings projectSettings);

    MDPSettings CompactionMdpSettings(CompactionProjectSettings projectSettings);

    TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings projectSettings, bool nativeValues = true);

    double[] CompactionTemperatureDetailsSettings(CompactionProjectSettings projectSettings);

    double[] CompactionCmvPercentChangeSettings(CompactionProjectSettings projectSettings);

    PassCountSettings CompactionPassCountSettings(CompactionProjectSettings projectSettings);

    double[] CompactionCutFillSettings(CompactionProjectSettings projectSettings);

    List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents,
      CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors);
  }
}
