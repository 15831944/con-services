﻿using System;
using FluentAssertions;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics.GridFabric
{
  public class CMVStatisticsResponseTests
  {
    private CMVStatisticsResponse _response => new CMVStatisticsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      CellSize = TestConsts.CELL_SIZE,
      CellsScannedOverTarget = TestConsts.CELLS_OVER_TARGET,
      CellsScannedAtTarget = TestConsts.CELLS_AT_TARGET,
      CellsScannedUnderTarget = TestConsts.CELLS_UNDER_TARGET,
      SummaryCellsScanned = TestConsts.CELLS_OVER_TARGET + TestConsts.CELLS_AT_TARGET + TestConsts.CELLS_UNDER_TARGET,
      IsTargetValueConstant = true,
      LastTargetCMV = 70,
      Counts = new long[] { 10, 50, 100, 150, 200, 250, 300, 350, 400 }
    };

    [Fact]
    public void Test_CMVStatisticsResponse_Creation()
    {
      var response = new CMVStatisticsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.CellSize < Consts.TOLERANCE_DIMENSION, "CellSize invalid after creation.");
      Assert.True(response.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(response.LastTargetCMV == 0, "Invalid initial value for LastTargetCMV.");
      Assert.True(response.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!response.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(response.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_CMVStatisticsResponse_ConstructResult_Successful()
    {
      Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

      var result = _response.ConstructResult();

      Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

      Assert.True(result.ConstantTargetCMV ==_response.LastTargetCMV, "Invalid initial result value for ConstantTargetCMV.");
      Assert.True(Math.Abs(result.AboveTargetPercent - _response.ValueOverTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for AboveCMVPercent.");
      Assert.True(Math.Abs(result.WithinTargetPercent - _response.ValueAtTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for WithinCMVPercent.");
      Assert.True(Math.Abs(result.BelowTargetPercent - _response.ValueUnderTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for BelowCMVPercent.");
      Assert.True(Math.Abs(result.TotalAreaCoveredSqMeters - _response.SummaryProcessedArea) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for TotalAreaCoveredSqMeters.");
      Assert.True(result.IsTargetCMVConstant == _response.IsTargetValueConstant, "Invalid initial result value for IsTargetCMVConstant.");

      Assert.True(result.Counts.Length == _response.Counts.Length, "Invalid value for Counts.");
      for (int i = 0; i < result.Counts.Length; i++)
        Assert.True(result.Counts[i] == _response.Counts[i], $"Invalid value for Counts[{i}].");

      result.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
    }

    [Fact]
    public void Test_CMVStatisticsResponse_AgregateWith_Successful()
    {
      var responseClone = new CMVStatisticsResponse()
      {
        ResultStatus = _response.ResultStatus,
        CellSize = _response.CellSize,
        CellsScannedOverTarget = _response.CellsScannedOverTarget,
        CellsScannedAtTarget = _response.CellsScannedAtTarget,
        CellsScannedUnderTarget = _response.CellsScannedUnderTarget,
        SummaryCellsScanned = _response.SummaryCellsScanned,
        IsTargetValueConstant = _response.IsTargetValueConstant,
        LastTargetCMV = _response.LastTargetCMV,
        Counts = _response.Counts
      };

      var response = _response.AggregateWith(responseClone);

      Assert.True(Math.Abs(response.CellSize - _response.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after aggregation.");
      Assert.True(response.SummaryCellsScanned == _response.SummaryCellsScanned * 2, "Invalid aggregated value for SummaryCellsScanned.");
      Assert.True(response.LastTargetCMV == _response.LastTargetCMV, "Invalid aggregated value for LastTargetCMV.");
      Assert.True(response.CellsScannedOverTarget == _response.CellsScannedOverTarget * 2, "Invalid aggregated value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == _response.CellsScannedAtTarget * 2, "Invalid aggregated value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == _response.CellsScannedUnderTarget * 2, "Invalid aggregated value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == _response.IsTargetValueConstant, "Invalid aggregated value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == _response.MissingTargetValue, "Invalid aggregated value for MissingTargetValue.");

      Assert.True(response.Counts.Length == _response.Counts.Length, "Invalid value for Counts.");
      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] > _response.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }
  }
}
