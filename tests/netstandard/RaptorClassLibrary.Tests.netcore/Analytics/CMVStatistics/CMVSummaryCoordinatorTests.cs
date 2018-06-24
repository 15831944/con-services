﻿using System;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary;
using VSS.TRex.Analytics.CMVStatistics.Summary;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVSummaryCoordinatorTests : BaseCoordinatorTests
  {
    private CMVSummaryArgument Arg => new CMVSummaryArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
      OverrideMachineCMV = true,
      OverridingMachineCMV = 70
    };

    private CMVSummaryCoordinator _getCoordinator()
    {
      return new CMVSummaryCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private CMVSummaryAggregator _getCMVAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as CMVSummaryAggregator;
    }

    [Fact]
    public void Test_CMVSummaryCoordinator_Creation()
    {
      var coordinator = new CMVSummaryCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_CMVSummaryCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getCMVAggregator();

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideMachineCMV == Arg.OverrideMachineCMV, "Invalid aggregator value for OverrideMachineCMV.");
      Assert.True(aggregator.OverridingMachineCMV == Arg.OverridingMachineCMV, "Invalid aggregator value for OverridingMachineCMV.");
    }

    [Fact]
    public void Test_CMVSummaryCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getCMVAggregator();
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.CCV, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_CMVSummaryCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getCMVAggregator();
      var coordinator = _getCoordinator();

      var response = new CMVSummaryResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");
      Assert.True(response.SummaryCellsScanned == aggregator.SummaryCellsScanned, "Invalid read-out value for SummaryCellsScanned.");
      Assert.True(response.LastTargetCMV == aggregator.LastTargetCMV, "Invalid read-out value for LastTargetCMV.");
      Assert.True(response.CellsScannedOverTarget == aggregator.CellsScannedOverTarget, "Invalid read-out value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == aggregator.CellsScannedAtTarget, "Invalid read-out value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == aggregator.CellsScannedUnderTarget, "Invalid read-out value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == aggregator.IsTargetValueConstant, "Invalid read-out value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == aggregator.MissingTargetValue, "Invalid initial read-out for MissingTargetValue.");
    }
  }
}
