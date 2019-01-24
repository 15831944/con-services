﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITileServiceProxy
  {
    Task<TileMetadata> GenerateDxfTiles(string dcFileName, string dxfFileName, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders, int timeoutMins);
  }
}
