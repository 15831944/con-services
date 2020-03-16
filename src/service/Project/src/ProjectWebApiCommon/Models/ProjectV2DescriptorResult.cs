﻿using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  /// <summary>
  ///   Single project descriptor
  /// </summary>
  ///   /// <seealso cref="ContractExecutionResult" />
  public class ProjectV2DescriptorResult : ContractExecutionResult
  {
    /// <summary>
    /// The id for the project.
    /// </summary>
    /// <value>
    /// The legacy project ID.
    /// </value>
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public long ShortRaptorProjectId { get; set; }

    /// <summary>
    /// The name for the project.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }

    /// <summary>
    /// The start date for the project.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    [JsonProperty(PropertyName = "startDate", Required = Required.Default)]
    public string StartDate { get; set; }

    /// <summary>
    /// The end date for the project.
    /// </summary>
    /// <value>
    /// The end date.
    /// </value>
    [JsonProperty(PropertyName = "endDate", Required = Required.Default)]
    public string EndDate { get; set; }

    /// <summary>
    /// The project type: Standard = 0 (default), Landfill = 1, ProjectMonitoring = 2  
    /// </summary>
    /// <value>
    /// The type of the project.
    /// </value>
    [JsonProperty(PropertyName = "projectType", Required = Required.Default)]
    public ProjectType ProjectType { get; set; }

    

    public override bool Equals(object obj)
    {
      if (!(obj is ProjectV2DescriptorResult otherProject)) return false;
      return otherProject.ShortRaptorProjectId == this.ShortRaptorProjectId
             && otherProject.ProjectType == this.ProjectType
             && otherProject.Name == this.Name
             && otherProject.StartDate == this.StartDate
             && otherProject.EndDate == this.EndDate
          ;
    }

    public override int GetHashCode() { return 0; }
  }
}
