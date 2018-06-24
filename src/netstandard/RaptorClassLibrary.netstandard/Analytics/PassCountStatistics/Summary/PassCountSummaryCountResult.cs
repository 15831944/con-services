﻿using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics.Summary
{
  /// <summary>
  /// The result obtained from performing a Pass Count analytics request
  /// </summary>
  public class PassCountSummaryCountResult : SummaryAnalyticsResult
  {
    /// <summary>
    /// Are the Pass Count target range values applying to all processed cells constant?
    /// </summary>
    public bool IsTargetPassCountConstant { get; set; }

    /// <summary>
    /// The Pass Count target range values applied to all processed cells.
    /// </summary>
    public PassCountRangeRecord ConstantTargetPassCountRange;

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public MissingTargetDataResultType ReturnCode { get; set; }
  }
}
