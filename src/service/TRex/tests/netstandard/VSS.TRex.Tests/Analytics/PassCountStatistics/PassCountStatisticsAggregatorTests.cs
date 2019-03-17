﻿using System;
using FluentAssertions;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.PassCountStatistics
{
  public class PassCountStatisticsAggregatorTests
  {
    [Fact]
    public void Test_PassCountStatisticsAggregator_Creation()
    {
      var aggregator = new PassCountStatisticsAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(!aggregator.OverrideTargetPassCount, "Invalid initial value for OverrideTargetPassCount.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Min == CellPassConsts.NullPassCountValue, "Invalid initial value for OverridingTargetPassCountRange.Min.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Max == CellPassConsts.NullPassCountValue, "Invalid initial value for OverridingTargetPassCountRange.Max.");
      Assert.True(aggregator.LastPassCountTargetRange.Min == CellPassConsts.NullPassCountValue, "Invalid initial value for LastPassCountTargetRange.Min.");
      Assert.True(aggregator.LastPassCountTargetRange.Max == CellPassConsts.NullPassCountValue, "Invalid initial value for LastPassCountTargetRange.Max.");

      Assert.True(aggregator.DetailsDataValues == null, "Invalid initial value for DetailsDataValues.");
      Assert.True(aggregator.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_PassCountStatisticsAggregator_ProcessResult_NoAggregation_Details()
    {
      var aggregator = new PassCountStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      aggregator.CellSize = TestConsts.CELL_SIZE;
      aggregator.DetailsDataValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      aggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      Assert.True(aggregator.Counts.Length == aggregator.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] > 0, $"Invalid value for Counts[{i}].");
    }

    [Fact]
    public void Test_PassCountStatisticsAggregator_ProcessResult_NoAggregation_Summary()
    {
      var aggregator = new PassCountStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = TestConsts.CELL_SIZE;
      aggregator.OverrideTargetPassCount = true;
      aggregator.OverridingTargetPassCountRange = new PassCountRangeRecord((ushort)length, (ushort)length);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_PassCountStatisticsAggregator_ProcessResult_WithAggregation_Details()
    {
      var aggregator = new PassCountStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = TestConsts.CELL_SIZE;
      aggregator.DetailsDataValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      aggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new PassCountStatisticsAggregator();

      otherAggregator.CellSize = TestConsts.CELL_SIZE;
      otherAggregator.DetailsDataValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      otherAggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      otherAggregator.ProcessSubGridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.Counts.Length == aggregator.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] == otherAggregator.Counts[i] * 2, $"Invalid aggregated value for Counts[{i}].");
    }

    [Fact]
    public void Test_PassCountStatisticsAggregator_ProcessResult_WithAggregation_Summary()
    {
      var aggregator = new PassCountStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = TestConsts.CELL_SIZE;
      aggregator.OverrideTargetPassCount = true;
      aggregator.OverridingTargetPassCountRange = new PassCountRangeRecord((ushort)length, (ushort)length);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new PassCountStatisticsAggregator();

      otherAggregator.CellSize = TestConsts.CELL_SIZE;
      otherAggregator.OverrideTargetPassCount = true;
      otherAggregator.OverridingTargetPassCountRange = new PassCountRangeRecord((ushort)length, (ushort)length);

      otherAggregator.ProcessSubGridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.SummaryCellsScanned == dLength * 2, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - 2 * dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length * 2, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == (dLength - length) * 2, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_PassCountStatisticsAggregator_AggregateWith_SamePassCountTargets()
    {
      var aggregator = new PassCountStatisticsAggregator
      {
        LastPassCountTargetRange = new PassCountRangeRecord(2, 4),
        SummaryCellsScanned = 1
      };

      var otheraggregator = new PassCountStatisticsAggregator
      {
        LastPassCountTargetRange = new PassCountRangeRecord(2, 4),
        SummaryCellsScanned = 1
      };

      aggregator.IsTargetValueConstant.Should().BeTrue();
      otheraggregator.IsTargetValueConstant.Should().BeTrue();

      aggregator.AggregateWith(otheraggregator);
      aggregator.IsTargetValueConstant.Should().BeTrue();
    }

    [Fact]
    public void Test_PassCountStatisticsAggregator_AggregateWith_DifferingPassCountTargets()
    {
      var aggregator = new PassCountStatisticsAggregator
      {
        LastPassCountTargetRange = new PassCountRangeRecord(2, 4),
        SummaryCellsScanned = 1
      };

      var otheraggregator = new PassCountStatisticsAggregator
      {
        LastPassCountTargetRange = new PassCountRangeRecord(3, 5),
        SummaryCellsScanned = 1
      };

      aggregator.IsTargetValueConstant.Should().BeTrue();
      otheraggregator.IsTargetValueConstant.Should().BeTrue();

      aggregator.AggregateWith(otheraggregator);
      aggregator.IsTargetValueConstant.Should().BeFalse();
    }
  }
}
