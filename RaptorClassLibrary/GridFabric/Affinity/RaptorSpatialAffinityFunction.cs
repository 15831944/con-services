﻿using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cluster;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.GridFabric.Affinity
{
    /// <summary>
    /// The affinity function used by Raptor to spread spatial data amongst processing servers
    /// </summary>
    [Serializable]
    public class RaptorSpatialAffinityFunction : IAffinityFunction
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The number of partitions data a partitioned caches in the grid will be spread across
        /// </summary>
        private int NumPartitions = -1;

        /// <summary>
        /// Constructor accepting the node role to be used for spatial affinity selection
        /// </summary>
        /// <param name="role"></param>
        public RaptorSpatialAffinityFunction(string role, int numPartitions)
        {
            Role = role;
            NumPartitions = numPartitions;
        }

        /// <summary>
        /// Return the number of partitions to use for affinity. For this affinity function, the number of partitions
        /// is governed by the configured number of Raptor spatial processing divisions
        /// </summary>
        public int Partitions
        {
            get
            {
                return NumPartitions; 
            }
        }

        /// <summary>
        /// The role that nodes in the grid need to be tagged with to be considered a part of the pool of
        /// nodes being used for spatial affinity
        /// </summary>
        public readonly string Role = "";

        /// <summary>
        /// Determine how the nodes in the grid are to be assigned into the spatial divisions configured in the system
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<IEnumerable<IClusterNode>> AssignPartitions(AffinityFunctionContext context)
        {
            if (NumPartitions < 0)
            {
                throw new ArgumentException($"{NumPartitions} is an invalid number of partitions for an affinity partition map");
            }

            // Create the (empty) list of node mappings for the affinity partition assignment
            List<List<IClusterNode>> result = Enumerable.Range(0, NumPartitions).Select(x => new List<IClusterNode>()).ToList();

            try
            {
                // Given the set of nodes in the cluster, determine that there is (at least) <n> nodes marked with
                // the configured role. If not, then throw an exception. If there are exactly that many nodes, then assign
                // one node to to each partition (where a partition is a Raptor spatial processing subdivision), based
                // on the order the cluster nodes occur in the provided topology. If there are more than n nodes, then
                // assign them in turn to the partitions as backup nodes.

                Log.InfoFormat($"RaptorSpatialAffinityFunction: Assigning partitions from topology for role {Role}");

                /* Debug code to dumo the attributes assigned to nodes being looked at
                foreach (var node in context.CurrentTopologySnapshot)
                {
                    Log.Info($"Topology Node {node.Id}:");
                    foreach (KeyValuePair<string, object> pair in node.GetAttributes())
                    {
                        Log.Info($"Attributes Pair: {pair.Key} -> {pair.Value}");
                    }
                } */

                List<IClusterNode> Nodes = context.CurrentTopologySnapshot.Where(x => x.TryGetAttribute($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", out string State) && State == "True").ToList();

                if (Nodes.Count < NumPartitions)
                {
                    Log.InfoFormat("RaptorSpatialAffinityFunction: Insufficient nodes to establish affinity. {0} nodes available with {1} configured spatial subdivisions, will return partial affinity map.", Nodes.Count, NumPartitions);                 
                }

                // Assign all nodes to affinity partitions. Spare nodes will be mapped as backups. 

                if (Nodes.Count() > 0)
                {
                    /* Debug code to dumo the attributes assigned to nodes being looked at
                    foreach (var a in Nodes.First().GetAttributes())
                    {
                        Log.Info($"Attribute: {a.ToString()}");
                    } */

                    Log.Info("Assigning Raptor spatial partitions");
                    for (int divisionIndex = 0; divisionIndex < NumPartitions; divisionIndex++)
                    {
                        List<IClusterNode> spatialDivisionNodes = Nodes.Where(x => x.TryGetAttribute("SpatialDivision", out int division) && division == divisionIndex).ToList();

                        foreach (IClusterNode node in spatialDivisionNodes)
                        {
                            Log.Info($"Assigned node {node.Id} to division {divisionIndex}");
                            result[divisionIndex].Add(node);
                        }

                        Log.Info($"--> Assigned {spatialDivisionNodes.Count} nodes (out of {Nodes.Count}) to spatial division {divisionIndex}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("RaptorSpatialAffinityFunction: Exception: {0}", e);
                return new List<List<IClusterNode>>();
            }

            return result;
        }

        /// <summary>
        /// Given a cache key, determine which partition the cache item should reside
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetPartition(object key)
        {
            // Pull the subgrid origin location for the subgrid or segment represented in the cache key and calculate the 
            // spatial processing division descriptor to use as the partition affinity key

            if (!(key is SubGridSpatialAffinityKey))
            {
                Log.InfoFormat("Unknown key type to compute spatial affinity partition key for: {0}", key.ToString());
                throw new ArgumentException(String.Format("Unknown key type to compute spatial affinity partition key for: {0}", key.ToString()));
            }

            SubGridSpatialAffinityKey value = (SubGridSpatialAffinityKey)key;

            return (int)SubGridCellAddress.ToSpatialDivisionDescriptor(value.SubGridX, value.SubGridY, (uint)NumPartitions);
        }

        /// <summary>
        /// Remove a node from the topology. There is no special logic required here; the AssignPartitions method should be called again
        /// to reassign the remaining nodes into the spatial partitions
        /// </summary>
        /// <param name="nodeId"></param>
        public void RemoveNode(Guid nodeId)
        {
            Log.InfoFormat("RaptorSpatialAffinityFunction: Removing node {0}", nodeId);
            // Don't care at this point, I think...
            // throw new NotImplementedException();
        }
    }
}
