﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using Filter = VSS.Productivity3D.Common.Models.Filter;

namespace VSS.Productivity3D.WebApiModels.MapHandling
{
  /// <summary>
  /// Helper class for constructing a tile generation request
  /// </summary>
  public class TileGenerationRequestHelper : DataRequestBase, ITileGenerationRequestHelper
  {
    private Filter baseFilter;
    private Filter topFilter;
    private VolumeCalcType? volCalcType;

    private IEnumerable<GeofenceData> geofences;
    private IEnumerable<GeofenceData> boundaries;
    private IEnumerable<DesignDescriptor> alignmentDescriptors;
    private IEnumerable<FileData> dxfFiles;
    private ProjectDescriptor project;

    public TileGenerationRequestHelper()
    { }

    public TileGenerationRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    public TileGenerationRequestHelper SetVolumeCalcType(VolumeCalcType? calcType)
    {
      this.volCalcType = calcType;
      return this;
    }

    public TileGenerationRequestHelper SetBaseFilter(Filter baseFilter)
    {
      this.baseFilter = baseFilter;
      return this;
    }

    public TileGenerationRequestHelper SetTopFilter(Filter topFilter)
    {
      this.topFilter = topFilter;
      return this;
    }

    public TileGenerationRequestHelper SetGeofences(IEnumerable<GeofenceData> geofences)
    {
      this.geofences = geofences;
      return this;
    }

    public TileGenerationRequestHelper SetCustomBoundaries(IEnumerable<GeofenceData> boundaries)
    {
      this.boundaries = boundaries;
      return this;
    }

    public TileGenerationRequestHelper SetAlignmentDescriptors(IEnumerable<DesignDescriptor> alignmentDescriptors)
    {
      this.alignmentDescriptors = alignmentDescriptors;
      return this;
    }


    public TileGenerationRequestHelper SetDxfFiles(IEnumerable<FileData> dxfFiles)
    {
      this.dxfFiles = dxfFiles;
      return this;
    }

    public TileGenerationRequestHelper SetProject(ProjectDescriptor project)
    {
      this.project = project;
      return this;
    }


    public TileGenerationRequest CreateTileGenerationRequest(TileOverlayType[] overlays, int width, int height,
      MapType? mapType, DisplayMode? mode, string language)
    {
      return TileGenerationRequest.CreateTileGenerationRequest(DesignDescriptor, Filter, baseFilter, topFilter, volCalcType,
        geofences, boundaries, alignmentDescriptors, dxfFiles, overlays, width, height, mapType, mode, language, project, ProjectSettings, ProjectSettingsColors);

    }

  }
}
