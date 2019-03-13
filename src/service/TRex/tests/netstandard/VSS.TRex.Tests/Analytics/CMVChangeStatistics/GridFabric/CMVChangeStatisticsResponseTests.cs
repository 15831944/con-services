﻿using System;
using System.Linq;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVChangeStatistics.GridFabric
{
  public class CMVChangeStatisticsResponseTests
  {
    private CMVChangeStatisticsResponse _response => new CMVChangeStatisticsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      CellSize = TestConsts.CELL_SIZE,
      CellsScannedOverTarget = TestConsts.CELLS_OVER_TARGET,
      CellsScannedAtTarget = TestConsts.CELLS_AT_TARGET,
      CellsScannedUnderTarget = TestConsts.CELLS_UNDER_TARGET,
      SummaryCellsScanned = TestConsts.CELLS_OVER_TARGET + TestConsts.CELLS_AT_TARGET + TestConsts.CELLS_UNDER_TARGET,
      IsTargetValueConstant = true,
      Counts = TestConsts.CountsArray
    };

    [Fact]
    public void Test_CMVChangeStatisticsResponse_Creation()
    {
      var response = new CMVChangeStatisticsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.CellSize < Consts.TOLERANCE_DIMENSION, "CellSize invalid after creation.");
      Assert.True(response.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(response.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!response.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(response.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_CMVChangeStatisticsResponse_ConstructResult_Successful()
    {
      Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

      var result = _response.ConstructResult();

      Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");
      Assert.True(Math.Abs(result.TotalAreaCoveredSqMeters - _response.SummaryProcessedArea) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for TotalAreaCoveredSqMeters.");
      Assert.True(result.Counts.Length == _response.Counts.Length, "Invalid value for Counts.");
      for (int i = 0; i < result.Counts.Length; i++)
        Assert.True(result.Counts[i] == _response.Counts[i], $"Invalid value for Counts[{i}].");
    }

    [Fact]
    public void Test_CMVChangeStatisticsResponse_AgregateWith_Successful()
    {
      var responseClone = new CMVChangeStatisticsResponse()
      {
        ResultStatus = _response.ResultStatus, 
        CellSize = _response.CellSize,
        CellsScannedOverTarget = _response.CellsScannedOverTarget,
        CellsScannedAtTarget = _response.CellsScannedAtTarget,
        CellsScannedUnderTarget = _response.CellsScannedUnderTarget,
        SummaryCellsScanned = _response.SummaryCellsScanned,
        IsTargetValueConstant = _response.IsTargetValueConstant,
        Counts = _response.Counts.ToArray()
      };

      var response = _response.AggregateWith(responseClone);

      Assert.True(Math.Abs(response.CellSize - _response.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after aggregation.");
      Assert.True(response.SummaryCellsScanned == _response.SummaryCellsScanned * 2, "Invalid aggregated value for SummaryCellsScanned.");
      Assert.True(Math.Abs(response.SummaryProcessedArea - _response.SummaryProcessedArea * 2) < Consts.TOLERANCE_DIMENSION, "Invalid aggregated value for SummaryProcessedArea.");

      Assert.True(response.Counts.Length == responseClone.Counts.Length, "Invalid value for Counts.");
      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] > responseClone.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }
  }
}
