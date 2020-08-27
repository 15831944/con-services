﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CCAStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a CCA statistics request
  /// </summary>
  public class CCAStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<CCAStatisticsResponse>,
    IAnalyticsOperationResponseResultConversion<CCAStatisticsResult>
  {
    private static byte VERSION_NUMBER = 1;

    /// <summary>
    /// Holds last known good target CCA value.
    /// </summary>
    public byte LastTargetCCA { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByte(LastTargetCCA);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        LastTargetCCA = reader.ReadByte();
      }
    }

    /// <summary>
    /// Aggregate a set of CCA summary into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetCCA = ((CCAStatisticsResponse)other).LastTargetCCA;
    }

    public CCAStatisticsResponse AggregateWith(CCAStatisticsResponse other)
    {
      return base.AggregateWith(other) as CCAStatisticsResponse;
    }

    public CCAStatisticsResult ConstructResult()
    {
      return new CCAStatisticsResult
      {
        IsTargetCCAConstant = IsTargetValueConstant,
        ConstantTargetCCA = IsTargetValueConstant ? LastTargetCCA : (short)-1,
        AboveTargetPercent = ValueOverTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        BelowTargetPercent = ValueUnderTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        ReturnCode = MissingTargetValue ? SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoResult : MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.NoProblems,

        ResultStatus = ResultStatus
      };
    }
  }
}
