﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Gateway.Tests
{
  public class AutoMapperTests : IClassFixture<AutoMapperFixture>
  {
    [Fact]
    public void MapPointToFencePoint()
    {
      var point = new Point
      {
        x = 10,
        y = 15
      };
      var fencePoint = AutoMapperUtility.Automapper.Map<FencePoint>(point);
      Assert.Equal(point.x, fencePoint.X);
      Assert.Equal(point.y, fencePoint.Y);
      Assert.Equal(0, fencePoint.Z);
    }

    [Fact]
    public void MapWGSPointToFencePoint()
    {
      var point = new WGSPoint(123.4, 567.8);
      var fencePoint = AutoMapperUtility.Automapper.Map<FencePoint>(point);
      Assert.Equal(point.Lon, fencePoint.X);
      Assert.Equal(point.Lat, fencePoint.Y);
      Assert.Equal(0, fencePoint.Z);
    }

    [Fact]
    public void MapBoundingBox2DGridToBoundingWorldExtent3D()
    {
      var box = new BoundingBox2DGrid(10, 12, 35, 27);  
      var box3d = AutoMapperUtility.Automapper.Map<BoundingWorldExtent3D>(box);
      Assert.Equal(box.BottomLeftX, box3d.MinX);
      Assert.Equal(box.BottomleftY, box3d.MinY);
      Assert.Equal(box.TopRightX, box3d.MaxX);
      Assert.Equal(box.TopRightY, box3d.MaxY);
    }

    [Fact]
    public void MapBoundingBox2DLatLonToBoundingWorldExtent3D()
    {
      var box = new BoundingBox2DLatLon(10, 12, 35, 27);
      var box3d = AutoMapperUtility.Automapper.Map<BoundingWorldExtent3D>(box);
      Assert.Equal(box.BottomLeftLon, box3d.MinX);
      Assert.Equal(box.BottomLeftLat, box3d.MinY);
      Assert.Equal(box.TopRightLon, box3d.MaxX);
      Assert.Equal(box.TopRightLat, box3d.MaxY);
    }

    [Fact]
    public void MapFilterResultWithPolygonToCombinedFilter()
    {
      List<WGSPoint> polygonLonLat = new List<WGSPoint>
      {
        new WGSPoint(1, 1),
        new WGSPoint(2, 2),
        new WGSPoint(3, 3)
      };
      var filter = new FilterResult(null, new Filter(), polygonLonLat, null, null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filter);
      Assert.NotNull(combinedFilter.AttributeFilter);
      Assert.Equal(filter.ReturnEarliest, combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass);
      Assert.True(combinedFilter.AttributeFilter.HasElevationTypeFilter);
      Assert.Equal(Types.ElevationType.First, combinedFilter.AttributeFilter.ElevationType);

      Assert.NotNull(combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList);
      Assert.Empty(combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList);

      Assert.NotNull(combinedFilter.SpatialFilter);
      Assert.False(combinedFilter.SpatialFilter.CoordsAreGrid);
      Assert.True(combinedFilter.SpatialFilter.IsSpatial);
      Assert.NotNull(combinedFilter.SpatialFilter.Fence);
      Assert.NotNull(combinedFilter.SpatialFilter.Fence.Points);
      Assert.Equal(polygonLonLat.Count, combinedFilter.SpatialFilter.Fence.Points.Count);
      for (int i =0; i<combinedFilter.SpatialFilter.Fence.Points.Count; i++)
      {
        Assert.Equal(filter.PolygonLL[i].Lon, combinedFilter.SpatialFilter.Fence.Points[i].X);
        Assert.Equal(filter.PolygonLL[i].Lat, combinedFilter.SpatialFilter.Fence.Points[i].Y);
      }
    }

    [Fact]
    public void MapFilterResultNoPolygonToCombinedFilter()
    {
      var filter = new FilterResult(null, new Filter(), null, null, null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filter);
      Assert.NotNull(combinedFilter.AttributeFilter);
      Assert.Equal(filter.ReturnEarliest, combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass);
      Assert.True(combinedFilter.AttributeFilter.HasElevationTypeFilter);
      Assert.Equal(Types.ElevationType.First, combinedFilter.AttributeFilter.ElevationType);

      Assert.NotNull(combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList);
      Assert.Empty(combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList);

      Assert.NotNull(combinedFilter.SpatialFilter);
      Assert.False(combinedFilter.SpatialFilter.CoordsAreGrid);
      Assert.False(combinedFilter.SpatialFilter.IsSpatial);
      Assert.Null(combinedFilter.SpatialFilter.Fence);
    }

    [Fact]
    public void MapMachineToMachineStatus()
    {
      var machineUid1 = Guid.NewGuid();
      var machineUid2 = Guid.NewGuid();
      var machineUid3 = Guid.Empty;
      var machines = new MachinesList { DataModelID = Guid.NewGuid() };
      machines.CreateNew("MachineName1", "hardwareID444", MachineType.ConcretePaver, (int) DeviceTypeEnum.SNM940, false, machineUid1);
      machines[0].InternalSiteModelMachineIndex = 0;
      machines[0].LastKnownX = 34.34;
      machines[0].LastKnownY = 77.77;
      machines[0].LastKnownPositionTimeStamp = DateTime.UtcNow.AddMonths(-2);
      machines[0].LastKnownDesignName = "design1";
      machines[0].LastKnownLayerId = 11;

      machines.CreateNew("MachineName2", "hardwareID555", MachineType.AsphaltCompactor, (int) DeviceTypeEnum.EC520, false, machineUid2);
      machines.CreateNew("MachineName3", "hardwareID666", MachineType.Generic, (int)DeviceTypeEnum.MANUALDEVICE, true, machineUid3);

      var machineStatuses = machines.Select(machine =>
          AutoMapperUtility.Automapper.Map<MachineStatus>(machine)).ToArray();
      machineStatuses.Length.Equals(3);
      machineStatuses[0].AssetUid.HasValue.Equals(true);
      machineStatuses[0].AssetUid?.Equals(machines[0].ID);
      machineStatuses[0].AssetId.Equals(-1);
      machineStatuses[0].MachineName.IsNullOrEmpty().Equals(false);
      machineStatuses[0].MachineName.Equals(machines[0].Name);
      machineStatuses[0].IsJohnDoe.Equals(machines[0].IsJohnDoeMachine);
      machineStatuses[0].lastKnownDesignName.IsNullOrEmpty().Equals(false);
      machineStatuses[0].lastKnownDesignName.Equals(machines[0].LastKnownDesignName);
      machineStatuses[0].lastKnownLayerId.HasValue.Equals(true);
      machineStatuses[0].lastKnownLayerId?.Equals(machines[0].LastKnownLayerId);
      machineStatuses[0].lastKnownTimeStamp.HasValue.Equals(true);
      machineStatuses[0].lastKnownTimeStamp?.Equals(machines[0].LastKnownPositionTimeStamp);
      machineStatuses[0].lastKnownLatitude.HasValue.Equals(true);
      machineStatuses[0].lastKnownLatitude?.Equals(Double.MaxValue); 
      machineStatuses[0].lastKnownLongitude.HasValue.Equals(true);
      machineStatuses[0].lastKnownLongitude?.Equals(Double.MaxValue); 
      machineStatuses[0].lastKnownX.HasValue.Equals(true);
      machineStatuses[0].lastKnownX?.Equals(machines[0].LastKnownX);
      machineStatuses[0].lastKnownY.HasValue.Equals(true);
      machineStatuses[0].lastKnownY?.Equals(machines[0].LastKnownY);

      machineStatuses[1].AssetUid.HasValue.Equals(true);
      machineStatuses[1].AssetUid?.Equals(machineUid2);
      machineStatuses[1].lastKnownX.HasValue.Equals(false);
      machineStatuses[1].lastKnownY.HasValue.Equals(false);

      machineStatuses[2].AssetUid.HasValue.Equals(true);
      machineStatuses[2].AssetUid?.Equals(machineUid3);
    }
  }
}
