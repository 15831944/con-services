﻿using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSubGridTests
  /// </summary>
  public class MDPClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    private static readonly ILogger _log = VSS.TRex.Logging.Logger.CreateLogger<MDPClientLeafSubGridTests>();

    [Fact]
    public void Test_NullCells()
    {
      var cell = new SubGridCellPassDataMDPEntryRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MDP) as ClientMDPLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MDP) as ClientMDPLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }


    [Fact]
    public void DumpToLog()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MDP) as ClientMDPLeafSubGrid;
      clientGrid.DumpToLog(_log, clientGrid.ToString());
    }

  }
}
