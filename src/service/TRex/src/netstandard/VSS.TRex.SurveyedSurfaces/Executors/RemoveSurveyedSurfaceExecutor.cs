﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.Executors
{
  public class RemoveSurveyedSurfaceExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RemoveSurveyedSurfaceExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public DesignProfilerRequestResult Execute(Guid projectUid, Guid surveyedSurfaceUid)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          _log.LogError($"Site model {projectUid} not found");
          return DesignProfilerRequestResult.NoSelectedSiteModel;
        }

        var removed = DIContext.Obtain<ISurveyedSurfaceManager>().Remove(projectUid, surveyedSurfaceUid);

        if (!removed)
        {
          _log.LogError($"Failed to remove surveyed surface {surveyedSurfaceUid} from project {projectUid} as it may not exist in the project");
          return DesignProfilerRequestResult.DesignDoesNotExist;
        }

        return DesignProfilerRequestResult.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return DesignProfilerRequestResult.UnknownError;
      }
    }
  }
}