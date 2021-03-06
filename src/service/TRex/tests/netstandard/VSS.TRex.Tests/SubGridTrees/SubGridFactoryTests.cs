﻿using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees 
{
        public class SubGridFactoryTests : IClassFixture<DILoggingFixture>
    {
        [Fact]
        public void Test_SubGridFactory_Creation()
        {
            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, LeafSubGrid>();

            Assert.NotNull(factory);

            // Create a tree for the factory to create sub grids for
            ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, factory);

            Assert.NotNull(tree);
        }

        [Fact]
        public void Test_SubGridFactory_Create_NodeAndLeafSubGrids()
        {
            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, LeafSubGrid>();

            // Create a tree for the factory to create sub grids for
            ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, factory);

            // Create sub grids for each layer, ensure layers
            // Ask the factory to create node for an invalid tree level
            try
            {
                ISubGrid invalid = factory.GetSubGrid(tree, 0);
                Assert.True(false,"Factory created sub grid for invalid tree level.");
            }
            catch (ArgumentException)
            {
                // As expected
            }

            // Ask the factory to create node and leaf sub grids for a 6 level tree, from root to leaf.
            ISubGrid root = factory.GetSubGrid(tree, 1);
            Assert.NotNull(root);
            Assert.True(root is NodeSubGrid, "Factory did not create node sub grid for root tree level.");

            ISubGrid level2 = factory.GetSubGrid(tree, 2);
            Assert.NotNull(level2);
            Assert.True(level2 is NodeSubGrid, "Factory did not create node sub grid for tree level 2.");

            ISubGrid level3 = factory.GetSubGrid(tree, 3);
            Assert.NotNull(level3);
            Assert.True(level3 is NodeSubGrid, "Factory did not create node sub grid for tree level 3.");

            ISubGrid level4 = factory.GetSubGrid(tree, 4);
            Assert.NotNull(level4);
            Assert.True(level4 is NodeSubGrid, "Factory did not create node sub grid for tree level 4.");

            ISubGrid level5 = factory.GetSubGrid(tree, 5);
            Assert.NotNull(level5);
            Assert.True(level5 is NodeSubGrid, "Factory did not create node sub grid for tree level 5.");

            ISubGrid leaf = factory.GetSubGrid(tree, 6);
            Assert.NotNull(leaf);
            Assert.True(leaf is LeafSubGrid, "Factory did not create leaf sub grid for tree level 6.");
        }

        [Fact]
        public void Test_SubGridClientLeafFactory_Creation()
        {
            IClientLeafSubGridFactory factory = new ClientLeafSubGridFactory();

            Assert.NotNull(factory);

            IClientLeafSubGrid HeightLeaf = factory.GetSubGrid(Types.GridDataType.Height);

            Assert.NotNull(factory);

            IClientLeafSubGrid HeightAndTimeLeaf = factory.GetSubGrid(Types.GridDataType.HeightAndTime);

            Assert.NotNull(factory);
        }

        [Fact]
        public void Test_SubGridClientLeafFactory_Recycling()
        {
            IClientLeafSubGridFactory factory = new ClientLeafSubGridFactory();

            Assert.NotNull(factory);

            IClientLeafSubGrid HeightLeaf = factory.GetSubGrid(Types.GridDataType.Height);
            factory.ReturnClientSubGrid(ref HeightLeaf);

            Assert.Null(HeightLeaf);

            IClientLeafSubGrid HeightAndTimeLeaf = factory.GetSubGrid(Types.GridDataType.HeightAndTime);
            factory.ReturnClientSubGrid(ref HeightAndTimeLeaf);

            Assert.Null(HeightAndTimeLeaf);
        }

        [Fact]
        public void Test_SubGridClientLeafFactory_Reuse()
        {
            IClientLeafSubGridFactory factory = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory();

            Assert.NotNull(factory);

            IClientLeafSubGrid HeightLeaf = factory.GetSubGrid(Types.GridDataType.Height);
            factory.ReturnClientSubGrid(ref HeightLeaf);

            IClientLeafSubGrid HeightAndTimeLeaf = factory.GetSubGrid(Types.GridDataType.HeightAndTime);
            factory.ReturnClientSubGrid(ref HeightAndTimeLeaf);

            IClientLeafSubGrid HeightLeaf2 = factory.GetSubGrid(Types.GridDataType.Height);
            Assert.NotNull(HeightLeaf2);
            Assert.Equal(Types.GridDataType.Height, HeightLeaf2.GridDataType);

            IClientLeafSubGrid HeightAndTimeLeaf2 = factory.GetSubGrid(Types.GridDataType.HeightAndTime);
            Assert.NotNull(HeightAndTimeLeaf2);
            Assert.Equal(Types.GridDataType.HeightAndTime, HeightAndTimeLeaf2.GridDataType);
        }
    }
}
