﻿using System.Diagnostics;

namespace VSS.TRex.Analytics.Foundation.Aggregators
{
  /// <summary>
  /// Base class used by data analytics aggregators supporting functions such as pass count summary/details, cut/fill details, speed summary etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public class DataStatisticsAggregator : AggregatorBase
  {
    /// <summary>
    /// Aggregator state is now single threaded in the context of processing sub grid
    /// information into it as the processing threads access independent sub-state aggregators which
    /// are aggregated together to form the final aggregation result. However, in contexts that do support
    /// threaded access to this structure the FRequiresSerialisation flag should be set
    /// </summary>
    public bool RequiresSerialisation { get; set; }

    /// <summary>
    /// Details data values.
    /// </summary>
    public int[] DetailsDataValues { get; set; }

    /// <summary>
    /// An array values representing the counts of cells within each of the CMV details bands defined in the request.
    /// The array's size is the same as the number of the CMV details bands.
    /// </summary>
    public long[] Counts { get; set; }

    /// <summary>
    /// The number of cells scanned while summarizing information in the resulting analytics, report or export
    /// </summary>
    public int SummaryCellsScanned { get; set; }

    /// <summary>
    /// The number of cells scanned where the value from the cell was in the target value range
    /// </summary>
    public int CellsScannedAtTarget { get; set; }

    /// <summary>
    /// The number of cells scanned where the value from the cell was over the target value range
    /// </summary>
    public int CellsScannedOverTarget { get; set; }

    /// <summary>
    /// The number of cells scanned where the value from the cell was below the target value range
    /// </summary>
    public int CellsScannedUnderTarget { get; set; }

    /// <summary>
    /// Were the target values for all data extracted for the analytics requested the same
    /// </summary>
    public bool IsTargetValueConstant { get; set; } = true;

    /// <summary>
    /// Were there any missing target values within the data extracted for the analytics request
    /// </summary>
    public bool MissingTargetValue { get; set; }

    public double ValueAtTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedAtTarget / SummaryCellsScanned * 100 : 0;

    public double ValueOverTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedOverTarget / SummaryCellsScanned * 100 : 0;

    public double ValueUnderTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedUnderTarget / SummaryCellsScanned * 100 : 0;

    public double SummaryProcessedArea => SummaryCellsScanned * (CellSize * CellSize);

    /// <summary>
    /// Combine this aggregator with another aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public DataStatisticsAggregator AggregateWith(DataStatisticsAggregator other)
    {
      AggregateBaseDataWith(other);

      DataCheck(other);

      return this;
    }

    /// <summary>
    /// Aggregate a set of generic data statistics into this set.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void AggregateBaseDataWith(DataStatisticsAggregator other)
    {
      CellSize = other.CellSize;

      // Details...
      if (Counts != null && other.Counts != null)
      {
        Counts = Counts ?? new long[other.Counts.Length];

        Debug.Assert(Counts.Length == other.Counts.Length);

        for (int i = 0; i < Counts.Length; i++)
          Counts[i] += other.Counts[i];
      }

      // Summary...
      SummaryCellsScanned += other.SummaryCellsScanned;

      CellsScannedAtTarget += other.CellsScannedAtTarget;
      CellsScannedOverTarget += other.CellsScannedOverTarget;
      CellsScannedUnderTarget += other.CellsScannedUnderTarget;

      if (other.SummaryCellsScanned > 0)
      {
        IsTargetValueConstant &= other.IsTargetValueConstant;
        MissingTargetValue |= other.MissingTargetValue;
      }
    }

    protected virtual void DataCheck(DataStatisticsAggregator other)
    {
      // Nothing to implement...
    }

    protected virtual void IncrementCountOfTransition(double value)
    {
      if (DetailsDataValues == null || Counts == null)
        return;

      Debug.Assert(DetailsDataValues.Length == Counts.Length, "Invalid size of the Counts array.");

      for (int i = 0; i < DetailsDataValues.Length; i++)
      {
        var startTransitionValue = DetailsDataValues[i];
        var endTransitionValue = i < DetailsDataValues.Length - 1 ? DetailsDataValues[i + 1] : GetMaximumValue();

        if (value >= startTransitionValue && value < endTransitionValue)
        {
          Counts[i]++;
          break;
        }
      }
    }

    protected virtual int GetMaximumValue()
    {
      // No implementation in base class
      return 0;
    }
  }
}
