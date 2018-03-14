﻿using Apache.Ignite.Core;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Grids
{
    public static class RaptorGridFactory
    {
        /// <summary>
        /// Creates an appropriate new Ignite grid reference depending on the Raptor Grid passed in
        /// </summary>
        /// <param name="gridName"></param>
        /// <returns></returns>
        public static IIgnite Grid(string gridName)
        {
            if (gridName.Equals(RaptorGrids.RaptorMutableGridName()))
            {
                return Ignition.TryGetIgnite(gridName);
            }
            else if (gridName.Equals(RaptorGrids.RaptorImmutableGridName()))
            {
                return Ignition.TryGetIgnite(gridName);
            }
            else
            {
                throw new ArgumentException($"{gridName} is an unknown grid to create a reference for.");
            }
        }
    }
}
