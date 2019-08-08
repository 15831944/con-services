﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Defines all the filter parameters that may be supplied as a part of a request. Filters control spatial, temporal and attribute aspects of the info
  /// Filter will override filter ID, if both are selected.
  /// </summary>
  [DataContract(Name = "Filter")]
  public class FilterResult : IEquatable<FilterResult>
  {
    /// <summary>
    /// The ID for a filter if stored in the Filters service. Not required or used in the proper functioning of a filter.
    /// Will be overridden by Filter, if Filter is also selected.
    /// </summary>
    [JsonProperty(PropertyName = "ID", Required = Required.Default)]
    public long? Id { get; set; }

    /// <summary>
    /// A filter unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "UID", Required = Required.Default)]
    public Guid? Uid { get; set; }

    /// <summary>
    /// The name for a filter if stored in the Filters service. Not required or used in the proper functioning of a filter.
    /// </summary>
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; private set; }

    /// <summary>
    /// The description for a filter if stored in the Filters service. Not required or used in the proper functioning of a filter.
    /// </summary>
    [JsonProperty(PropertyName = "description", Required = Required.Default)]
    public string Description { get; private set; }

    /// <summary>
    /// The 'start' time for a time based filter. Data recorded earlier to this time is not considered.
    /// Optional. If not present then there is no start time bound.
    /// </summary>
    [JsonProperty(PropertyName = "startUTC", Required = Required.Default)]
    public DateTime? StartUtc { get; private set; }

    /// <summary>
    /// The 'end' time for a time based filter. Data recorded after this time is not considered.
    /// Optional. If not present there is no end time bound.
    /// </summary>
    [JsonProperty(PropertyName = "endUTC", Required = Required.Default)]
    public DateTime? EndUtc { get; private set; }

    /// <summary>
    /// A machine reported design.
    /// This filter is used to select Cell passes recorded when a machine had this design loaded.
    /// May be null/empty, which indicates no restriction.
    ///   This is obtained by Trex/Raptor from tagFiles.
    ///   There is no direct relationship to VSS designs i.e. ImportedFile 
    /// </summary>
    [JsonProperty(PropertyName = "onMachineDesignID", Required = Required.Default)]
    public long? OnMachineDesignId { get; set; } // Raptor/Trex not VSS ID
    
    /// <summary>
    ///  A list of machine IDs. Cell passes recorded by machine other than those in this list are not considered.
    ///  May be null/empty, which indicates no restriction.
    /// </summary>
    [JsonProperty(PropertyName = "assetIDs", Required = Required.Default)]
    public List<long> AssetIDs { get; private set; }

    /// <summary>
    /// Only filter cell passes recorded when the vibratory drum was 'on'.  If set to null, returns all cell passes.  If true, returns only cell passes with the cell pass parameter and the drum was on.  If false, returns only cell passes with the cell pass parameter and the drum was off.
    /// </summary>
    [JsonProperty(PropertyName = "vibeStateOn", Required = Required.Default)]
    public bool? VibeStateOn { get; private set; }

    /// <summary>
    /// Only use cell passes recorded by compaction machines. If true, only return data recorded by compaction machines.  If false or null, returns all machines.
    /// </summary>
    [JsonProperty(PropertyName = "compactorDataOnly", Required = Required.Default)]
    public bool? CompactorDataOnly { get; private set; }

    /// <summary>
    /// Controls the cell pass from which to determine data based on its elevation.
    /// </summary>
    [JsonProperty(PropertyName = "elevationType", Required = Required.Default)]
    public ElevationType? ElevationType { get; private set; }

    /// <summary>
    /// A polygon to be used as a spatial filter boundary. The vertices are WGS84 positions
    /// </summary>
    [JsonProperty(PropertyName = "polygonLL", Required = Required.Default)]
    public List<WGSPoint> PolygonLL { get; private set; }

    /// <summary>
    /// A polygon to be used as a spatial filter boundary. The vertices are grid positions within the project grid coordinate system
    /// </summary>
    [JsonProperty(PropertyName = "polygonGrid", Required = Required.Default)]
    public List<Point> PolygonGrid { get; private set; }

    /// <summary>
    /// Only use cell passes recorded when the machine was driving in the forwards direction. If true, only returns machines travelling forward, if false, returns machines travelling in reverse, if null, returns all machines.
    /// </summary>
    [JsonProperty(PropertyName = "forwardDirection", Required = Required.Default)]
    public bool? ForwardDirection { get; private set; }

    /// <summary>
    /// The alignment file to be used as an alignment spatial filter
    /// </summary>
    [JsonProperty(PropertyName = "alignmentFile", Required = Required.Default)]
    public DesignDescriptor AlignmentFile { get; private set; }

    /// <summary>
    /// The starting station position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? StartStation { get; private set; }

    /// <summary>
    /// The ending station position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? EndStation { get; private set; }

    /// <summary>
    /// The left offset position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// This value may be negative, in which case it will be to the right of the alignment.
    /// </summary>
    [Range(ValidationConstants3D.MIN_OFFSET, ValidationConstants3D.MAX_OFFSET)]
    [JsonProperty(PropertyName = "leftOffset", Required = Required.Default)]
    public double? LeftOffset { get; private set; }

    /// <summary>
    /// The right offset position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// This value may be negative, in which case it will be to the left of the alignment.
    /// </summary>
    [Range(ValidationConstants3D.MIN_OFFSET, ValidationConstants3D.MAX_OFFSET)]
    [JsonProperty(PropertyName = "rightOffset", Required = Required.Default)]
    public double? RightOffset { get; private set; }

    /// <summary>
    /// A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.
    /// </summary>
    [JsonProperty(PropertyName = "machineDesignName", Required = Required.Default)]
    public string OnMachineDesignName { get; private set; } // Raptor/Trex not VSS Design

    /// <summary>
    /// layerType indicates the layer analysis method to be used for determining layers from cell passes. Some of the layer types are implemented as a 
    /// 3D spatial volume inclusion implemented via 3D spatial filtering. Only cell passes whose three dimensional location falls within the bounds
    /// 3D spatial volume are considered. If it is required to apply layer filter, lift analysis method and corresponding parameters should be specified here.
    ///  Otherwise (build lifts but do not filter, only do production data analysis) Lift Layer Analysis setting should be specified in LiftBuildSettings.
    /// </summary>
    [JsonProperty(PropertyName = "layerType", Required = Required.Default)]
    public FilterLayerMethod? LayerType { get; private set; }

    /// <summary>
    /// The design or alignment file in the project that is to be used as a spatial filter when the filter layer method is OffsetFromDesign or OffsetFromProfile.
    /// </summary>
    [JsonProperty(PropertyName = "designOrAlignmentFile", Required = Required.Default)]
    public DesignDescriptor LayerDesignOrAlignmentFile { get; private set; }

    /// <summary>
    /// The elevation of the bench to be used as the datum elevation for LayerBenchElevation filter layer type. The value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_ELEVATION, ValidationConstants3D.MAX_ELEVATION)]
    [JsonProperty(PropertyName = "benchElevation", Required = Required.Default)]
    public double? BenchElevation { get; private set; }

    /// <summary>
    /// The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file) to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.
    /// </summary>
    [Range(ValidationConstants.MIN_LAYER_NUMBER, ValidationConstants.MAX_LAYER_NUMBER)]
    [JsonProperty(PropertyName = "layerNumber", Required = Required.Default)]
    public int? LayerNumber { get; private set; }

    /// <summary>
    /// The layer thickness to be used for layers determined spatially vie the layerType member. The value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_THICKNESS, ValidationConstants3D.MAX_THICKNESS)]
    [JsonProperty(PropertyName = "layerThickness", Required = Required.Default)]
    public double? LayerThickness { get; private set; }

    /// <summary>
    /// Cell passes are only considered if the machines that recorded them are included in this list of machines. Use machine ID (historically VL Asset ID), or Machine Name from tagfile, not both.
    /// This may be null, which is no restriction on machines. 
    /// </summary>
    [JsonProperty(PropertyName = "contributingMachines", Required = Required.Default)]
    public List<MachineDetails> ContributingMachines { get; private set; }

    /// <summary>
    /// A list of surveyed surfaces that have been added to the project which are to be excluded from consideration in Raptor.
    /// </summary>
    [JsonProperty(PropertyName = "surveyedSurfaceExclusionList", Required = Required.Default)]
    public List<long> SurveyedSurfaceExclusionList { get; private set; }

    /// <summary>
    /// A list of surveyed surfaces that have been added to the project which are to be excluded from consideration in TRex.
    /// </summary>
    [JsonProperty(PropertyName = "excludedSurveyedSurfaceUids", Required = Required.Default)]
    public List<Guid> ExcludedSurveyedSurfaceUids { get; private set; }

    /// <summary>
    /// The selected cell pass to be used from the cell passes matching a filter is the earliest matching pass if true. If false, or not present the latest cell pass is used.
    /// This value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "returnEarliest", Required = Required.Default)]
    public bool? ReturnEarliest { get; set; }

    /// <summary>
    /// Sets the GPS accuracy filtering aspect.
    /// </summary>
    /// <value>
    /// The GPS accuracy.
    /// </value>
    [JsonProperty(PropertyName = "gpsAccuracy", Required = Required.Default)]
    public GPSAccuracy? GpsAccuracy { get; private set; }

    /// <summary>
    /// Determines if the GPS accuracy filter is inclusive or not. If the value is true then each GPS accuracy level
    /// includes the level(s) below it. If false (the default) then the GPS accuracy level is for that value only.
    /// </summary>
    [JsonProperty(PropertyName = "gpsAccuracyIsInclusive", Required = Required.Default)]
    public bool? GpsAccuracyIsInclusive { get; private set; }

    /// <summary>
    /// Use cell passes generated from the primary machine implement (blade/drum(s)). 
    /// If true, data recorded from primary implement tracking is used in satisfying the request. 
    /// If false, returns no data where cell passes are recorded from primary implement positions. 
    /// If null, the same behaviour as True is applied.
    /// </summary>
    [JsonProperty(PropertyName = "implementMapping", Required = Required.Default)]
    public bool? BladeOnGround { get; private set; }

    /// <summary>
    /// Use cell passes generated from the machine track positions. 
    /// If true, data recorded from the machine track positions is used in satisfying the request. 
    /// If false, returns no data where cell passes are recorded from track positions. 
    /// If null, the same behaviour as False is applied.
    /// </summary>
    [JsonProperty(PropertyName = "trackMapping", Required = Required.Default)]
    public bool? TrackMapping { get; private set; }

    /// <summary>
    /// Use cell passes generated from the machine wheel positions. 
    /// If true, data recorded from the machine wheel positions is used in satisfying the request. 
    /// If false, returns no data where cell passes are recorded from wheel positions. 
    /// If null, the same behaviour as False is applied.
    /// </summary>
    [JsonProperty(PropertyName = "wheelMapping", Required = Required.Default)]
    public bool? WheelTracking { get; private set; }

    /// <summary>
    /// The design file in the project that is to be used as a spatial filter.
    /// </summary>
    [JsonProperty(PropertyName = "designFile", Required = Required.Default)]
    public DesignDescriptor DesignFile { get; private set; }

    /// <summary>
    /// Filter cell passes recorded when the guidance mode is automatics, manual or unknown.
    /// If set to null, returns all cell passes.  If set to an automatics type, returns only cell passes with the 
    /// guidance mode set to the specified automatics type.  
    /// </summary>
    [JsonProperty(PropertyName = "automaticsType", Required = Required.Default)]
    public AutomaticsType? AutomaticsType { get; protected set; }

    /// <summary>
    /// The minimum temperature in °C for a temperature range filter. Only cell passes within the range will be selected.
    /// </summary>
    [Range(ValidationConstants.MIN_TEMPERATURE, ValidationConstants.MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "temperatureRangeMin", Required = Required.Default)]
    public double? TemperatureRangeMin { get; private set; }

    /// <summary>
    /// The maximum temperature in °C for a temperature range filter. Only cell passes within the range will be selected.
    /// </summary>
    [Range(ValidationConstants.MIN_TEMPERATURE, ValidationConstants.MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "temperatureRangeMax", Required = Required.Default)]
    public double? TemperatureRangeMax { get; private set; }

    /// <summary>
    /// The minimum pass count for a  pass count range filter. Only cell passes within the range will be selected.
    /// </summary>
    [Range(ValidationConstants.MIN_PASS_COUNT, ValidationConstants.MAX_PASS_COUNT)]
    [JsonProperty(PropertyName = "passCountRangeMin", Required = Required.Default)]
    public int? PassCountRangeMin { get; private set; }

    /// <summary>
    /// The maximum pass count for a  pass count range filter. Only cell passes within the range will be selected.
    /// </summary>
    [Range(ValidationConstants.MIN_PASS_COUNT, ValidationConstants.MAX_PASS_COUNT)]
    [JsonProperty(PropertyName = "passCountRangeMax", Required = Required.Default)]
    public int? PassCountRangeMax { get; private set; }

    public bool isFilterContainsSSOnly { get; private set; }

    public bool IsFilterEmpty => isFilterEmpty();

    [JsonIgnore]
    public DateRangeType? DateRangeType { get; }

    [JsonIgnore]
    public bool? AsAtDate { get; protected set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public FilterResult()
    { }

    private bool isFilterEmpty()
    {
      if (
        !StartUtc.HasValue &&
        !EndUtc.HasValue &&
        !OnMachineDesignId.HasValue &&
        string.IsNullOrEmpty(OnMachineDesignName) &&
        (AssetIDs == null || AssetIDs.Count == 0) &&
        !VibeStateOn.HasValue &&
        !CompactorDataOnly.HasValue &&
        !ElevationType.HasValue &&
        (PolygonLL == null || PolygonLL.Count == 0) &&
        (PolygonGrid == null || PolygonGrid.Count == 0) &&
        !ForwardDirection.HasValue &&
        AlignmentFile == null &&
        !StartStation.HasValue &&
        !EndStation.HasValue &&
        !LeftOffset.HasValue &&
        !RightOffset.HasValue &&
        !LayerType.HasValue &&
        LayerDesignOrAlignmentFile == null &&
        !BenchElevation.HasValue &&
        !LayerNumber.HasValue &&
        !LayerThickness.HasValue &&
        (ContributingMachines == null || ContributingMachines.Count == 0) &&
        (SurveyedSurfaceExclusionList == null || SurveyedSurfaceExclusionList.Count == 0) &&
        (ExcludedSurveyedSurfaceUids == null || ExcludedSurveyedSurfaceUids.Count == 0) &&
        !ReturnEarliest.HasValue &&
        !GpsAccuracy.HasValue &&
        !GpsAccuracyIsInclusive.HasValue &&
        !BladeOnGround.HasValue &&
        !TrackMapping.HasValue &&
        !WheelTracking.HasValue &&
        DesignFile == null &&
        !AutomaticsType.HasValue &&
        !TemperatureRangeMin.HasValue &&
        !TemperatureRangeMax.HasValue &&
        !PassCountRangeMin.HasValue &&
        !PassCountRangeMax.HasValue
      ) return true;
      return false;
    }

    // TODO (Aaron) Refactor the constructors for this object. The following is only used for unit testing.
    /// <summary>
    /// Static constructor.
    /// </summary>
    public static FilterResult CreateFilterObsolete(
      long? id,
      Guid? uid,
      string name,
      string description,
      DateTime? startUtc,
      DateTime? endUtc,
      long? onMachineDesignId,
      string onMachineDesignName,
      List<long> assetIDs,
      bool? vibeStateOn,
      bool? compactorDataOnly,
      ElevationType? elevationType,
      List<WGSPoint> polygonLL,
      List<Point> polygonGrid,
      bool? forwardDirection,
      DesignDescriptor alignmentFile,
      double? startStation,
      double? endStation,
      double? leftOffset,
      double? rightOffset,
      FilterLayerMethod? layerType,
      DesignDescriptor layerDesignOrAlignmentFile,
      double? benchElevation,
      int? layerNumber,
      double? layerThickness,
      List<MachineDetails> contributingMachines,
      List<long> surveyedSurfaceExclusionList,
      List<Guid> excludedSurveyedSurfaceUids,
      bool? returnEarliest,
      GPSAccuracy? accuracy,
      bool? inclusive,
      bool? bladeOnGround,
      bool? trackMapping,
      bool? wheelTracking,
      DesignDescriptor designFile,
      AutomaticsType? automaticsType,
      double? temperatureRangeMin,
      double? temperatureRangeMax,
      int? passCountRangeMin,
      int? passCountRangeMax
    )
    {
      return new FilterResult
      {
        Id = id,
        Uid = uid,
        Name = name,
        Description = description,
        StartUtc = startUtc,
        EndUtc = endUtc,
        OnMachineDesignId = onMachineDesignId,
        AssetIDs = assetIDs,
        VibeStateOn = vibeStateOn,
        CompactorDataOnly = compactorDataOnly,
        ElevationType = elevationType,
        PolygonLL = polygonLL,
        PolygonGrid = polygonGrid,
        ForwardDirection = forwardDirection,
        AlignmentFile = alignmentFile,
        StartStation = startStation,
        EndStation = endStation,
        LeftOffset = leftOffset,
        RightOffset = rightOffset,
        OnMachineDesignName = onMachineDesignName,
        LayerType = layerType,
        LayerDesignOrAlignmentFile = layerDesignOrAlignmentFile,
        BenchElevation = benchElevation,
        LayerNumber = layerNumber,
        LayerThickness = layerThickness,
        ContributingMachines = contributingMachines,
        SurveyedSurfaceExclusionList = surveyedSurfaceExclusionList,
        ExcludedSurveyedSurfaceUids = excludedSurveyedSurfaceUids,
        ReturnEarliest = returnEarliest,
        GpsAccuracy = accuracy,
        GpsAccuracyIsInclusive = inclusive,
        BladeOnGround = bladeOnGround,
        TrackMapping = trackMapping,
        WheelTracking = wheelTracking,
        DesignFile = designFile,
        AutomaticsType = automaticsType,
        TemperatureRangeMin = temperatureRangeMin,
        TemperatureRangeMax = temperatureRangeMax,
        PassCountRangeMin = passCountRangeMin,
        PassCountRangeMax = passCountRangeMax
      };
    }

    /// <summary>
    /// Static constructor for use with the CCATileController class.
    /// </summary>
    public static FilterResult CreateFilterForCCATileRequest(
      DateTime? startUtc,
      DateTime? endUtc,
      List<long> assetIDs,
      List<WGSPoint> polygonLL,
      FilterLayerMethod? layerType,
      int? layerNumber,
      List<MachineDetails> contributingMachines)
    {
      return new FilterResult
      {
        StartUtc = startUtc,
        EndUtc = endUtc,
        AssetIDs = assetIDs,
        PolygonLL = polygonLL,
        LayerType = layerType,
        LayerNumber = layerNumber,
        ContributingMachines = contributingMachines,
      };
    }

    /// <summary>
    /// Creates a new <see cref="FilterResult"/> specifically for excluding surveyed surfaces only.
    /// </summary>
    public static FilterResult CreateFilter(List<(long, Guid)> surveyedSurfaceExclusionList)
    {
      return new FilterResult
      {
        isFilterContainsSSOnly = true,
        SurveyedSurfaceExclusionList = surveyedSurfaceExclusionList.Select(s => s.Item1).ToList(),
        ExcludedSurveyedSurfaceUids = surveyedSurfaceExclusionList.Select(s => s.Item2).ToList()
      };
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public FilterResult
    (
      Guid? uid,
      Productivity3D.Filter.Abstractions.Models.Filter filter,
      List<WGSPoint> polygonLL,
      DesignDescriptor alignmentFile,
      FilterLayerMethod? layerType,
      List<long> surveyedSurfaceExclusionList,
      List<Guid> excludedSurveyedSurfaceUids,
      bool? returnEarliest,
      DesignDescriptor designFile)
    {
      Uid = uid;
      StartUtc = filter.StartUtc;
      EndUtc = filter.EndUtc;
      OnMachineDesignId = filter.OnMachineDesignId;
      OnMachineDesignName = filter.OnMachineDesignName;
      VibeStateOn = filter.VibeStateOn;
      ElevationType = filter.ElevationType;
      PolygonLL = polygonLL;
      ForwardDirection = filter.ForwardDirection;
      AlignmentFile = alignmentFile;
      StartStation = filter.StartStation;
      EndStation = filter.EndStation;
      LeftOffset = filter.LeftOffset;
      RightOffset = filter.RightOffset;
      LayerType = layerType;
      LayerNumber = filter.LayerNumber;
      ContributingMachines = filter.ContributingMachines;
      SurveyedSurfaceExclusionList = surveyedSurfaceExclusionList;
      ExcludedSurveyedSurfaceUids = excludedSurveyedSurfaceUids;
      ReturnEarliest = returnEarliest;
      DesignFile = designFile;
      DateRangeType = filter.DateRangeType;
      AsAtDate = filter.AsAtDate;
      AutomaticsType = filter.AutomaticsType;
      TemperatureRangeMin = filter.TemperatureRangeMin;
      TemperatureRangeMax = filter.TemperatureRangeMax;
      PassCountRangeMin = filter.PassCountRangeMin;
      PassCountRangeMax = filter.PassCountRangeMax;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (PolygonGrid != null)
      {
        foreach (var pt in PolygonGrid)
          pt.Validate();
      }
      AlignmentFile?.Validate();
      LayerDesignOrAlignmentFile?.Validate();
      DesignFile?.Validate();

      if (ContributingMachines != null)
      {
        foreach (var machine in ContributingMachines)
          machine.Validate();
      }

      //Check date range parts
      if (StartUtc.HasValue || EndUtc.HasValue)
      {
        if (StartUtc.HasValue && EndUtc.HasValue)
        {
          if (StartUtc.Value > EndUtc.Value)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "StartUTC must be earlier than EndUTC"));
          }
        }
        else
        {
          if (!AsAtDate.HasValue || AsAtDate.Value == false)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "If using a date range both dates must be provided"));
          }

          if (!EndUtc.HasValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Either EndUTC or DateRangeType must be provided for an as-at date filter"));
          }
        }
      }
      if (AsAtDate.HasValue)
      {
        bool valid = EndUtc.HasValue || DateRangeType.HasValue &&
                     DateRangeType.Value != MasterData.Models.Internal.DateRangeType.Custom;//custom must have end UTC
        if (!valid)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Either EndUTC or DateRangeType must be provided for an as-at date filter"));
        }
      }

      //Check alignment filter parts
      if (AlignmentFile != null || StartStation.HasValue || EndStation.HasValue ||
          LeftOffset.HasValue || RightOffset.HasValue)
      {
        if (AlignmentFile == null || !StartStation.HasValue || !EndStation.HasValue ||
            !LeftOffset.HasValue || !RightOffset.HasValue)

          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "If using an alignment filter, alignment file, start and end station, left and right offset  must be provided"));

        AlignmentFile.Validate();
      }

      //Check layer filter parts
      if (LayerNumber.HasValue && !LayerType.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "To use the layer number filter, layer type must be specified"));
      }

      if (LayerType.HasValue)
      {
        switch (LayerType.Value)
        {
          case FilterLayerMethod.OffsetFromDesign:
          case FilterLayerMethod.OffsetFromBench:
          case FilterLayerMethod.OffsetFromProfile:
            if (LayerType.Value == FilterLayerMethod.OffsetFromBench)
            {
              if (!BenchElevation.HasValue)
              {
                throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    "If using an offset from bench filter, bench elevation must be provided"));
              }
            }
            else
            {
              if (LayerDesignOrAlignmentFile == null)
              {
                throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    "If using an offset from design or profile filter, design or alignment file must be provided"));
              }
              LayerDesignOrAlignmentFile.Validate();
            }
            if (!LayerNumber.HasValue || !LayerThickness.HasValue)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "If using an offset from bench, design or alignment filter, layer number and layer thickness must be provided"));
            }
            break;
          case FilterLayerMethod.TagfileLayerNumber:
            if (!LayerNumber.HasValue)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "If using a tag file layer filter, layer number must be provided"));
            }
            break;
        }
      }

      //Check boundary if provided
      //Raptor handles any weird boundary you give it and automatically closes it if not closed already therefore we just need to check we have at least 3 points
      if (PolygonLL != null && PolygonLL.Count < 3)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Too few points for filter polygon"));
      }

      if (PolygonGrid != null && PolygonGrid.Count < 3)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Too few points for filter polygon"));
      }

      if (PolygonLL != null && PolygonLL.Count > 0 && PolygonGrid != null && PolygonGrid.Count > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Only one type of filter boundary can be defined at one time"));
      }

      if (TemperatureRangeMin.HasValue != TemperatureRangeMax.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid temperature range filter. Both minimum and maximum must be provided."));
      }

      if (TemperatureRangeMin.HasValue && TemperatureRangeMax.HasValue)
      {
        if (TemperatureRangeMin.Value > TemperatureRangeMax.Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid temperature range filter. Minimum must be less than maximum."));
        }
        if (TemperatureRangeMin.Value < ValidationConstants.MIN_TEMPERATURE || TemperatureRangeMin.Value > ValidationConstants.MAX_TEMPERATURE ||
            TemperatureRangeMax.Value < ValidationConstants.MIN_TEMPERATURE || TemperatureRangeMax.Value > ValidationConstants.MAX_TEMPERATURE)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Invalid temperature range filter. Range must be between {ValidationConstants.MIN_TEMPERATURE} and {ValidationConstants.MAX_TEMPERATURE}."));
        }
      }

      if (PassCountRangeMin.HasValue != PassCountRangeMax.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid pass count range filter. Both minimum and maximum must be provided."));
      }

      if (PassCountRangeMin.HasValue && PassCountRangeMax.HasValue)
      {
        if (PassCountRangeMin.Value > PassCountRangeMax.Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid pass count range filter. Minimum must be less than maximum."));
        }
        if (PassCountRangeMin.Value < ValidationConstants.MIN_TEMPERATURE || PassCountRangeMin.Value > ValidationConstants.MAX_TEMPERATURE ||
            PassCountRangeMax.Value < ValidationConstants.MIN_TEMPERATURE || PassCountRangeMax.Value > ValidationConstants.MAX_TEMPERATURE)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Invalid pass count range filter. Range must be between {ValidationConstants.MIN_PASS_COUNT} and {ValidationConstants.MAX_PASS_COUNT}."));
        }
      }
    }

    #region IEquatable
    public bool Equals(FilterResult other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Id.Equals(other.Id) &&
             Uid.Equals(other.Uid) &&
             string.Equals(Name, other.Name) &&
             string.Equals(Description, other.Description) &&
             StartUtc.Equals(other.StartUtc) &&
             EndUtc.Equals(other.EndUtc) &&
             OnMachineDesignId == other.OnMachineDesignId &&
             OnMachineDesignName == other.OnMachineDesignName &&
             AssetIDs.ScrambledEquals(other.AssetIDs) &&
             VibeStateOn == other.VibeStateOn &&
             CompactorDataOnly.Equals(other.CompactorDataOnly) &&
             ElevationType == other.ElevationType &&
             PolygonLL.ScrambledEquals(other.PolygonLL) &&
             PolygonGrid.ScrambledEquals(other.PolygonGrid) &&
             ForwardDirection == other.ForwardDirection &&
             (AlignmentFile == null ? other.AlignmentFile == null : AlignmentFile.Equals(other.AlignmentFile)) &&
             StartStation.Equals(other.StartStation) &&
             EndStation.Equals(other.EndStation) &&
             LeftOffset.Equals(other.LeftOffset) &&
             RightOffset.Equals(other.RightOffset) &&
             LayerType.Equals(other.LayerType) &&
             (LayerDesignOrAlignmentFile == null
               ? other.LayerDesignOrAlignmentFile == null
               : LayerDesignOrAlignmentFile.Equals(other.LayerDesignOrAlignmentFile)) &&
             BenchElevation.Equals(other.BenchElevation) &&
             LayerNumber == other.LayerNumber &&
             LayerThickness.Equals(other.LayerThickness) &&
             ContributingMachines.ScrambledEquals(other.ContributingMachines) &&
             SurveyedSurfaceExclusionList.ScrambledEquals(other.SurveyedSurfaceExclusionList) &&
             ExcludedSurveyedSurfaceUids.ScrambledEquals(other.ExcludedSurveyedSurfaceUids) &&
             ReturnEarliest.Equals(other.ReturnEarliest) &&
             GpsAccuracy.Equals(other.GpsAccuracy) &&
             GpsAccuracyIsInclusive.Equals(other.GpsAccuracyIsInclusive) &&
             BladeOnGround.Equals(other.BladeOnGround) &&
             TrackMapping.Equals(other.TrackMapping) &&
             WheelTracking.Equals(other.WheelTracking) &&
             (DesignFile == null ? other.DesignFile == null : DesignFile.Equals(other.DesignFile)) &&
             AutomaticsType == other.AutomaticsType &&
             TemperatureRangeMin.Equals(other.TemperatureRangeMin) &&
             TemperatureRangeMax.Equals(other.TemperatureRangeMax) &&
             PassCountRangeMin.Equals(other.PassCountRangeMin) &&
             PassCountRangeMax.Equals(other.PassCountRangeMax) &&
             isFilterContainsSSOnly == other.isFilterContainsSSOnly;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;

      return Equals((FilterResult)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = GetNullableHashCode(Id);
        hashCode = GetNullableHashCode(Uid);
        hashCode = GetHashCode(hashCode, GetNullableHashCode(Name));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(Description));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(StartUtc));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(EndUtc));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(OnMachineDesignId));
        hashCode = GetHashCode(hashCode, GetListHashCode(AssetIDs));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(VibeStateOn));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(CompactorDataOnly));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(ElevationType));
        hashCode = GetHashCode(hashCode, GetListHashCode(PolygonLL));
        hashCode = GetHashCode(hashCode, GetListHashCode(PolygonGrid));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(ForwardDirection));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(AlignmentFile));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(StartStation));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(EndStation));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(LeftOffset));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(RightOffset));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(OnMachineDesignName));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(LayerType));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(LayerDesignOrAlignmentFile));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(BenchElevation));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(LayerNumber));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(LayerThickness));
        hashCode = GetHashCode(hashCode, GetListHashCode(ContributingMachines));
        hashCode = GetHashCode(hashCode, GetListHashCode(SurveyedSurfaceExclusionList));
        hashCode = GetHashCode(hashCode, GetListHashCode(ExcludedSurveyedSurfaceUids));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(ReturnEarliest));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(GpsAccuracy));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(GpsAccuracyIsInclusive));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(BladeOnGround));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(TrackMapping));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(WheelTracking));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(DesignFile));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(AutomaticsType));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(TemperatureRangeMin));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(TemperatureRangeMax));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(PassCountRangeMin));
        hashCode = GetHashCode(hashCode, GetNullableHashCode(PassCountRangeMax));
        return hashCode;
      }
    }

    private int GetHashCode(int totalHashCode, int singleHashCode)
    {
      return (totalHashCode * 397) ^ singleHashCode;
    }

    private int GetListHashCode<T>(List<T> list) => list?.GetListHashCode() ?? 0;

    private int GetNullableHashCode<T>(T nullable)
    {
      return nullable == null ? 0 : nullable.GetHashCode();
    }

    public static bool operator ==(FilterResult left, FilterResult right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(FilterResult left, FilterResult right)
    {
      return !Equals(left, right);
    }
    #endregion

    public void ApplyDateRange(string ianaTimeZoneName)
    {
      if (!string.IsNullOrEmpty(ianaTimeZoneName) &&
          DateRangeType != null &&
          DateRangeType != MasterData.Models.Internal.DateRangeType.Custom)
      {
        // Force date range filters to be null if ProjectExtents is specified.
        if (DateRangeType == MasterData.Models.Internal.DateRangeType.ProjectExtents)
        {
          StartUtc = null;
          EndUtc = null;
        }
        else
        {
          StartUtc = DateRangeType?.UtcForDateRangeType(ianaTimeZoneName, true, true);
          EndUtc = DateRangeType?.UtcForDateRangeType(ianaTimeZoneName, false, true);
        }
      }
      //For as-at dates only use EndUTC, so make sure StartUTC is null
      if (AsAtDate == true)
      {
        StartUtc = null;
      }
    }
  }
}
