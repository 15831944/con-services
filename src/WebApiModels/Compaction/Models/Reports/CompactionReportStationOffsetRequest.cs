﻿using Newtonsoft.Json;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using Filter = VSS.Productivity3D.Common.Models.Filter;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  /// <summary>
  /// The request representation for getting production data from Raptor for a grid report.
  /// </summary>
  public class CompactionReportStationOffsetRequest : CompactionReportRequest
  {
    /// <summary>
    /// Sets the alignment file to be used for station/offset calculations
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Always)]
    public DesignDescriptor AlignmentFile { get; protected set; }

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double CrossSectionInterval { get; protected set; }

    [JsonProperty(Required = Required.Always)]
    public double StartStation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public double EndStation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public double[] Offsets { get; set; }

    public UserPreferenceData UserPreferences { get; set; }

    protected CompactionReportStationOffsetRequest()
    { }

    public static CompactionReportStationOffsetRequest CreateRequest(
      long projectId,
      Filter filter,
      long filterId,
      LiftBuildSettings liftBuildSettings,
      bool reportElevation,
      bool reportCmv,
      bool reportMdp,
      bool reportPassCount,
      bool reportTemperature,
      bool reportCutFill,
      DesignDescriptor cutFillDesignDescriptor,
      DesignDescriptor alignmentDescriptor,
      double crossSectionInterval,
      double startStation,
      double endStation,
      double[] offsets,
      UserPreferenceData userPreferences)
    {
      return new CompactionReportStationOffsetRequest
      {
        projectId = projectId,
        Filter = filter,
        FilterID = filterId,
        LiftBuildSettings = liftBuildSettings,
        ReportElevation = reportElevation,
        ReportCMV = reportCmv,
        ReportMDP = reportMdp,
        ReportPassCount = reportPassCount,
        ReportTemperature = reportTemperature,
        ReportCutFill = reportCutFill,
        DesignFile = cutFillDesignDescriptor,
        AlignmentFile = alignmentDescriptor,
        CrossSectionInterval = crossSectionInterval,
        StartStation = startStation,
        EndStation = endStation,
        Offsets = offsets,
        UserPreferences = userPreferences
      };
    }

    /// <summary>
    /// Validates properties.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (AlignmentFile == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Alignment file must be specified for station and offset report."));

      }
      AlignmentFile?.Validate();

      if (this.CrossSectionInterval < ValidationConstants.MIN_SPACING_INTERVAL || this.CrossSectionInterval > ValidationConstants.MAX_SPACING_INTERVAL)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Interval must be >= {ValidationConstants.MIN_SPACING_INTERVAL}m and <= {ValidationConstants.MAX_SPACING_INTERVAL}m. Actual value: {this.CrossSectionInterval}"));
      }

      if (Offsets == null || Offsets.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Offsets must be specified for station and offset report."));
      }

      if (StartStation < 0 || EndStation < 0 || StartStation > EndStation)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid station range for station and offset report."));
      }
    }
  }
}