﻿using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.IO;
using VSS.TRex.IO.Helpers;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  public static class DXFFileUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("DXFFileUtilities");

    public static DXFUtilitiesResult RequestBoundariesFromLineWork(MemoryStream dxfFileStream, DxfUnitsType units, uint maxBoundariesToProcess, bool convertLineStringCoordsToPolygon, out PolyLineBoundaries boundaries)
    {
      boundaries = null;
      var reader = new DXFReader(dxfFileStream, units);

      try
      {
        if (!reader.FindEntitiesSection())
        {
          return DXFUtilitiesResult.NoEntitiesSectionFound;
        }

        var extractor = new PolyLineBoundaryExtractor(reader);
        var result = extractor.Extract(units, maxBoundariesToProcess, convertLineStringCoordsToPolygon);

        if (result == DXFUtilitiesResult.Ok)
          boundaries = extractor.Boundaries;

        return result;
      }
      catch (FormatException e)
      {
        Log.LogError(e, "DXF file not in expected format");
        return DXFUtilitiesResult.UnknownFileFormat;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception processing DXF file");
        throw;
      }
    }

    public static DXFUtilitiesResult RequestBoundariesFromLineWork(string dxfFileContext, DxfUnitsType units, uint maxBoundariesToProcess, bool convertLineStringCoordsToPolygon, out PolyLineBoundaries boundaries)
    {
      using var stream = new MemoryStream(Convert.FromBase64String(dxfFileContext));

      return DXFFileUtilities.RequestBoundariesFromLineWork(stream, units, maxBoundariesToProcess, convertLineStringCoordsToPolygon, out boundaries);
    }
  }
}
