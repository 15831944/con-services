﻿using System.Threading.Tasks;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics
{
  /// <summary>
  /// Implements the specific business rules for calculating a Pass Count summary
  /// </summary>
  public class PassCountStatisticsAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// The flag is to indicate whether or not the machine Pass Count target range to be user overrides.
    /// </summary>
    public bool OverrideTargetPassCount { get; set; }

    /// <summary>
    /// Pass Count target range.
    /// </summary>
    public PassCountRangeRecord OverridingTargetPassCountRange;

    /// <summary>
    /// Holds last known good target Pass Count range values.
    /// </summary>
    public PassCountRangeRecord LastPassCountTargetRange;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public PassCountStatisticsAggregator()
    {
      OverridingTargetPassCountRange.Clear();
      LastPassCountTargetRange.Clear();
    }

    protected override int GetMaximumValue() => CellPassConsts.MaxPassCountValue;

    protected override void DataCheck(DataStatisticsAggregator other)
    {
      var aggregator = (PassCountStatisticsAggregator) other;

      if (IsTargetValueConstant && aggregator.SummaryCellsScanned > 0) // If we need to check for a difference...
      {
        // Compare grouped results to determine if target varies...
        if (aggregator.LastPassCountTargetRange.Min != CellPassConsts.NullPassCountValue && aggregator.LastPassCountTargetRange.Max != CellPassConsts.NullPassCountValue &&
            LastPassCountTargetRange.Min != CellPassConsts.NullPassCountValue && LastPassCountTargetRange.Max != CellPassConsts.NullPassCountValue) // If data valid...
        {
          if (LastPassCountTargetRange.Min != aggregator.LastPassCountTargetRange.Min && LastPassCountTargetRange.Max != aggregator.LastPassCountTargetRange.Max) // Compare...
            IsTargetValueConstant = false;
        }
      }

      if (aggregator.LastPassCountTargetRange.Min != CellPassConsts.NullPassCountValue && aggregator.LastPassCountTargetRange.Max != CellPassConsts.NullPassCountValue) // If data valid...
        LastPassCountTargetRange = aggregator.LastPassCountTargetRange; // Set value...
    }

    /// <summary>
    /// Processes a Pass Count sub grid into a Pass Count isopach and calculate the counts of cells where the Pass Count value matches the requested target range.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      lock (this)
      {
        base.ProcessSubGridResult(subGrids);

        // Works out the percentage each colour on the map represents
        foreach (IClientLeafSubGrid[] subGrid in subGrids)
        {
          if ((subGrid?.Length ?? 0) > 0 && subGrid[0] is ClientPassCountLeafSubGrid SubGrid)
          {
            var currentPassTargetRange = new PassCountRangeRecord(CellPassConsts.NullPassCountValue, CellPassConsts.NullPassCountValue);

            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
              var passCountValue = SubGrid.Cells[I, J];

              if (passCountValue.MeasuredPassCount != CellPassConsts.NullPassCountValue) // Is there a value to test...
              {
                // This part is related to Summary data
                if (OverrideTargetPassCount)
                {
                  if (LastPassCountTargetRange.Min != OverridingTargetPassCountRange.Min && LastPassCountTargetRange.Max != OverridingTargetPassCountRange.Max)
                    LastPassCountTargetRange = OverridingTargetPassCountRange;

                  if (currentPassTargetRange.Min != OverridingTargetPassCountRange.Min && currentPassTargetRange.Max != OverridingTargetPassCountRange.Max)
                    currentPassTargetRange = OverridingTargetPassCountRange;
                }
                else
                {
                  // Using the machine target values test if target varies...
                  if (IsTargetValueConstant) // Do we need to test?..
                  {
                    if (passCountValue.TargetPassCount != CellPassConsts.NullPassCountValue && LastPassCountTargetRange.Min != CellPassConsts.NullPassCountValue &&
                        LastPassCountTargetRange.Max != CellPassConsts.NullPassCountValue) // Values all good to test...
                      IsTargetValueConstant = passCountValue.TargetPassCount >= LastPassCountTargetRange.Min && passCountValue.TargetPassCount <= LastPassCountTargetRange.Max; // Check to see if target value varies...
                  }

                  if (passCountValue.TargetPassCount != CellPassConsts.NullPassCountValue && (passCountValue.TargetPassCount < LastPassCountTargetRange.Min || passCountValue.TargetPassCount > LastPassCountTargetRange.Max))
                    LastPassCountTargetRange.SetMinMax(passCountValue.TargetPassCount, passCountValue.TargetPassCount); // ConstantPassTarget holds last good values...

                  if (passCountValue.TargetPassCount < currentPassTargetRange.Min || passCountValue.TargetPassCount > currentPassTargetRange.Max)
                    currentPassTargetRange.SetMinMax(passCountValue.TargetPassCount, passCountValue.TargetPassCount);
                }

                if (currentPassTargetRange.Min != CellPassConsts.NullPassCountValue && currentPassTargetRange.Max != CellPassConsts.NullPassCountValue)
                {
                  SummaryCellsScanned++; // For summary only...
                  if (passCountValue.MeasuredPassCount > LastPassCountTargetRange.Max)
                    CellsScannedOverTarget++;
                  else if (passCountValue.MeasuredPassCount < LastPassCountTargetRange.Min)
                    CellsScannedUnderTarget++;
                  else
                    CellsScannedAtTarget++;
                }
                else // We have data but no target data to do a summary of cell...
                  MissingTargetValue = true; // Flag to issue a warning to user...

                IncrementCountOfTransition(passCountValue.MeasuredPassCount); // Pass count detail is counted here...
              }
            });
          }
        }
      }
    }
  }
}
