﻿using System;
using FluentAssertions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class LeafSubgridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_LeafSubgrid_Creation()
    {
      ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      ILeafSubGrid leaf = null;

      // Test creation of a leaf node without an owner tree
      try
      {
        leaf = new LeafSubGrid(null, null, (byte) (tree.NumLevels + 1));
        Assert.True(false, "Was able to create a leaf subgrid with no owning tree");
      }
      catch (Exception)
      {
        // As expected
      }

      // Test creation of a leaf node at an inappropriate level
      try
      {
        leaf = new LeafSubGrid(tree, null, (byte) (tree.NumLevels + 1));
        Assert.True(false, "Was able to create a leaf subgrid at an inappropriate level");
      }
      catch (Exception)
      {
        // As expected
      }

      leaf = new LeafSubGrid(tree, null, tree.NumLevels);

      Assert.True(leaf != null && leaf.Level == tree.NumLevels);
    }

    [Fact]
    public void Test_LeafSubgrid_IsEmpty()
    {
      ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      ILeafSubGrid leaf = new LeafSubGrid(tree, null, tree.NumLevels);

      leaf.IsEmpty().Should().BeTrue();
    }
  }
}
