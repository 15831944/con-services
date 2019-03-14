﻿using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class GenericClientLeafSubgridTests
  {
    static int GetGridDataTypeCount() => Enum.GetValues(typeof(GridDataType)).Length;

    static readonly int GridDataTypeCount = GetGridDataTypeCount();

    private const int kGridDataTypeCount_Expected = 15;
    private const int kGridDataTypeCount = 29;

    /// <summary>
    /// Which grid data types have implementations that should be tested?
    /// </summary>
    /// <param name="gridDataType"></param>
    /// <returns></returns>
    private static bool Include(GridDataType gridDataType)
    {
      return gridDataType == GridDataType.Height ||
             gridDataType == GridDataType.HeightAndTime ||
             gridDataType == GridDataType.CompositeHeights ||
             gridDataType == GridDataType.CCV ||
             gridDataType == GridDataType.CCVPercentChange ||
             gridDataType == GridDataType.MDP ||
             gridDataType == GridDataType.MachineSpeed ||
             gridDataType == GridDataType.MachineSpeedTarget ||
             gridDataType == GridDataType.Temperature ||
             gridDataType == GridDataType.TemperatureDetail ||
             gridDataType == GridDataType.PassCount ||
             gridDataType == GridDataType.CellProfile ||
             gridDataType == GridDataType.CellPasses ||
             gridDataType == GridDataType.CCA ||
             gridDataType == GridDataType.CutFill;
    }

    /// <summary>
    /// Provides the list of grid data types the tests should apply to
    /// </summary>
    /// <param name="numTests"></param>
    /// <returns></returns>
    public static IEnumerable<object[]> ClientLeafDataTypes(int numTests)
    {
      var allData = (Enum.GetValues(typeof(GridDataType)) as int[]).Select(x => new object[] {(GridDataType) x, Include((GridDataType)x) }).ToList();

      return allData.Take(numTests);
    }

    /// <summary>
    /// Selects only the grid data types with expected == true from ClientLeafDataTypes
    /// </summary>
    /// <param name="numTests"></param>
    /// <returns></returns>
    public static IEnumerable<object[]> ClientLeafDataTypes_ExpectedOnly(int numTests)
    {
      var allData = (Enum.GetValues(typeof(GridDataType)) as int[]).Where(x => Include((GridDataType)x)).Select(x => new object[] { (GridDataType)x }).ToList();

      return allData.Take(numTests);
    }

    /// <summary>
    /// Fail if a new grid data type has been added to ensure tests are created for it
    /// </summary>
    [Fact]
    public void Test_GenericClientLeafSubgrid_EnsureExpectedGridDataTypes()
    {
      Assert.True(kGridDataTypeCount == GridDataTypeCount, $"{GridDataTypeCount} grid data types found, but {kGridDataTypeCount} were expected");
    }

    /// <summary>
    /// Fail if a new grid data type has been added to ensure tests are created for it
    /// </summary>
    [Fact]
    public void Test_GenericClientLeafSubgrid_EnsureThereAreNumberOfExpectedGridDataTypes()
    {
      Assert.True(kGridDataTypeCount_Expected == ClientLeafDataTypes_ExpectedOnly(100).Count(), $"Number of expected grid data types is not {kGridDataTypeCount_Expected} as expected");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes), parameters: kGridDataTypeCount)]
    public void Test_GenericClientLeafSubgrid_Creation_EX(GridDataType gridDataType, bool expected)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      if (expected)
        Assert.NotNull(clientGrid);
      else
        Assert.Null(clientGrid);
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_ForEach_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      int Count = 0;

      clientGrid.ForEach((x, y) => Count++);
      Assert.True(SubGridTreeConsts.SubGridTreeCellsPerSubGrid == Count, "ForEach did not iterate all cells");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Clear_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.FillWithTestPattern();
      clientGrid.Clear();
      clientGrid.ForEach((x, y) =>
      {
        if (gridDataType != GridDataType.CCVPercentChange)
          Assert.True(!clientGrid.CellHasValue(x, y), "Clear() did not clear all cells");
      });
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_ReadWrite_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.FillWithTestPattern();
      byte[] bytes = clientGrid.ToBytes();
      Assert.True(bytes.Length > 0);

      var clientGrid2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid2.FromBytes(bytes);

      Assert.True(clientGrid.LeafContentEquals(clientGrid2), "Client grids not equal after read/write serialisation");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_CellHasValue_True_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.FillWithTestPattern();

      clientGrid.ForEach((x, y) => Assert.True(clientGrid.CellHasValue(x, y), "Cell does not have value when it should"));
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_CellHasValue_False_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.ForEach((x, y) =>
      {
        if (gridDataType != GridDataType.CCVPercentChange)
          Assert.False(clientGrid.CellHasValue(x, y), "Cell does have value when it should not");
        else
          Assert.True(clientGrid.CellHasValue(x, y), "Cell does not have value when it should");
      });
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_IndicativeSizeInBytes(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      Assert.True(clientGrid.IndicativeSizeInBytes() > 0, "Indicative size in bytes is <= 0!");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_AssignFromCachedPreProcessedClientSubgrid(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.AssignFromCachedPreProcessedClientSubgrid(clientGrid, clientGrid.FilterMap);

      // If we get here it's all good!
      Assert.True(true, "");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_AssignFromCachedPreProcessedClientSubgrid2(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.AssignFromCachedPreProcessedClientSubgrid(clientGrid);

      // If we get here it's all good!
      Assert.True(true, "");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_DumpToLog(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.DumpToLog();

      // If we get here it's all good!
      Assert.True(true, "");
    }
  }
}
