﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Utilities;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// The affinity function used by TRex to spread spatial data amongst processing servers
  /// </summary>
  public class ProjectBasedSpatialAffinityFunction : AffinityFunctionBase
  {
    /// <summary>
    /// Given a cache key, determine which partition the cache item should reside
    /// </summary>
    public override int GetPartition(object key)
    {
      // Pull the sub grid origin location for the sub grid or segment represented in the cache key and calculate the 
      // spatial processing division descriptor to use as the partition affinity key

      if (key is IProjectAffinity value)
      {
        // Compute partition number as the modulo NumPartitions result against the project iD in the spatial affinity key
        return Math.Abs(GuidHashCode.Hash(value.ProjectUID)) % NumPartitions;
      }

      Log.LogCritical($"Unknown key type to compute non spatial affinity partition key for: [{key.GetType().FullName}] {key}. Returning partition 0 to avoid thrown exception");
      return 0;

      //throw new ArgumentException($"Unknown key type to compute spatial affinity partition key for: [{key.GetType().FullName}] {key}");
    }
  }
}
