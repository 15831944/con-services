﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{
    /// <summary>
    /// The factory used to create the client subgrid creation factory. This abstracts the factory creation aspect away
    /// fromt he depednecy injection aspect.
    /// </summary>
    public static class ClientLeafSubgridFactoryFactory
    {
        /// <summary>
        /// Local instance variable for the singleton factory isntance that is provided to all callers
        /// </summary>
        private static IClientLeafSubgridFactory instance = null;

        /// <summary>
        /// Gets the subgrid client factory to use. Replace this with an implementation that 
        /// returns an appropriate element from the Dependency Injection container when this is implemented
        /// </summary>
        /// <returns></returns>
        public static IClientLeafSubgridFactory GetClientLeafSubGridFactory()
        {
            if (instance == null)
            {
                instance = new ClientLeafSubGridFactory();

                // Hardwiring registration of client data types here. May want to make this more dependency injection controlled....
                instance.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));
            }

            return instance;
        }
    }
}
