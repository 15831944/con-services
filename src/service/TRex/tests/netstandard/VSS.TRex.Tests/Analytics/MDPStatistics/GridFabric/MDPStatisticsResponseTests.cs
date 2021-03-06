﻿using System;
using FluentAssertions;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.MDPStatistics.GridFabric
{
  public class MDPStatisticsResponseTests
  {
    private MDPStatisticsResponse _response => new MDPStatisticsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      CellSize = TestConsts.CELL_SIZE,
      CellsScannedOverTarget = TestConsts.CELLS_OVER_TARGET,
      CellsScannedAtTarget = TestConsts.CELLS_AT_TARGET,
      CellsScannedUnderTarget = TestConsts.CELLS_UNDER_TARGET,
      SummaryCellsScanned = TestConsts.CELLS_OVER_TARGET + TestConsts.CELLS_AT_TARGET + TestConsts.CELLS_UNDER_TARGET,
      IsTargetValueConstant = true,
      LastTargetMDP = 70
    };

    [Fact]
    public void Test_MDPStatisticsResponse_Creation()
    {
      var response = new MDPStatisticsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.CellSize < Consts.TOLERANCE_DIMENSION, "CellSize invalid after creation.");
      Assert.True(response.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(response.LastTargetMDP == 0, "Invalid initial value for LastTargetMDP.");
      Assert.True(response.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!response.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
    }

    [Fact]
    public void Test_MDPStatisticsResponse_ConstructResult_Successful()
    {
      Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

      var result = _response.ConstructResult();

      Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

      Assert.True(result.ConstantTargetMDP == _response.LastTargetMDP, "Invalid initial result value for ConstantTargetMDP.");
      Assert.True(Math.Abs(result.AboveTargetPercent - _response.ValueOverTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for AboveMDPPercent.");
      Assert.True(Math.Abs(result.WithinTargetPercent - _response.ValueAtTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for WithinMDPPercent.");
      Assert.True(Math.Abs(result.BelowTargetPercent - _response.ValueUnderTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for BelowMDPPercent.");
      Assert.True(Math.Abs(result.TotalAreaCoveredSqMeters - _response.SummaryProcessedArea) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for TotalAreaCoveredSqMeters.");
      Assert.True(result.IsTargetMDPConstant == _response.IsTargetValueConstant, "Invalid initial result value for IsTargetMDPConstant.");

      result.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
    }

    [Fact]
    public void Test_MDPStatisticsResponse_AgregateWith_Successful()
    {
      var responseClone = new MDPStatisticsResponse()
      {
        ResultStatus = _response.ResultStatus,
        CellSize = _response.CellSize,
        CellsScannedOverTarget = _response.CellsScannedOverTarget,
        CellsScannedAtTarget = _response.CellsScannedAtTarget,
        CellsScannedUnderTarget = _response.CellsScannedUnderTarget,
        SummaryCellsScanned = _response.SummaryCellsScanned,
        IsTargetValueConstant = _response.IsTargetValueConstant,
        LastTargetMDP = _response.LastTargetMDP
      };

      var response = _response.AggregateWith(responseClone);

      Assert.True(Math.Abs(response.CellSize - _response.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after aggregation.");
      Assert.True(response.SummaryCellsScanned == _response.SummaryCellsScanned * 2, "Invalid aggregated value for SummaryCellsScanned.");
      Assert.True(response.LastTargetMDP == _response.LastTargetMDP, "Invalid aggregated value for LastTargetMDP.");
      Assert.True(response.CellsScannedOverTarget == _response.CellsScannedOverTarget * 2, "Invalid aggregated value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == _response.CellsScannedAtTarget * 2, "Invalid aggregated value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == _response.CellsScannedUnderTarget * 2, "Invalid aggregated value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == _response.IsTargetValueConstant, "Invalid aggregated value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == _response.MissingTargetValue, "Invalid aggregated value for MissingTargetValue.");
    }
  }
}
