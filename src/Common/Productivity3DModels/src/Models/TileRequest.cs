﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation for rendering a tile of thematic information such as elevation, compaction, temperature etc
  /// The bounding box of the area to be rendered may be specified in either WGS84 lat/lon or cartesian grid coordinates in the project coordinate system.
  /// </summary>
  public class TileRequest : RaptorHelper
  {
    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; protected set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode Mode { get; protected set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// In case of cut/fill data rendering the transition order should be datum value descendent.
    /// </summary>
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    //Use default palette
    public List<ColorPalette> Palettes { get; protected set; }

    /// <summary>
    /// Color to be used to render subgrids representationaly when the production data is zoomed too far away.
    /// </summary>
    /// <value>
    /// The display color of the representational.
    /// </value>
    [JsonProperty(PropertyName = "representationalDisplayColor", Required = Required.Default)]
    public uint RepresentationalDisplayColor { get; protected set; }

    /// <summary>
    /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }

    /// <summary>
    /// The volume computation type to use for summary volume thematic rendering
    /// </summary>
    [JsonProperty(PropertyName = "computeVolType", Required = Required.Default)]
    public VolumesType ComputeVolumesType { get; protected set; }

    /// <summary>
    /// The tolerance to be used to indicate no change in volume for a cell. Used for summary volume thematic rendering. Value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_NO_CHANGE_TOLERANCE, ValidationConstants3D.MAX_NO_CHANGE_TOLERANCE)]
    [JsonProperty(PropertyName = "computeVolNoChangeTolerance", Required = Required.Default)]
    public double ComputeVolNoChangeTolerance { get; protected set; }

    /// <summary>
    /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Default)]
    public DesignDescriptor DesignDescriptor { get; protected set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; protected set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId1", Required = Required.Default)]
    public long FilterId1 { get; protected set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult Filter2 { get; protected set; }

    /// <summary>
    /// The ID of the top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId2", Required = Required.Default)]
    public long FilterId2 { get; protected set; }

    /// <summary>
    /// The method of filtering cell passes into layers to be used for thematic renderings that require layer analysis as an input into the rendered data.
    /// If this value is provided any layer method provided in a filter is ignored.
    /// </summary>
    [JsonProperty(PropertyName = "filterLayerMethod", Required = Required.Default)]
    public FilterLayerMethod FilterLayerMethod { get; protected set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
    /// Value may be null but either this or the bounding box in grid coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxLL", Required = Required.Default)]
    public BoundingBox2DLatLon BoundBoxLatLon { get; protected set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of cartesian grid coordinates in the project coordinate system, expressed in meters.
    /// Value may be null but either this or the bounding box in lat/lng coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxGrid", Required = Required.Default)]
    public BoundingBox2DGrid BoundBoxGrid { get; protected set; }

    /// <summary>
    /// The width, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(PropertyName = "width", Required = Required.Always)]
    [Required]
    public ushort Width { get; protected set; }

    /// <summary>
    /// The height, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(PropertyName = "height", Required = Required.Always)]
    [Required]
    public ushort Height { get; protected set; }

    [JsonIgnore]
    public bool ExplicitFilters {get; set;}

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public TileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="callId"></param>
    /// <param name="mode"></param>
    /// <param name="palettes"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="computeVolType"></param>
    /// <param name="computeVolNoChangeTolerance"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="filter1"></param>
    /// <param name="filterId1"></param>
    /// <param name="filter2"></param>
    /// <param name="filterId2"></param>
    /// <param name="filterLayerMethod"></param>
    /// <param name="boundingBoxLatLon"></param>
    /// <param name="boundingBoxGrid"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="representationalDisplayColor"></param>
    /// <param name="cmvDetailsColorNumber"></param>
    /// <param name="cmvPercentChangeColorNumber"></param>
    /// <param name="setSummaryDataLayersVisibility"></param>
    public TileRequest(
      long projectId,
      Guid? projectUid,
      Guid? callId,
      DisplayMode mode,
      List<ColorPalette> palettes,
      LiftBuildSettings liftBuildSettings,
      VolumesType computeVolType,
      double computeVolNoChangeTolerance,
      DesignDescriptor designDescriptor,
      FilterResult filter1,
      long filterId1,
      FilterResult filter2,
      long filterId2,
      FilterLayerMethod filterLayerMethod,
      BoundingBox2DLatLon boundingBoxLatLon,
      BoundingBox2DGrid boundingBoxGrid,
      ushort width,
      ushort height,
      uint representationalDisplayColor = 0,
      uint cmvDetailsColorNumber = 5,
      uint cmvPercentChangeColorNumber = 6,
      bool setSummaryDataLayersVisibility = true,
      bool explicitFilters = false)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      CallId = callId;
      Mode = mode;
      Palettes = palettes;
      LiftBuildSettings = liftBuildSettings;
      ComputeVolumesType = computeVolType;
      ComputeVolNoChangeTolerance = computeVolNoChangeTolerance;
      DesignDescriptor = designDescriptor;
      Filter1 = filter1;
      FilterId1 = filterId1;
      Filter2 = filter2;
      FilterId2 = filterId2;
      FilterLayerMethod = filterLayerMethod;
      BoundBoxLatLon = boundingBoxLatLon;
      BoundBoxGrid = boundingBoxGrid;
      Width = width;
      Height = height;
      RepresentationalDisplayColor = representationalDisplayColor;
      p_cmvDetailsColorNumber = cmvDetailsColorNumber;
      p_cmvPercentChangeColorNumber = cmvPercentChangeColorNumber;
      SetSummaryDataLayersVisibility = setSummaryDataLayersVisibility;
      ExplicitFilters = explicitFilters;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      ValidatePalettes(Palettes, Mode);

      //Compaction settings
      LiftBuildSettings?.Validate();

      //Volumes
      //mode == DisplayMode.VolumeCoverage
      //computeVolNoChangeTolerance and computeVolType must be provided but since not nullable types they always will have a value anyway
      ValidateDesign(DesignDescriptor, Mode, ComputeVolumesType);

      //Summary volumes: v1 has mode VolumeCoverage, v2 has mode CutFill but computeVolType is set
      if (Mode == DisplayMode.VolumeCoverage ||
          (Mode == DisplayMode.CutFill &&
           (ComputeVolumesType == VolumesType.Between2Filters ||
            ComputeVolumesType == VolumesType.BetweenDesignAndFilter ||
            ComputeVolumesType == VolumesType.BetweenFilterAndDesign)))
      {
        ValidateVolumesFilters(ComputeVolumesType, this.Filter1, this.FilterId1, this.Filter2, this.FilterId2);
      }

      if (BoundBoxLatLon == null && BoundBoxGrid == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Bounding box required either in lat/lng or grid coordinates"));
      }

      if (BoundBoxLatLon != null && BoundBoxGrid != null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Only one bounding box is allowed"));
      }

      if (Mode == DisplayMode.TargetThicknessSummary && LiftBuildSettings.LiftThicknessTarget == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "For this mode LiftThickness Target in LIftBuildSettings must be specified."));
      }

      if (Mode == DisplayMode.TargetSpeedSummary && LiftBuildSettings.MachineSpeedTarget == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "For this mode SpeedSummary Target in LiftBuildSettings must be specified."));
      }
    }
  }
}
