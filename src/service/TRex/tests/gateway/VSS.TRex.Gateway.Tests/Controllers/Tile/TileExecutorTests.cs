﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Tile
{
  public class TileExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private void AddRoutings()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();
    }

    [Fact]
    public async Task TileExecutor_EmptySiteModel()
    {
      AddRoutings();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new TRexTileRequest
      (
        siteModel.ID,
        DisplayMode.Height,
        null, //List<ColorPalette> palettes,
        null, //new DesignDescriptor(0, FileDescriptor.EmptyFileDescriptor, 0),
        new FilterResult(),
        new FilterResult(),
        null, //new BoundingBox2DLatLon boundingBoxLatLon,
        new BoundingBox2DGrid(0, 0, 100, 100),
        256,
        256,
        null,
        null);

      request.Validate();

      var executor = RequestExecutorContainer
        .Build<TileExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await executor.ProcessAsync(request) as TileResult;

      result.Should().NotBeNull();
      result?.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result?.TileData.Should().NotBeNull();
    }


    /// <summary>
    /// Actually only checking for serialization and deserilization exceptions
    /// </summary>
    private async Task<bool> ExecuteTileRequest(TRexTileRequest request)
    {
      var executor = RequestExecutorContainer
        .Build<TileExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await executor.ProcessAsync(request) as TileResult;
      result?.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      return true; 
    }

    private TRexTileRequest MakeTileRequest(Guid sMId, DisplayMode dm)
    {
      return new TRexTileRequest
      (
        sMId,
        dm,
        null, 
        null, 
        new FilterResult(),
        new FilterResult(),
        null, 
        new BoundingBox2DGrid(0, 0, 100, 100),
        256,
        256,
        null,
        null);
    }

    [Fact]
    public async Task TileExecutor_TestPaletteSerilizationAndDeserilization()
    {
      AddRoutings();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = MakeTileRequest(siteModel.ID, DisplayMode.Height);
      request.Validate();
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.CCVPercentSummary);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.CMVChange);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.PassCountSummary);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.CCASummary);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.CCV);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.MDP);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.TargetSpeedSummary);
      Assert.True(ExecuteTileRequest(request).Result);
      request = MakeTileRequest(siteModel.ID, DisplayMode.TemperatureSummary);
      Assert.True(ExecuteTileRequest(request).Result);
    }

  }
}
