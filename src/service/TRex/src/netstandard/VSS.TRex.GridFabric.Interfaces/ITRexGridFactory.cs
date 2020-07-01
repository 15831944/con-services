﻿using Apache.Ignite.Core;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Grids
{
  public interface ITRexGridFactory
  {
    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in
    /// </summary>
    IIgnite Grid(string gridName, IgniteConfiguration cfg = null);

    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in.
    /// If the grid reference has previously been requested it returned from a cached reference.
    /// </summary>
    IIgnite Grid(StorageMutability mutability, IgniteConfiguration cfg = null);

    /// <summary>
    /// Stops all running Ignite grids in this instance
    /// </summary>
    void StopGrids();
  }
}
