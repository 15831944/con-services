﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Pegasus.Client
{
  public interface IPegasusClient
  {
    Task<TileMetadata> GenerateDxfTiles(string dcFileName, string dxfFileName, DxfUnitsType dxfUnitsType,
      IDictionary<string, string> customHeaders);

    Task<bool> DeleteDxfTiles(string dxfFileName, IDictionary<string, string> customHeaders);

    Task<TileMetadata> GenerateGeoTiffTiles(string geoTiffFileName, IDictionary<string, string> customHeaders);

    Task<bool> DeleteGeoTiffTiles(string geoTiffFileName, IDictionary<string, string> customHeaders);
  }
}
