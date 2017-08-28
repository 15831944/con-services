using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Request representation for requesting project statistics
  /// </summary>
  public class ProjectStatisticsRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// The set of surveyed surfaces that should be excluded from the calculation of the spatial and temporal extents of the project.
    /// </summary>
    [JsonProperty(PropertyName = "excludedSurveyedSurfaceIds", Required = Required.Default)]
    public long[] excludedSurveyedSurfaceIds { get; private set; }

    public override void Validate()
    {
      base.Validate();
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;
      validator.TryValidate(this, out results);
      if (results.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
      }
    }

    public static ProjectStatisticsRequest CreateStatisticsParameters(long ProjectId, long[] ExcludedSurveyedSurfaceIds)
    {
      return new ProjectStatisticsRequest
      { 
                 projectId = ProjectId, 
                 excludedSurveyedSurfaceIds = ExcludedSurveyedSurfaceIds
             };
    }

    //Private constructor to hide the request builder
    private ProjectStatisticsRequest()
    { }

    /// <summary>
    /// Statistics parameters request help instance
    /// </summary>
    public new static ProjectStatisticsRequest HelpSample => new ProjectStatisticsRequest
    {
      projectId = 100,
      excludedSurveyedSurfaceIds = new long[] {1, 3, 5 }
    };
  }
}