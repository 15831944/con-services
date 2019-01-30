﻿using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridUtilitiesTests
    {
        [Fact]
        public void Test_SubGridDimensionalIterator()
        {
            // Ensure the iterator covers all the cells in a subgrid
            int counter = 0;

            SubGridUtilities.SubGridDimensionalIterator((x, y) => counter++);
            Assert.Equal(SubGridTreeConsts.SubGridTreeCellsPerSubGrid, counter);
        }
    }
}
