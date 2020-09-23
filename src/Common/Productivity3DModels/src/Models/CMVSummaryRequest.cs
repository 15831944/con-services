﻿using System;
using VSS.Productivity3D.Filter.Abstractions.Models;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request CMV summary.
  /// </summary>
  public class CMVSummaryRequest : TRexBaseRequest
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CMVSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CMVSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      short cmvTarget,
      bool overrideTargetCMV,
      double maxCMVPercent,
      double minCMVPercent,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(cmvTarget: cmvTarget, overrideTargetCMV: overrideTargetCMV, maxCMVPercent: maxCMVPercent, minCMVPercent: minCMVPercent);
      LiftSettings = liftSettings;
    }
  }
}
