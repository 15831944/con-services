﻿using System;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.Types;
using Xunit;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVStatisticsAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_CMVStatisticsAggregator_Creation()
    {
      var aggregator = new CMVStatisticsAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(!aggregator.OverrideMachineCMV, "Invalid initial value for OverrideTemperatureWarningLevels.");
      Assert.True(aggregator.OverridingMachineCMV == CellPassConsts.NullCCV, "Invalid initial value for OverridingMachineCMV.");
      Assert.True(aggregator.LastTargetCMV == CellPassConsts.NullCCV, "Invalid initial value for LastTargetCMV.");

      Assert.True(aggregator.DetailsDataValues == null, "Invalid initial value for DetailsDataValues.");
      Assert.True(aggregator.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_CMVStatisticsAggregator_ProcessResult_NoAggregation_Details()
    {
      var aggregator = new CMVStatisticsAggregator();

      var clientGrid = new ClientCMVLeafSubGrid();

      clientGrid.FillWithTestPattern();

      aggregator.CellSize = CELL_SIZE;
      aggregator.DetailsDataValues = new[] { 1, 5, 10, 15, 20, 25, 31 };
      aggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new [] { new [] { clientGrid } };
      
      aggregator.ProcessSubGridResult(subGrids);

      Assert.True(aggregator.Counts.Length == aggregator.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] > 0, $"Invalid value for Counts[{i}].");
    }

    [Fact]
    public void Test_CMVStatisticsAggregator_ProcessResult_NoAggregation_Summary()
    {
      var aggregator = new CMVStatisticsAggregator();
      var clientGrid = new ClientCMVLeafSubGrid(); 

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideMachineCMV = true;
      aggregator.OverridingMachineCMV = (short)(length - 1);
      aggregator.CMVPercentageRange = new CMVRangePercentageRecord(100, 100);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_CMVStatisticsAggregator_ProcessResult_WithAggregation_Details()
    {
      var aggregator = new CMVStatisticsAggregator();

      var clientGrid = new ClientCMVLeafSubGrid();

      clientGrid.FillWithTestPattern();

      aggregator.CellSize = CELL_SIZE;
      aggregator.DetailsDataValues = new[] { 1, 5, 10, 15, 20, 25, 31 };
      aggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new CMVStatisticsAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.DetailsDataValues = new[] { 1, 5, 10, 15, 20, 25, 31 };
      otherAggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      otherAggregator.ProcessSubGridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.Counts.Length == aggregator.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] == otherAggregator.Counts[i] * 2, $"Invalid aggregated value for Counts[{i}].");
    }

    [Fact]
    public void Test_CMVStatisticsAggregator_ProcessResult_WithAggregation_Summary()
    {
      var aggregator = new CMVStatisticsAggregator();
      var clientGrid = new ClientCMVLeafSubGrid(); 

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideMachineCMV = true;
      aggregator.OverridingMachineCMV = (short)(length - 1);
      aggregator.CMVPercentageRange = new CMVRangePercentageRecord(100, 100);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new CMVStatisticsAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.OverrideMachineCMV = true;
      otherAggregator.OverridingMachineCMV = (short)(length - 1);
      otherAggregator.CMVPercentageRange = new CMVRangePercentageRecord(100, 100);

      otherAggregator.ProcessSubGridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.SummaryCellsScanned == dLength * 2, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - 2 * dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length * 2, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == (dLength - length) * 2, "Invalid value for CellsScannedUnderTarget.");
    }
  }
}
