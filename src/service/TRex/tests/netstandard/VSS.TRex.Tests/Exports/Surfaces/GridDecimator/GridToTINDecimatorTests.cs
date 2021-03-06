﻿using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces.GridDecimator;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
  public class DecimationElevationSubGridTree : GenericSubGridTree<float, GenericLeafSubGrid<float>>
  {
    public override float NullCellValue => TRex.Common.Consts.NullHeight;
  }
  
  public class GridToTINDecimatorTests : IClassFixture<DILoggingFixture>
  {
      [Fact]
      public void GridToTINDecimatorTests_Creation_NoDataStore()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_Creation_WithDataStore()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_Refresh()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());
        decimator.Refresh();

        Assert.NotNull(decimator);
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_NoDataSource()
      {
        Action act = () =>
        {
          var decimator = new GridToTINDecimator(null);
        };

        act.Should().Throw<TRexTINException>().WithMessage("No data store provided to decimator");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_NonEmptyDestinationTIN()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());
        decimator.GridCalcExtents = new BoundingIntegerExtent2D(0, 0, 100, 100);
        decimator.GetTIN().Triangles.AddTriangle(new TriVertex(0, 0, 0), new TriVertex(1, 1, 1), new TriVertex(0, 0, 1));

        decimator.BuildMesh().Should().BeFalse();
        decimator.BuildMeshFaultCode.Should().Be(DecimationResult.DestinationTINNotEmpty);
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_EmptyDataSource()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());
        decimator.BuildMesh().Should().BeFalse();
        decimator.BuildMeshFaultCode.Should().Be(DecimationResult.NoData);
        
        decimator.GetTIN().Triangles.Count.Should().Be(0);
        decimator.GetTIN().Vertices.Count.Should().Be(0);
        decimator.GetTIN().Edges.Count.Should().Be(0);
        decimator.GetTIN().StartPoints.Count.Should().Be(0);
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_SetDecimationExtents()
      {
        GridToTINDecimator decimator = new GridToTINDecimator(new DecimationElevationSubGridTree());
        decimator.SetDecimationExtents( new BoundingWorldExtent3D(0, 0, 100, 100));

        Assert.True(decimator.GridCalcExtents.MinX == 536870912);
        Assert.True(decimator.GridCalcExtents.MaxX == 536871206);
        Assert.True(decimator.GridCalcExtents.MinY == 536870912);
        Assert.True(decimator.GridCalcExtents.MaxY == 536871206);
      }

      private BoundingWorldExtent3D DataStoreExtents(DecimationElevationSubGridTree dataStore)
      {
        BoundingWorldExtent3D ComputedGridExtent = BoundingWorldExtent3D.Inverted();

        dataStore.ScanAllSubGrids(subGrid =>
        {
          SubGridUtilities.SubGridDimensionalIterator((x, y) =>
          {
            float elev = ((GenericLeafSubGrid<float>) subGrid).Items[x, y];
            if (elev != 0)
              ComputedGridExtent.Include((int)(subGrid.OriginX + x), (int)(subGrid.OriginY + y), elev);
            else
              ((GenericLeafSubGrid<float>)subGrid).Items[x, y] = TRex.Common.Consts.NullHeight;
          });

          return true;
        });

        if (ComputedGridExtent.IsValidPlanExtent)
          ComputedGridExtent.Offset(-(int)SubGridTreeConsts.DefaultIndexOriginOffset, -(int)SubGridTreeConsts.DefaultIndexOriginOffset);

        // Convert the grid rectangle to a world rectangle
        BoundingWorldExtent3D ComputedWorldExtent = new BoundingWorldExtent3D
         ((ComputedGridExtent.MinX - 0.01) * dataStore.CellSize,
          (ComputedGridExtent.MinY - 0.01) * dataStore.CellSize,
          (ComputedGridExtent.MaxX + 1 + 0.01) * dataStore.CellSize,
          (ComputedGridExtent.MaxY + 1 + 0.01) * dataStore.CellSize,
          ComputedGridExtent.MinZ, ComputedGridExtent.MaxZ);

      return ComputedWorldExtent;
      }

    [Fact]
      public void GridToTINDecimatorTests_BuildMesh_SinglePoint()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 100, SubGridTreeConsts.DefaultIndexOriginOffset + 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);

        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result && decimator.GetTIN().Triangles.Count == 0, $"Failed to fail to build mesh from data store with single point fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_TwoPoints()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 100, SubGridTreeConsts.DefaultIndexOriginOffset + 100] = 100.0f;
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 101, SubGridTreeConsts.DefaultIndexOriginOffset + 101] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result && decimator.GetTIN().Triangles.Count == 0, $"Failed to fail to build mesh from data store with two points fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_ThreePoints()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 100, SubGridTreeConsts.DefaultIndexOriginOffset + 100] = 100.0f;
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 101, SubGridTreeConsts.DefaultIndexOriginOffset + 101] = 100.0f;
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 101, SubGridTreeConsts.DefaultIndexOriginOffset + 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        Assert.True(result, $"Failed to build mesh from data store with three points fault code {decimator.BuildMeshFaultCode}");
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_ExceedPointLimit()
      {
        DecimationElevationSubGridTree dataStore = new DecimationElevationSubGridTree();
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 100, SubGridTreeConsts.DefaultIndexOriginOffset + 100] = 100.0f;
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 101, SubGridTreeConsts.DefaultIndexOriginOffset + 101] = 100.0f;
        dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + 101, SubGridTreeConsts.DefaultIndexOriginOffset + 100] = 100.0f;

        GridToTINDecimator decimator = new GridToTINDecimator(dataStore);
        decimator.PointLimit = 2;
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        decimator.BuildMesh().Should().BeFalse();
        decimator.BuildMeshFaultCode.Should().Be(DecimationResult.VerticesExceeded);
      }

      [Fact]
      public void GridToTINDecimatorTests_BuildMesh_GetTIN()
      {
        var dataStore = new DecimationElevationSubGridTree();

        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
            dataStore[SubGridTreeConsts.DefaultIndexOriginOffset + x, SubGridTreeConsts.DefaultIndexOriginOffset + y] = 100f;
        });

        var decimator = new GridToTINDecimator(dataStore);
        decimator.SetDecimationExtents(DataStoreExtents(dataStore));
        bool result = decimator.BuildMesh();

        result.Should().BeTrue($"Failed to build mesh from data store with a sub grid of points fault code {decimator.BuildMeshFaultCode}");

        decimator.GetTIN().Should().NotBeNull();

        //string fileName = $@"C:\temp\UnitTestExportTTM({DateTime.Now.Ticks}).ttm";
        //decimator.GetTIN().SaveToFile(fileName, true);  

        //TrimbleTINModel tin = new TrimbleTINModel();        
        //tin.LoadFromFile(fileName);

        decimator.GetTIN().Triangles.Count.Should().Be(3);
        decimator.GetTIN().Triangles[0].Vertices.ForEach(x => x.Z.Should().Be(100f));

        decimator.GetTIN().Vertices.Count.Should().Be(5);
        decimator.GetTIN().Vertices.ForEach(x => x.Z.Should().Be(100.0f));
    }
  }
}
