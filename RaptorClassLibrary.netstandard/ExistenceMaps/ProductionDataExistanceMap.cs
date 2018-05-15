﻿using System;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ExistenceMaps
{
    /// <summary>
    /// ProductionDataExistanceMap stores a map of existing subgrids that contain data processed into the datamodel
    /// </summary>
    public class ProductionDataExistanceMap
    {
        public Guid DataModelID { get; set; } 

        public SubGridTreeSubGridExistenceBitMask Map { get; set; }

        public ProductionDataExistanceMap(Guid dataModelID, double cellSize)
        {
            DataModelID = dataModelID;

            Map = new SubGridTreeSubGridExistenceBitMask
            {
                CellSize = cellSize
            }; //, kICFSSubgridSpatialExistanceMapHeader, kICFSSubgridSpatialExistanceMapVersion);
        }    
    }
}
