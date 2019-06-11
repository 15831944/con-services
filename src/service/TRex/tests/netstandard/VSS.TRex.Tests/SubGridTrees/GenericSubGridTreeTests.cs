﻿using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class GenericSubGridTreeTests : IClassFixture<DILoggingFixture>
  {
        [Fact]
        public void Test_GenericSubGridTree_Creation()
        {
           var tree1 = new GenericSubGridTree_Bool(SubGridTreeConsts.SubGridTreeLevels, 1.0);

            Assert.True(tree1 != null && tree1.NumLevels == SubGridTreeConsts.SubGridTreeLevels && tree1.CellSize == 1.0,
                "Generic sub grid tree not created as expected with 3 arg constructor");

            var tree2 = new GenericSubGridTree_Bool(SubGridTreeConsts.SubGridTreeLevels, 1.0);

            Assert.True(tree2 != null && tree2.NumLevels == SubGridTreeConsts.SubGridTreeLevels && tree2.CellSize == 1.0,
                "Generic sub grid tree not created as expected with  arg constructor");
        }

        [Fact]
        public void Test_GenericSubGridTree_GetCell()
        {
            var tree = new GenericSubGridTree_Bool(SubGridTreeConsts.SubGridTreeLevels, 1.0);

            Assert.False(tree[0, 0]);
            tree[0, 0] = true;
            Assert.True(tree[0, 0]);
        }

        [Fact]
        public void Test_GenericSubGridTree_SetCell()
        {
            var tree = new GenericSubGridTree_Bool(SubGridTreeConsts.SubGridTreeLevels, 1.0);

            tree[0, 0] = true;
            Assert.True(tree[0, 0]);
        }

        [Fact]
        public void Test_GenericSubGridTree_NullCellValue_Bool()
        {
            var tree1 = new GenericSubGridTree_Bool(SubGridTreeConsts.SubGridTreeLevels, 1.0);
            Assert.False(tree1.NullCellValue);

            var tree2 = new GenericSubGridTree<long, GenericLeafSubGrid<long>> (SubGridTreeConsts.SubGridTreeLevels, 1.0);
            Assert.Equal(0, tree2.NullCellValue);

            var tree3 = new GenericSubGridTree<object, GenericLeafSubGrid<object>>(SubGridTreeConsts.SubGridTreeLevels, 1.0);
            Assert.Null(tree3.NullCellValue);
        }

    [Fact]
    public void Test_GenericSubGridTree_NullCellValue_Int()
    {
      var tree1 = new GenericSubGridTree_Int(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree1.NullCellValue);

      var tree2 = new GenericSubGridTree<int, GenericLeafSubGrid<int>>(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree2.NullCellValue);
    }

    [Fact]
    public void Test_GenericSubGridTree_NullCellValue_long()
    {
      var tree1 = new GenericSubGridTree_Long(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree1.NullCellValue);

      var tree2 = new GenericSubGridTree<long, GenericLeafSubGrid<long>>(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree2.NullCellValue);
    }

    [Fact]
    public void Test_GenericSubGridTree_NullCellValue_Float()
    {
      var tree1 = new GenericSubGridTree_Float(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree1.NullCellValue);

      var tree2 = new GenericSubGridTree<float, GenericLeafSubGrid<float>>(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree2.NullCellValue);
    }

    [Fact]
    public void Test_GenericSubGridTree_NullCellValue_Double()
    {
      var tree1 = new GenericSubGridTree_Float(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree1.NullCellValue);

      var tree2 = new GenericSubGridTree<double, GenericLeafSubGrid<double>>(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Equal(0, tree2.NullCellValue);
    }

    [Fact]
    public void Test_GenericSubGridTree_NullCellValue_Object()
    {
      var tree3 = new GenericSubGridTree<object, GenericLeafSubGrid<object>>(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      Assert.Null(tree3.NullCellValue);
    }

    [Fact]
    public void Test_GenericSubGridTree_ForEach()
    {
      var tree = new GenericSubGridTree_Bool(SubGridTreeConsts.SubGridTreeLevels, 1.0);
      int count;

      // Count 'true' cells (should be none yet)
      count = 0;
      tree.ForEach(x =>
      {
        count = x ? count++ : count;
        return true;
      });
      Assert.Equal(0, count);

      // Add some true cells to the tree and count them
      for (int x = 0; x < 10; x++)
      {
        for (int y = 0; y < 10; y++)
        {
          tree[x * 10, y * 10] = true;
        }
      }

      count = 0;
      tree.ForEach(x =>
      {
        count = x ? count + 1 : count;
        return true;
      });
      Assert.Equal(100, count);
    }
  }
}
