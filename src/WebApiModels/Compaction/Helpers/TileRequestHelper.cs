﻿using System;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Helper class for constructing a tile request
  /// </summary>
  public class TileRequestHelper : DataRequestBase, ITileRequestHelper
  {
    private Filter baseFilter;
    private Filter topFilter;
    private VolumeCalcType? volCalcType;
    private DesignDescriptor volumeDesign;

    public TileRequestHelper()
    { }

    public TileRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    public TileRequestHelper SetVolumeCalcType(VolumeCalcType? calcType)
    {
      this.volCalcType = calcType;
      return this;
    }

    public TileRequestHelper SetVolumeDesign(DesignDescriptor volumeDesign)
    {
      this.volumeDesign = volumeDesign;
      return this;
    }

    public TileRequestHelper SetBaseFilter(Filter baseFilter)
    {
      this.baseFilter = baseFilter;
      return this;
    }

    public TileRequestHelper SetTopFilter(Filter topFilter)
    {
      this.topFilter = topFilter;
      return this;
    }

    /// <summary>
    /// Creates an instance of the TileRequest class and populate it with data needed for a tile.   
    /// </summary>
    /// <returns>An instance of the TileRequest class.</returns>
    public TileRequest CreateTileRequest(DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, ElevationStatisticsResult elevExtents)
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);
      Filter?.Validate();//Why is this here? Should be done where filter set up???
      var palette = SettingsManager.CompactionPalette(mode, elevExtents, ProjectSettings);
      var computeVolType = (int) (volCalcType ?? VolumeCalcType.None);
      DesignDescriptor design = mode == DisplayMode.CutFill && (volCalcType == VolumeCalcType.GroundToDesign ||
                                                                volCalcType == VolumeCalcType.DesignToGround)
        ? volumeDesign
        : DesignDescriptor;
      Filter filter1 = mode == DisplayMode.CutFill && (volCalcType == VolumeCalcType.GroundToGround ||
                                                       volCalcType == VolumeCalcType.GroundToDesign)
        ? baseFilter
        : Filter;
      Filter filter2 = mode == DisplayMode.CutFill && (volCalcType == VolumeCalcType.GroundToGround ||
                                                       volCalcType == VolumeCalcType.DesignToGround)
        ? topFilter
        : null;
 
      TileRequest tileRequest = TileRequest.CreateTileRequest(
        ProjectId, null, mode, palette, liftSettings, (RaptorConverters.VolumesType) computeVolType,
        0, design, filter1, 0, filter2, 0,
        Filter == null || !Filter.layerType.HasValue ? FilterLayerMethod.None : Filter.layerType.Value,
        bbox, null, width, height, 0, CMV_DETAILS_NUMBER_OF_COLORS, false);

      return tileRequest;
    }

    private const int CMV_DETAILS_NUMBER_OF_COLORS = 16;
  }
}