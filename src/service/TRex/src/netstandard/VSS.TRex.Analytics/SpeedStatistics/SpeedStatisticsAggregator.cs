﻿using System.Threading.Tasks;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Implements the specific business rules for calculating a Speed statistics
	/// </summary>
	public class SpeedStatisticsAggregator : DataStatisticsAggregator
  {
	  /// <summary>
	  /// Machine speed target record. It contains min/max machine speed target value.
	  /// </summary>
	  public MachineSpeedExtendedRecord TargetMachineSpeed;

	  /// <summary>
	  /// Default no-arg constructor
	  /// </summary>
	  public SpeedStatisticsAggregator()
	  {
			TargetMachineSpeed.Clear();
	  }

    /// <summary>
    /// Processes a Speed subgrid into a Speed isopach and calculate the counts of cells where the Speed value
    /// fits into the requested bands, i.e. less than min target, between min and max targets, greater than max target
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
          if ((subGrid?.Length ?? 0) > 0 && subGrid[0] is ClientMachineTargetSpeedLeafSubGrid SubGrid)
          {
            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
              var SpeedRangeValue = SubGrid.Cells[I, J];
              if (SpeedRangeValue.Max != CellPassConsts.NullMachineSpeed) // is there a value to test
              {
                SummaryCellsScanned++;
                if (SpeedRangeValue.Max > TargetMachineSpeed.Max)
                  CellsScannedOverTarget++;
                else if (SpeedRangeValue.Min < TargetMachineSpeed.Min && SpeedRangeValue.Max < TargetMachineSpeed.Min)
                  CellsScannedUnderTarget++;
                else
                  CellsScannedAtTarget++;
              }
            });
          }
        }
      }
    }
  }
}
