﻿using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class CutFillDetailsRequest : ProjectID
  {
    /// <summary>
    /// The collection of cut-fill tolerances to use. There must be 7 of them - 
    /// 3 cut values greater than zero, on-grade equal to zero and 3 fill values less than zero.
    /// Values are in meters.
    /// </summary>
    public double[] CutFillTolerances { get; private set; }
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public Filter filter { get; private set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// The descriptor for the design for which to to generate the cut-fill data.
    /// </summary>
    public DesignDescriptor designDescriptor { get; private set; }

    public static CutFillDetailsRequest CreateCutFillDetailsRequest(long projectId, double[] tolerances, Filter filter, LiftBuildSettings liftBuildSettings, DesignDescriptor designDescriptor)
    {
      return new CutFillDetailsRequest
      {
        projectId = projectId,
        CutFillTolerances = tolerances,
        filter = filter,
        liftBuildSettings = liftBuildSettings,
        designDescriptor = designDescriptor
      };
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (filter != null)
      {
        filter.Validate();  
      }

      if (liftBuildSettings != null)
      {
        liftBuildSettings.Validate();
      }

      if (designDescriptor != null)
      {
        designDescriptor.Validate();
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Design must be specified for cut-fill details"));
      }
    }

  }
}
