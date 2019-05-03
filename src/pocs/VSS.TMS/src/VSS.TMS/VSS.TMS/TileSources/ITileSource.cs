﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.TMS.TileSources
{
  interface ITileSource
  {
  Task<byte[]> GetImageTileAsync(int x, int y, int z);

  Task<byte[]> GetTerrainTileAsync(int x, int y, int z);

  Task<byte[]> GetTerrainQMTileAsync(int x, int y, int z, string path, int mode);

    TileSetConfiguration Configuration { get; }

  string ContentType { get; }

  }
}
