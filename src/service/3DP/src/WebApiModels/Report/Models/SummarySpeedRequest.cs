﻿using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// Represents speed summary request.
  /// </summary>
  public class SummarySpeedRequest : ProjectID
  {

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; protected set; }

    /// <summary>
    /// The filter to be used 
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; protected set; }

    /// <summary>
    /// Gets or sets the filter identifier.
    /// </summary>
    /// <value>
    /// The filter identifier.
    /// </value>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public int FilterId { get; protected set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }

    public override void Validate()
    {
      base.Validate();
      if (this.LiftBuildSettings.MachineSpeedTarget == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Target speed must be specified for the request."));

      this.LiftBuildSettings?.Validate();
    }

    /// <summary>
    /// Default protected constructor.
    /// </summary>
    protected SummarySpeedRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="callId"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="filter"></param>
    /// <param name="filterId"></param>
    /// <returns></returns>
    public SummarySpeedRequest(
      long projectId,
      Guid? projectUid,
      Guid? callId,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      int filterId)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      CallId = callId;
      LiftBuildSettings = liftBuildSettings;
      Filter = filter;
      FilterId = filterId;
    }
  }
}
