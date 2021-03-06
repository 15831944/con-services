﻿using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cluster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using VSS.TRex.Common;
using VSS.TRex.DI;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// The affinity function used by TRex to spread data amongst processing servers
  /// </summary>
  public class AffinityFunctionBase : IAffinityFunction
  {
    protected static readonly ILogger Log = Logging.Logger.CreateLogger<AffinityFunctionBase>();

    // Set NumPartitions to the default number of partitions
    protected static readonly int NumPartitions = DIContext.Obtain<IConfigurationStore>().GetValueInt("NUMPARTITIONS_PERDATACACHE", Consts.NUMPARTITIONS_PERDATACACHE);

    /// <summary>
    /// Return the number of partitions to use for affinity.
    /// </summary>
    public int Partitions => NumPartitions;
    
    /// <summary>
    /// Determine how the nodes in the grid are to be assigned into the partitions configured in the cache
    /// </summary>
    public IEnumerable<IEnumerable<IClusterNode>> AssignPartitions(AffinityFunctionContext context)
    {
      // Create the (empty) list of node mappings for the affinity partition assignment
      var result = Enumerable.Range(0, NumPartitions).Select(x => new List<IClusterNode>()).ToList();

      try
      {
        var traceEnabled = Log.IsTraceEnabled();

        if (traceEnabled)
        {
          Log.LogInformation("Assigning partitions");
        }

        /* Debug code to dump the attributes assigned to nodes being looked at
        foreach (var node in context.CurrentTopologySnapshot)
        {
            Log.LogInformation($"Topology Node {node.Id}:");
            foreach (KeyValuePair<string, object> pair in node.GetAttributes())
                Log.LogInformation($"Attributes Pair: {pair.Key} -> {pair.Value}");
        } */

        var nodes = context.CurrentTopologySnapshot.ToList();

        // Assign all nodes to affinity partitions. Spare nodes will be mapped as backups. 
        if (nodes.Count > 0)
        {
          /* Debug code to dump the attributes assigned to nodes being looked at
          foreach (var a in Nodes.First().GetAttributes())
              Log.LogInformation($"Attribute: {a.ToString()}");
          */

          if (traceEnabled)
          {
            Log.LogInformation($"Assigning partitions to {nodes.Count} nodes in {nameof(AffinityFunctionBase)}.{nameof(AssignPartitions)}");
          }

          for (var partitionIndex = 0; partitionIndex < NumPartitions; partitionIndex++)
          {
            result[partitionIndex].Add(nodes[partitionIndex % nodes.Count]);

            if (traceEnabled)
            {
              Log.LogTrace($"--> Assigned node:{nodes[partitionIndex % nodes.Count].ConsistentId} to partition {partitionIndex}");
            }
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception:");
        return new List<List<IClusterNode>>();
      }

      return result;
    }

    /// <summary>
    /// Given a cache key, determine which partition the cache item should reside
    /// </summary>
    public virtual int GetPartition(object key)
    {
      Log.LogWarning($"Base class GetPartition() called in {nameof(AffinityFunctionBase)}");

      // No-op in base class
      return 0;
    }

    /// <summary>
    /// Remove a node from the topology. There is no special logic required here; the AssignPartitions method should be called again
    /// to reassign the remaining nodes into the partitions
    /// </summary>
    public void RemoveNode(Guid nodeId)
    {
      Log.LogInformation($"Removing node {nodeId}");

      // Don't care at this point, I think...
    }
  }
}
