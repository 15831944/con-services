﻿using System;
using FluentAssertions;
using Moq;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Core;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridTests : IClassFixture<DILoggingAndStorageProxyFixture>
  {
        [Fact]
        public void Test_SubGrid_Creation()
        {
            ISubGrid subgrid = null;

            // Try creating a new base subgrid instance directly, supplying 
            subgrid = new SubGrid(new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>()), null, SubGridTreeConsts.SubGridTreeLevels);
            Assert.NotNull(subgrid);
        }

        [Fact]
        public void Test_SubGrid_LeafSubgridProperties()
        {
            ISubGrid leafSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            // Create a new base subgrid leaf instance directly
            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            Assert.True(leafSubgrid.IsLeafSubGrid());

            Assert.False(leafSubgrid.Dirty);
            Assert.Equal(leafSubgrid.Level, SubGridTreeConsts.SubGridTreeLevels);
            Assert.Equal(leafSubgrid.AxialCellCoverageByThisSubGrid(), SubGridTreeConsts.SubGridTreeDimension);

            Assert.Equal(0, leafSubgrid.OriginX);
            Assert.Equal(0, leafSubgrid.OriginY);
            Assert.Equal("0:0", leafSubgrid.Moniker());

            // Does the dirty flag change?
            leafSubgrid.SetDirty();
            Assert.True(leafSubgrid.Dirty, "Leaf sub grid is not marked as dirty after setting it to dirty");
        }

        [Fact]
        public void Test_SubGrid_NodeSubgridProperties()
        {
            ISubGrid nodeSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            // Create a new base subgrid node instance directly
            nodeSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
            Assert.False(nodeSubgrid.IsLeafSubGrid());

            Assert.False(nodeSubgrid.Dirty);
            Assert.Equal(nodeSubgrid.Level, SubGridTreeConsts.SubGridTreeLevels - 1);

            // A subgrid one level above a leaf subgrid covers sqr(SubGridTreeConsts.SubGridTreeDimension) cells in each dimension (X & Y)
            Assert.Equal((int)nodeSubgrid.AxialCellCoverageByThisSubGrid(), SubGridTreeConsts.SubGridTreeCellsPerSubGrid);

            // A child subgrid of this parent should ahve an axial coverage of SubGridTreeConsts.SubGridTreeDimension cells in each dimension (X & Y)
            // (as there are SubGridTreeConsts.SubGridTreeDimension children cells in the X and Y dimensions
            Assert.Equal(nodeSubgrid.AxialCellCoverageByChildSubGrid(), SubGridTreeConsts.SubGridTreeDimension);
        }

        [Fact]
        public void Test_SubGrid_ParentAssignment()
        {
            ISubGrid parentSubgrid = null;
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);
            parentSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

            leafSubgrid.Parent = parentSubgrid;
            leafSubgrid.SetOriginPosition(10, 10);

            Assert.Equal((int)leafSubgrid.OriginX, 10 * SubGridTreeConsts.SubGridTreeDimension);
            Assert.Equal((int)leafSubgrid.OriginY, 10 * SubGridTreeConsts.SubGridTreeDimension);
            Assert.Equal(leafSubgrid.Moniker(), string.Format("{0}:{0}", 10 * SubGridTreeConsts.SubGridTreeDimension));
        }

        [Fact]
        public void Test_SubGrid_SetOriginPosition_FailWithNoParent()
        {
          var leafSubgrid = new SubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels);
 
          Action act = () => leafSubgrid.SetOriginPosition(10, 10);

          act.Should().Throw<ArgumentException>().WithMessage("Cannot set origin position without parent");
        }

        [Fact]
        public void Test_SubGrid_SetOriginPosition_FailWithInvalidCellCoordinateInSubgrid()
        {
          ISubGrid parentSubgrid = null;
          ISubGrid leafSubgrid = null;

          SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

          leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);
          parentSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

          leafSubgrid.Parent = parentSubgrid;
          leafSubgrid.SetOriginPosition(10, 10);

          Action act = () => leafSubgrid.SetOriginPosition(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension);

          act.Should().Throw<ArgumentException>().WithMessage("Cell X, Y location is not in the valid cell address range for the sub grid");
        }

    [Fact]
        public void Test_SubGrid_Invalid_GetSubgrid()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // Check a call to the base GetSubGrid subgrid yields an exception
            try
            {
                ISubGrid gotSubgrid = subgrid.GetSubGrid(0, 0);
                Assert.True(false,"Base SubGrid class GetSubGrid() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_Invalid_SetSubgrid()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // Check a call to the base SetSubGrid subgrid yields an exception
            try
            {
                subgrid.SetSubGrid(0, 0, null);
                Assert.True(false,"Base SubGrid class SetSubGrid() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_Invalid_Clear()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // Check a call to the base SetSubGrid subgrid yields an exception
            try
            {
                subgrid.Clear();
                Assert.True(false,"Base SubGrid class Clear() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_Invalid_CellHasValue()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // Check a call to the base SetSubGrid subgrid yields an exception
            try
            {
                if (subgrid.CellHasValue(0, 0))
                { }

                Assert.True(false,"Base SubGrid class CellHasValue() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_GetWorldOrigin()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            double WorldOriginX, WorldOriginY;
            leafSubgrid.CalculateWorldOrigin(out WorldOriginX, out WorldOriginY);

            // World origin of leaf subgrid is the extreme origin of the overmapped world coordinate system (cell coordinate system * cell size)
            // as the cell origin position is 0, 0 in the cell address space for a newly created subgrid
            // The leaf So, both X and Y origin values 
            Assert.Equal(WorldOriginX, WorldOriginY);
            Assert.Equal(WorldOriginX, (-tree.IndexOriginOffset * tree.CellSize));
        }

        [Fact]
        public void Test_SubGrid_GetSubGridCellIdex()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // GetSubGridCellIndex is a subgrid relative operation only, and depends only on the Owner to derive the difference
            // between the numer of levels in the overall tree, and the level in the tree at which this subgrid resides (in this
            // case the bottom of the tree (level 6) to compute the subgrid relative X and y cell indices as it is a leaf subgrid.

            byte SubGridCellX, SubGridCellY;
            leafSubgrid.GetSubGridCellIndex(0, 0, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");

            leafSubgrid.GetSubGridCellIndex(SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == (SubGridTreeConsts.SubGridTreeDimensionMinus1) && SubGridCellY == (SubGridTreeConsts.SubGridTreeDimensionMinus1), "Subgrid cell indices incorrect");

            leafSubgrid.GetSubGridCellIndex(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");
        }  

        [Fact]
        public void Test_SubGrid_AllChangesMigrated()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            Assert.False(leafSubgrid.Dirty, "Leaf is Dirty after creation");
            leafSubgrid.SetDirty();
            Assert.True(leafSubgrid.Dirty, "Leaf is not Dirty after setting Dirty to true");
            leafSubgrid.AllChangesMigrated();
            Assert.False(leafSubgrid.Dirty, "Leaf is Dirty after AllChangesMigrated");
        }

        [Fact]
        public void Test_SubGrid_IsEmpty()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            Assert.False(leafSubgrid.IsEmpty(), "Base subgrid class identifying itself as empty");
        }

        [Fact]
        public void Test_SubGrid_RemoveFromParent_Null()
        {
            // This can't be tested fully as the entire Set/Get subgrid functionality is abstract at this point, and
            // RemoveFromParent is part of that abstract workflow. At this level, we will test that no exception occurs
            // if the parent relationship is null

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            var leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            leafSubgrid.RemoveFromParent();
            leafSubgrid.Parent.Should().BeNull();
            // Good!
        }

        [Fact]
        public void Test_SubGrid_RemoveFromParent()
        {
          // This can't be tested fully as the entire Set/Get subgrid functionality is abstract at this point, and
          // RemoveFromParent is part of that abstract workflow. At this level, we will test that no exception occurs
          // if the parent relationship is null

          SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

          var parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
          var leafSubgrid = new LeafSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

          parentSubgrid.SetSubGrid(0, 0, leafSubgrid);

          leafSubgrid.RemoveFromParent();
          leafSubgrid.Parent.Should().BeNull();
        }

        [Fact]
        public void Test_SubGrid_ContainsOTGCell()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            // Create a leaf subgrid with it's cell origin (IndexOriginOffset, IndexOriginOffset) 
            // matching the real work coordaintge origin (0, 0)
            leafSubgrid = tree.ConstructPathToCell(tree.IndexOriginOffset, tree.IndexOriginOffset, SubGridPathConstructionType.CreateLeaf);

            Assert.NotNull(leafSubgrid);
            Assert.True(leafSubgrid.OriginX == tree.IndexOriginOffset && leafSubgrid.OriginX == tree.IndexOriginOffset,
                "Failed to create leaf node at the expected location");

            // Check that a 1m x 1m square (the size of the cells in the subgridtree created above) registers as being
            // a part of the newly created subgrid. First, get the cell enclosing that worl location and then ask
            // the subgrid if it contains it

            int CellX, CellY;

            Assert.True(tree.CalculateIndexOfCellContainingPosition(0.5, 0.5, out CellX, out CellY),
                          "Failed to get cell index for (0.5, 0.5)");
            Assert.True(leafSubgrid.ContainsOTGCell(CellX, CellY),
                         "Leaf subgrid denies enclosing the OTG cell at (0.5, 0.5)");
        }

        [Fact]
        public void Test_SubGrid_SetAbsoluteOriginPosition()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, 2); // create a node to be a chile of the root node

            // Test setting origin for unattached subgrid
            subgrid.SetAbsoluteOriginPosition(100, 100);

            Assert.True(subgrid.OriginX == 100 && subgrid.OriginY == 100,
                          "SetAbsoluteOriginPosition did not set origin position for subgrid");

            // Add subgrid to the root (which will set it's parent and prevent the origin position from 
            // being changed and will throw an exception)
            tree.Root.SetSubGrid(0, 0, subgrid);
            try
            {
                subgrid.SetAbsoluteOriginPosition(100, 100);

                Assert.True(false,"Setting absolute position for node with a parent did not raise an exception");
            } catch (Exception)
            {
                // As expected`
            }
        }

        [Fact]
        public void Test_SubGrid_SetAbsoluteLevel()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, 2); // create a node to be a chile of the root node

            // Test setting level for unattached subgrid (even though we set it in the constructor above
            subgrid.SetAbsoluteLevel(3);

            Assert.Equal(3, subgrid.Level);

            // Add subgrid to the root (which will set it's parent and prevent the level from 
            // being changed and will throw an exception)
            try
            {
                tree.Root.SetSubGrid(0, 0, subgrid);
                Assert.True(false,"Calling SetSubGrid with an invalid/non-null level did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }

            // Restore Level to the correct value of 2, then assign it into the root subgrid
            subgrid.SetAbsoluteLevel(2);
            tree.Root.SetSubGrid(0, 0, subgrid);

            // Now test the level cannot be changed with root as its parent
            try
            {
                subgrid.SetAbsoluteLevel(2);
                Assert.True(false,"Setting absolute level for node with a parent did not raise an exception");
            }
            catch (Exception)
            {
                // As expected`
            }
        }

        [Fact]
        public void Test_SubGrid_SetAbsoluteLevel_FailWithNonNullParent()
        {
          SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

          var child = new NodeSubGrid(tree, null, 3); // create a node to be a child of the root node
          var parent = new NodeSubGrid(tree, null, 2); // create a node to be a child of the root node
                                                   // Test setting level for unattached subgrid (even though we set it in the constructor above
          parent.SetSubGrid(0, 0, child);

          Action act = () => child.SetAbsoluteLevel(3);

          act.Should().Throw<TRexSubGridTreeException>().WithMessage("Nodes referencing parent nodes may not have their level modified");
        }

        [Fact]
        public void Test_SubGrid_ToString()
        {
          SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

          var child = new NodeSubGrid(tree, null, 3); // create a node to be a child of the root node
          var parent = new NodeSubGrid(tree, null, 2); // create a node to be a child of the root node

          parent.SetSubGrid(0, 0, child);

          child.ToString().Should().Be($"Level:{3}, OriginX:{child.OriginX}, OriginY:{child.OriginY}");
        }

        [Fact]
        public void Test_SubGrid_GetOTGLeafSubGridCellIndex()
        {
             SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
          
             var subGrid = new SubGrid(tree, null, 6); 
          
             subGrid.GetOTGLeafSubGridCellIndex(0, 0, out byte subGridX, out byte subGridY);
             subGridX.Should().Be(0);
             subGridY.Should().Be(0);
          
             subGrid.GetOTGLeafSubGridCellIndex(SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1, out subGridX, out subGridY);
             subGridX.Should().Be(SubGridTreeConsts.SubGridTreeDimensionMinus1);
             subGridY.Should().Be(SubGridTreeConsts.SubGridTreeDimensionMinus1);
          
             subGrid.GetOTGLeafSubGridCellIndex(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, out subGridX, out subGridY);
             subGridX.Should().Be(0);
             subGridY.Should().Be(0);
        }

        [Fact]
        public void Test_ConstructPathToCell_WithThreadContention()
        {
          var mockSubGridFactory = new Mock<SubGridFactory<NodeSubGrid, LeafSubGrid>>();
          mockSubGridFactory.Setup(x => x.GetSubGrid(It.IsAny<ISubGridTree>(), It.IsAny<byte>()))
            .Returns((ISubGridTree tree, byte level) =>
            {
              if (level < tree.NumLevels)
              {
                var mockSubGrid = new Mock<NodeSubGrid>();
                mockSubGrid.Object.Owner = tree;
                mockSubGrid.Object.Level = level;

                int numCalls = 0;
                mockSubGrid
                  .Setup(x => x.GetSubGridContainingCell(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns((int cellX, int cellY) =>
                  {
                    numCalls++;
                    if (numCalls == 1) return null;
                    if (numCalls >= 2) return new NodeSubGrid()
                    {
                      Owner = tree,
                      Level = (byte)(level + 1) // Need to create the sub grid at the lower level
                    }; ;

                    return null; // Should never get here
                  });

                return mockSubGrid.Object;
              }

              // We are only creating the path to the leaf, so should not be asked to create a leaf sub grid
              return null; 
            });

          // This test creates a tree then simulates thread contention in the ConstructPathToCell to test handling of this scenario
          var _tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, mockSubGridFactory.Object);         
          var subGrid = _tree.ConstructPathToCell(0, 0, SubGridPathConstructionType.CreatePathToLeaf);
          subGrid.Should().NotBeNull();
        }
    }
}
