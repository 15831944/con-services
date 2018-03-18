﻿using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators.Tests
{
        public class SubGridSegmentIteratorTests
    {
        [Fact()]
        public void Test_SubGridSegmentIterator_SubGridSegmentIterator()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            Assert.True(iterator.Directory == leaf.Directory &&
                iterator.SubGrid == leaf, "SubGrid segment iterator not correctly initialised");
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_SetTimeRange()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            DateTime start = new DateTime(2000, 1, 1, 1, 1, 1);
            DateTime end = new DateTime(2000, 1, 2, 1, 1, 1);
            iterator.SetTimeRange(start, end);

            Assert.True(iterator.IterationState.StartSegmentTime == start && iterator.IterationState.EndSegmentTime == end,
                "Start and end time not set correctly");
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_MoveNext()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_MoveToFirstSubGridSegment()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_MoveToNextSubGridSegment()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_CurrentSubgridSegmentDestroyed()
        {
            Assert.Inconclusive();
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_InitialiseIterator()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            iterator.IterationDirection = IterationDirection.Forwards;
            iterator.InitialiseIterator();
            Assert.Equal(-1, iterator.IterationState.Idx);

            iterator.IterationDirection = IterationDirection.Backwards;
            iterator.InitialiseIterator();

            Assert.Equal(iterator.IterationState.Idx, leaf.Directory.SegmentDirectory.Count());
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_SegmentListExtended()
        {
            Assert.Inconclusive();
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_MarkCacheStamp()
        {
            Assert.Inconclusive();
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_SetIteratorElevationRange()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            double lowerElevation = 9.0;
            double upperElevation = 19.0;

            iterator.SetIteratorElevationRange(lowerElevation, upperElevation);

            Assert.True(iterator.IterationState.MinIterationElevation == lowerElevation && iterator.IterationState.MaxIterationElevation == upperElevation,
                "Elevation lower and upper bounds not set correctly");
        }
    }
}