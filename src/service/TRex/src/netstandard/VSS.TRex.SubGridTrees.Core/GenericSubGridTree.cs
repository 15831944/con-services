﻿using System;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees
{
  /// <summary>
  /// GenericSubGridTree in T implements a sub grid tree where all the cells in the leaf sub grids are generic type T, which
  /// are in turn implemented using a GenericLeafSubGrid in T based subclass, S.
  /// The tree automates tree node and leaf sub grid management behind a uniform tree wide Cells[,] facade.
  /// </summary>

  public class GenericSubGridTree<T, S> : SubGridTree, IGenericSubGridTree<T, S> 
    where S : IGenericLeafSubGrid<T>, ILeafSubGrid, new()
  {
    /// <summary>
    /// Default indexer property to access the cells as a default property of the generic sub grid
    /// </summary>

    public T this[int x, int y]
    {
      get => GetCell(x, y); 
      set => SetCell(x, y, value);
    }

    /// <summary>
    /// Generic cell value setter for the sub grid tree. 
    /// Setting a value for a cell automatically creates all necessary node & leaf sub grids to
    /// store the value.
    /// </summary>
    private T GetCell(int cellX, int cellY)
    {
      var subGrid = LocateSubGridContaining(cellX, cellY, numLevels);

      if (subGrid == null)
        return NullCellValue;

      subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);
      return ((S) subGrid).Items[subGridX, subGridY];
    }

    /// <summary>
    /// Generic cell value getter for the sub grid tree.
    /// Getting a value for a cell automatically traverses the tree to locate the appropriate leaf sub grid
    /// to return the value from.
    /// If there is no leaf sub grid, or the value in the leaf sub grid is nul, this function returns the value
    /// represented by NullCellValue().
    /// </summary>
    private void SetCell(int cellX, int cellY, T value)
    {
      var subGrid = ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf);

      subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);
      ((S) subGrid).Items[subGridX, subGridY] = value;
    }

    /// <summary>
    /// NullGetCellValue is the null value leaf cell values stored in this generic sub grid tree.
    /// Descendants should override this method. Calling it directly will result in the standard .Net
    /// default value for type T being returned.
    /// </summary>
    public virtual T NullCellValue => default(T);

    /// <summary>
    /// Generic sub grid tree constructor. Accepts the standard cell size, number of levels; however,
    /// the sub grid factory is created from the standard NodeSubGrid class, and the base generic leaf sub grid
    /// derived from T. Note: This is only suitable if the default(T) value is appropriate for the cell null value.
    /// </summary>
    public GenericSubGridTree(byte numLevels,
      double cellSize) : base(numLevels, cellSize, new SubGridFactory<NodeSubGrid, S>())
    {
    }

    /// <summary>
    /// Generic sub grid tree constructor. 
    /// The sub grid factory is created from the standard NodeSubGrid class, and the base generic leaf sub grid
    /// derived from T. Note: This is only suitable if the default(T) value is appropriate for the cell null value.
    /// </summary>
    public GenericSubGridTree() : base(SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize, new SubGridFactory<NodeSubGrid, S>())
    {
    }

    /// <summary>
    /// Iterates over all leaf cell values in the entire sub grid tree. All leaf sub grids are
    /// iterated over and all values (both null and non-null) in the leaf sub grid are presented
    /// to the functor.
    /// </summary>
    public void ForEach(Func<T, bool> functor)
    {
      ScanAllSubGrids(subGrid =>
      {
        ((S) subGrid).ForEach(functor);
        return true;
      });
    }
  }
}
