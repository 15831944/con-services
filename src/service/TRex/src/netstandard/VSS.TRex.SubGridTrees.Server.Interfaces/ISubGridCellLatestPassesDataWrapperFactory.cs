﻿namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellLatestPassesDataWrapperFactory
  {
    /// <summary>
    /// Constructs a mutable (non static) segment cell pass wrapper which is a fully mutable high fidelity representation (most memory blocks allocated)
    /// </summary>
    /// <returns></returns>
    ISubGridCellLatestPassDataWrapper NewMutableWrapper_Global();

    /// <summary>
    /// Constructs a mutable (non static) segment cell pass wrapper which is a fully mutable high fidelity representation (most memory blocks allocated)
    /// </summary>
    /// <returns></returns>
    ISubGridCellLatestPassDataWrapper NewMutableWrapper_Segment();

    /// <summary>
    /// Constructs a global immutable (static) cell pass wrapper which is immutable projection with lower fidelity and which
    /// is compressed (with trivial loss level), few memory blocks allocated
    /// </summary>
    /// <returns></returns>
    ISubGridCellLatestPassDataWrapper NewImmutableWrapper_Global();

    /// <summary>
    /// Constructs an immutable (static) segment cell pass wrapper which is immutable projection with lower fidelity and which
    /// is compressed (with trivial loss level), few memory blocks allocated
    /// </summary>
    /// <returns></returns>
    ISubGridCellLatestPassDataWrapper NewImmutableWrapper_Segment();
  }
}
