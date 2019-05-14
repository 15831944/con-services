﻿using System;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridSpatialAffinityKeyTests
    {
        [Fact]
        public void Test_SubGridSpatialAffinityKey_NullConstructor()
        {
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey();
            Assert.True(key.ProjectUID == Guid.Empty && key.SubGridX == 0 && key.SubGridY == 0 && string.IsNullOrEmpty(key.SegmentIdentifier),
                "Default constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, ID, 12345678, 34567890);
            Assert.True(key.Version == 1 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginAndSegmentConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(2, ID, 12345678, 34567890, "123-456-890-012");
            Assert.True(key.Version == 2 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, ID, new SubGridCellAddress(12345678, 34567890));
            Assert.True(key.Version == 1 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressAndSegmentConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(2, ID, new SubGridCellAddress(12345678, 34567890), "123-456-890-012");
            Assert.True(key.Version == 2 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSubgrid()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, ID, new SubGridCellAddress(12345678, 34567890), string.Empty);
            Assert.Equal($"{ID}-12345678-34567890", key.ToString());
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSegment()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, ID, new SubGridCellAddress(12345678, 34567890), "123-456-890-012");
            Assert.Equal($"{ID}-12345678-34567890-123-456-890-012", key.ToString());
        }
    }
}
