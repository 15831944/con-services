using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;

namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// Represent response with Project IDs available for a user
  /// </summary>
  public class ProjectListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets the list of project ids.
    /// </summary>
    /// <value>
    /// The list project ids.
    /// </value>
    [JsonProperty(PropertyName = "ProjectIds", Required = Required.Default)]
    public Dictionary<long, ProjectDescriptor> ProjectIds { get; private set; }

    private ProjectListResult()
    {
      // ...
    }

    /// <summary>
    /// Creates the project list result.
    /// </summary>
    /// <param name="projectIDs">The project ids.</param>
    /// <returns></returns>
    public static ProjectListResult CreateProjectListResult(Dictionary<long, ProjectDescriptor> projectIDs)
    {
      return new ProjectListResult() { ProjectIds = projectIDs };
    }
  }
}