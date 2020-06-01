﻿using System;
using Newtonsoft.Json;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  /// The request representation used to Create a project. 
  /// If CustomerUID is null, it will be populated via other means.
  /// This handles create of project, association to the customer and notification to raptor.
  /// </summary>
  public class CreateProjectRequest
  {
    /// <summary>
    /// The unique ID of the customer which the project is to be associated with. 
    /// if null, then the customer from the header will be used.
    /// </summary>
    [JsonProperty(PropertyName = "CustomerUID", Required = Required.Default)]
    public Guid? CustomerUID { get; set; } = null;

    /// <summary>
    /// The type of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectType", Required = Required.Always)]
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// The name of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectName", Required = Required.Always)]
    public string ProjectName { get; set; }

    /// <summary>
    /// The time zone of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectTimezone", Required = Required.Default)]
    public string ProjectTimezone { get; set; }

    /// <summary>
    /// The boundary of the project. This is now mutable.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectBoundary", Required = Required.Default)]
    public string ProjectBoundary { get; set; }

    /// <summary>
    /// The CS of the project. 
    /// This is required for landfills but optional for other project types.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileName", Required = Required.Default)]
    public string CoordinateSystemFileName { get; set; } = string.Empty;

    /// <summary>
    /// The guts of the CoordinateSystem to be contained in the CoordinateSystemFileContent. 
    /// Required if CoordinateSystemFilenAME is provided.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileContent", Required = Required.Default)]
    public byte[] CoordinateSystemFileContent { get; set; } = null;


    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectRequest()
    { }

    /// <summary>
    /// Create instance of CreateProjectRequest
    /// </summary>
    public static CreateProjectRequest CreateACreateProjectRequest(string customerUid,
      ProjectType projectType, string projectName, string projectTimezone, string projectBoundary,
      string coordinateSystemFileName, byte[] coordinateSystemFileContent
      )
    {
      return new CreateProjectRequest
      {
        CustomerUID = string.IsNullOrEmpty(customerUid) ? (Guid?)null : new Guid(customerUid),
        ProjectType = projectType,
        ProjectName = projectName,
        ProjectTimezone = projectTimezone,
        ProjectBoundary = projectBoundary,
        CoordinateSystemFileName = coordinateSystemFileName,
        CoordinateSystemFileContent = coordinateSystemFileContent
      };
    }
  }
}
