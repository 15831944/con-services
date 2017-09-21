﻿using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Interfaces
{
  public interface ICompactionSettingsManager
  {
    LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings projectSettings);
    
    CMVSettings CompactionCmvSettings(CompactionProjectSettings projectSettings);

    MDPSettings CompactionMdpSettings(CompactionProjectSettings projectSettings);
    TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings projectSettings, bool nativeValues = true);

    double[] CompactionCmvPercentChangeSettings(CompactionProjectSettings projectSettings);

    PassCountSettings CompactionPassCountSettings(CompactionProjectSettings projectSettings);

    double[] CompactionCutFillSettings(CompactionProjectSettings projectSettings);

    List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents,
      CompactionProjectSettings projectSettings);
  }
}