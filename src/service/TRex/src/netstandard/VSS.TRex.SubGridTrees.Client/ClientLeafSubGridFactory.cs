﻿using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// Factory responsible for creating concrete 'grid data' specific client sub grid leaf instances
  /// </summary>
  public class ClientLeafSubGridFactory : IClientLeafSubGridFactory
  {
    /// <summary>
    /// Simple array to hold client leaf sub grid type constructor map
    /// </summary>
    private readonly Func<IClientLeafSubGrid>[] _typeMap = new Func<IClientLeafSubGrid>[Enum.GetNames(typeof(GridDataType)).Length];

    /// <summary>
    /// Stores of cached client grids to reduce the object instantiation and garbage collection overhead
    /// </summary>
    // private readonly SimpleConcurrentBag<IClientLeafSubGrid>[] ClientLeaves = Enumerable.Range(0, Enum.GetNames(typeof(GridDataType)).Length).Select(x => new SimpleConcurrentBag<IClientLeafSubGrid>()).ToArray();

    public ClientLeafSubGridFactory()
    {
    }

    /// <summary>
    /// Register a type implementing IClientLeafSubGrid against a grid data type for the factory to 
    /// create on demand
    /// </summary>
    public void RegisterClientLeafSubGridType(GridDataType gridDataType, Func<IClientLeafSubGrid> constructor)
    {
      if ((int) gridDataType > _typeMap.Length)
        throw new ArgumentException("Unknown grid data type in RegisterClientLeafSubGridType", nameof(gridDataType));

      _typeMap[(int) gridDataType] = constructor;
    }

    /// <summary>
    /// Construct a concrete instance of a sub grid implementing the IClientLeafSubGrid interface based
    /// on the role it should play according to the grid data type requested. All aspects of leaf ownership
    /// by a sub grid tree, parentage, level, cell size, index origin offset are delegated responsibilities
    /// of the caller or a derived factory class
    /// </summary>
    /// <returns>An appropriate instance derived from ClientLeafSubGrid</returns>
    public IClientLeafSubGrid GetSubGrid(GridDataType gridDataType)
    {
      IClientLeafSubGrid result = null;

      //* TODO: Don't use repatriated sub grids for now...
      //    if (!ClientLeaves[(int) gridDataType].TryTake(out IClientLeafSubGrid result))
      //    {
            if (_typeMap[(int) gridDataType] != null)
              result = _typeMap[(int) gridDataType]();
      //    }

      result?.Clear();
      return result;
    }

    /// <summary>
    /// Construct a concrete instance of a sub grid implementing the IClientLeafSubGrid interface based
    /// on the role it should play according to the grid data type requested. All aspects of leaf ownership
    /// by a sub grid tree, parentage, level, cell size, index origin offset are delegated responsibilities
    /// of the caller or a derived factory class
    /// </summary>
    /// <returns>An appropriate instance derived from ClientLeafSubGrid</returns>
    public IClientLeafSubGrid GetSubGridEx(GridDataType gridDataType, double cellSize, byte level, int originX, int originY)
    {
      var result = GetSubGrid(gridDataType);

      if (result != null)
      {
        result.CellSize = cellSize;
        result.SetAbsoluteLevel(level);
        result.SetAbsoluteOriginPosition(originX, originY);
      }

      return result;
    }

    /// <summary>
    /// Return a client grid previous obtained from the factory so it may reuse it
    /// </summary>
    public void ReturnClientSubGrid(ref IClientLeafSubGrid clientGrid)
    {
      // TODO: Don't accept any repatriated sub grids for now

      /*
      if (clientGrid == null)
        return;

      // Make sure the type of the client grid being returned matches it's advertised grid type
      // if (!typeMap[(int)clientGrid.GridDataType].Equals(clientGrid.GetType()))
      // {
      //    throw new TRexException("Type of client grid being returned does not match advertised grid data type.");
      // }

      ClientLeaves[(int) clientGrid.GridDataType].Add(clientGrid);
      */
      clientGrid = null;
    }

    /// <summary>
    /// Return an array of client grids (of the same type) previously obtained from the factory so it may reuse them
    /// </summary>
    public void ReturnClientSubGrids(IClientLeafSubGrid[] clientGrids, int count)
    {
      if (count < 0 || count > clientGrids.Length)
        throw new ArgumentException("Invalid count of sub grids to return", nameof(count));

      for (var i = 0; i < count; i++)
        ReturnClientSubGrid(ref clientGrids[i]);
    }

    /// <summary>
    /// Return an array of client grids (of the same type) previously obtained from the factory so it may reuse them
    /// </summary>
    public void ReturnClientSubGrids(IClientLeafSubGrid[][] clientGrids, int count)
    {
      if (count < 0 || count > clientGrids.Length)
        throw new ArgumentException("Invalid count of sub grids to return", nameof(count));

      for (var i = 0; i < count; i++)
      {
        for (var j = 0; j < clientGrids[i]?.Length; j++)
          ReturnClientSubGrid(ref clientGrids[i][j]);
      }
    }
  }
}
