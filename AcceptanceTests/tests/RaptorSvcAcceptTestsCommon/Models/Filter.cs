﻿using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using RaptorSvcAcceptTestsCommon.Utils;

namespace RaptorSvcAcceptTestsCommon.Models
{
  /// <summary>
  /// Defines all the filter parameters that may be supplied as a part of a request. Filters control spatial, temporal and attribute aspects of the info
  /// This is copied from ...\RaptorServicesCommon\Models\Filter.cs 
  /// </summary>
  public class Filter : IEquatable<Filter>
  {
      #region Members
      /// <summary>
      /// The ID for a filter if stored in the Filters service. Not required or used in the proper functioning of a filter.
      /// </summary>
      public long? ID { get; set; }

      /// <summary>
      /// The name for a filter if stored in the Filters service. Not required or used in the proper functioning of a filter.
      /// </summary>
      public string name { get; set; }

      /// <summary>
      /// The description for a filter if stored in the Filters service. Not required or used in the proper functioning of a filter.
      /// </summary>
      public string description { get; set; }

      /// <summary>
      /// The 'start' time for a time based filter. Data recorded earlier to this time is not considered.
      /// Optional. If not present then there is no start time bound.
      /// </summary>
      public DateTime? startUTC { get; set; }

      /// <summary>
      /// The 'end' time for a time based filter. Data recorded after this time is not considered.
      /// Optional. If not present there is no end time bound.
      /// </summary>
      public DateTime? endUTC { get; set; }

      /// <summary>
      /// A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
      /// May be null/empty, which indicates no restriction. 
      /// </summary>
      public long? onMachineDesignID { get; set; } //PDS not VL ID

      /// <summary>
      ///  A list of machine IDs. Cell passes recorded by machine other than those in this list are not considered.
      ///  May be null/empty, which indicates no restriction.
      /// </summary>
      public List<long> assetIDs { get; set; }

      /// <summary>
      /// Only filter cell passes recorded when the vibratory drum was 'on'
      /// </summary>
      public bool? vibeStateOn { get; set; }

      /// <summary>
      /// Only use cell passes recorded by compaction machines
      /// </summary>
      public bool? compactorDataOnly { get; set; }

      /// <summary>
      /// Controls the cell pass from which to determine data based on its elevation.
      /// </summary>
      public ElevationType? elevationType { get; set; }

      /// <summary>
      /// A polygon to be used as a spatial filter boundary. The vertices are WGS84 positions
      /// </summary>
      public List<WGSPoint> polygonLL { get; set; }

      /// <summary>
      /// A polygon to be used as a spatial filter boundary. The vertices are grid positions within the project grid coordinate system
      /// </summary>
      public List<Point> polygonGrid { get; set; }

      /// <summary>
      /// Only use cell passes recorded when the machine was driving in the forwards direction
      /// </summary>
      public bool? forwardDirection { get; set; }

      /// <summary>
      /// The alignment file to be used as an alignment spatial filter
      /// </summary>
      public DesignDescriptor alignmentFile { get; set; }

      /// <summary>
      /// The starting station position on a alignment being used as a spatial filter. The value is expressed in meters.
      /// </summary>
      public double? startStation { get; set; }

      /// <summary>
      /// The ending station position on a alignment being used as a spatial filter. The value is expressed in meters.
      /// </summary>
      public double? endStation { get; set; }

      /// <summary>
      /// The left offset position on a alignment being used as a spatial filter. The value is expressed in meters.
      /// This value may be negative, in which case it will be to the right of the alignment.
      /// </summary>
      public double? leftOffset { get; set; }

      /// <summary>
      /// The right offset position on a alignment being used as a spatial filter. The value is expressed in meters.
      /// This value may be negative, in which case it will be to the left of the alignment.
      /// </summary>
      public double? rightOffset { get; set; }

      /// <summary>
      /// Only consider cell passes recorded when the machine had the named design loaded.
      /// </summary>
      public string machineDesignName { get; set; }

      /// <summary>
      /// layerType indicates the layer analysis method to be used for determining layers from cell passes. Some of the layer types are implemented as a 
      /// 3D spatial volume inclusion implemented via 3D spatial filtering. Only cell passes whose three dimensional location falls within the bounds
      /// 3D spatial volume are considered.
      /// </summary>
      public FilterLayerMethod? layerType { get; set; }

      /// <summary>
      /// The design or alignment file in the project that is to be used as a spatial filter when the filter layer method is OffsetFromDesign or OffsetFromProfile.
      /// </summary>
      public DesignDescriptor designOrAlignmentFile { get; set; }

      /// <summary>
      /// The elevation of the bench to be used as the datum elevation for LayerBenchElevation filter layer type. The value is expressed in meters.
      /// </summary>
      public double? benchElevation { get; set; }

      /// <summary>
      /// The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file) to be used as the layer type filter. Layer 3 is then the third layer from the
      /// datum elevation where each layer has a thickness defined by the layerThickness member.
      /// </summary>
      public int? layerNumber { get; set; }

      /// <summary>
      /// The layer thickness to be used for layers determined spatially vie the layerType member. The value is expressed in meters.
      /// </summary>
      public double? layerThickness { get; set; }

      /// <summary>
      /// Cell passes are only considered if the machines that recorded them are included in this list of machines.
      /// This may be null, which is no restriction on machines.
      /// </summary>
      public List<MachineDetails> contributingMachines { get; set; }

      /// <summary>
      /// A list of surveyed surfaces that have been added to the project which are to be excluded from consideration.
      /// </summary>
      public List<long> surveyedSurfaceExclusionList { get; set; }

      /// <summary>
      /// The selected cell pass to be used from the cell passes matching a filter is the earliest matching pass if true. If false, or not present the latest cell pass is used.
      /// This value may be null.
      /// </summary>
      public bool? returnEarliest { get; set; }

      /// <summary>
      /// Sets the GPS accuracy filtering aspect.
      /// </summary>
      /// <value>
      /// The GPS accuracy.
      /// </value>
      public GPSAccuracy? gpsAccuracy { get; set; }

      /// <summary>
      /// Determines if the GPS accuracy filter is inclusive or not. If the value is true then each GPS accuracy level
      /// includes the level(s) below it. If false (the default) then the GPS accuracy level is for that value only.
      /// </summary>
      public bool? gpsAccuracyIsInclusive { get; set; }

      /// <summary>
      /// Only use cell passes recorded with the machine blade position on the ground. If true, only returns machines travelling forward, if false, returns machines travelling in reverse, if null, returns all machines.
      /// </summary>
      public bool? implementMapping { get; set; }

      /// <summary>
      /// Only use cell passes recorded with the machine track position. If true, only returns machines travelling forward, if false, returns machines travelling in reverse, if null, returns all machines.
      /// </summary>
      public bool? trackMapping { get; set; }

      /// <summary>
      /// Only use cell passes recorded with the machine wheel position. If true, only returns machines travelling forward, if false, returns machines travelling in reverse, if null, returns all machines.
      /// </summary>
      public bool? wheelMapping { get; set; }
      #endregion

      #region Constructors
      //public Filter()
      //{ }

      public Filter(
        long? ID = null,
        string name = null,
        string description = null,
        DateTime? startUTC = null,
        DateTime? endUTC = null,
        long? onMachineDesignID = null,
        List<long> assetIDs = null,
        bool? vibeStateOn = null,
        bool? compactorDataOnly = null,
        ElevationType? elevationType = null,
        List<WGSPoint> polygonLL = null,
        List<Point> polygonGrid = null,
        bool? forwardDirection = null,
        DesignDescriptor alignmentFile = null,
        double? startStation = null,
        double? endStation = null,
        double? leftOffset = null,
        double? rightOffset = null,
        string machineDesignName = null,
        FilterLayerMethod? layerType = null,
        DesignDescriptor designOrAlignmentFile = null,
        double? benchElevation = null,
        int? layerNumber = null,
        double? layerThickness = null,
        List<MachineDetails> contributingMachines = null,
        List<long> surveyedSurfaceExclusionList = null,
        bool? returnEarliest = null,
        GPSAccuracy? accuracy = null,
        bool? inclusive = null,
        bool? implementMapping = null,
        bool? trackMapping = null,
        bool? wheelMapping = null
        )
      {
          this.ID = ID;
          this.name = name;
          this.description = description;
          this.startUTC = startUTC;
          this.endUTC = endUTC;
          this.onMachineDesignID = onMachineDesignID;
          this.assetIDs = assetIDs;
          this.vibeStateOn = vibeStateOn;
          this.compactorDataOnly = compactorDataOnly;
          this.elevationType = elevationType;
          this.polygonLL = polygonLL;
          this.polygonGrid = polygonGrid;
          this.forwardDirection = forwardDirection;
          this.alignmentFile = alignmentFile;
          this.startStation = startStation;
          this.endStation = endStation;
          this.leftOffset = leftOffset;
          this.rightOffset = rightOffset;
          this.machineDesignName = machineDesignName;
          this.layerType = layerType;
          this.designOrAlignmentFile = designOrAlignmentFile;
          this.benchElevation = benchElevation;
          this.layerNumber = layerNumber;
          this.layerThickness = layerThickness;
          this.contributingMachines = contributingMachines;
          this.surveyedSurfaceExclusionList = surveyedSurfaceExclusionList;
          this.returnEarliest = returnEarliest;
          this.gpsAccuracy = accuracy;
          this.gpsAccuracyIsInclusive = inclusive;
          this.implementMapping = implementMapping;
          this.trackMapping = trackMapping;
          this.wheelMapping = wheelMapping;

      }
      #endregion

      #region Equality test
      public bool Equals(Filter other)
      {
          if (other == null)
              return false;

          return this.ID == other.ID &&
              this.name == other.name &&
              this.description == other.description &&
              this.startUTC == other.startUTC &&
              this.endUTC == other.endUTC &&
              this.onMachineDesignID == other.onMachineDesignID &&
              Common.ListsAreEqual(this.assetIDs, other.assetIDs) &&
              this.vibeStateOn == other.vibeStateOn &&
              this.compactorDataOnly == other.compactorDataOnly &&
              this.elevationType == other.elevationType &&
              Common.ListsAreEqual(this.polygonLL, other.polygonLL) &&
              Common.ListsAreEqual(this.polygonGrid, other.polygonGrid) &&
              this.forwardDirection == other.forwardDirection &&
              this.alignmentFile == other.alignmentFile &&
              this.startStation == other.startStation &&
              this.endStation == other.endStation &&
              this.leftOffset == other.leftOffset &&
              this.rightOffset == other.rightOffset &&
              this.machineDesignName == other.machineDesignName &&
              this.layerType == other.layerType &&
              this.designOrAlignmentFile == other.designOrAlignmentFile &&
              this.benchElevation == other.benchElevation &&
              this.layerNumber == other.layerNumber &&
              this.layerThickness == other.layerThickness &&
              Common.ListsAreEqual(this.contributingMachines, other.contributingMachines) &&
              Common.ListsAreEqual(this.surveyedSurfaceExclusionList, other.surveyedSurfaceExclusionList) &&
              this.returnEarliest == other.returnEarliest &&
              this.gpsAccuracy == other.gpsAccuracy &&
              this.gpsAccuracyIsInclusive == other.gpsAccuracyIsInclusive &&
              this.implementMapping == other.implementMapping &&
              this.trackMapping == other.trackMapping &&
              this.wheelMapping == other.wheelMapping;
      }

      public static bool operator ==(Filter a, Filter b)
      {
          if ((object)a == null || (object)b == null)
              return Object.Equals(a, b);

          return a.Equals(b);
      }

      public static bool operator !=(Filter a, Filter b)
      {
          return !(a == b);
      }

      public override bool Equals(object obj)
      {
          return obj is Filter && this == (Filter)obj;
      }

      public override int GetHashCode()
      {
          return base.GetHashCode();
      }
      #endregion

      #region ToString override
      /// <summary>
      /// ToString override
      /// </summary>
      /// <returns>A string representation.</returns>
      public override string ToString()
      {
          return JsonConvert.SerializeObject(this, Formatting.Indented);
      }
      #endregion
  }
}