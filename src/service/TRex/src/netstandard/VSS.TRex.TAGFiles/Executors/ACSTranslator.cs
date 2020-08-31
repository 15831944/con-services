﻿using System;
using System.Collections.Generic;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Using the CoreX library covert UTM coordinates into WSG84LL coords, then back to project coordinates.  
  /// </summary>
  public class ACSTranslator : IACSTranslator
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ACSTranslator>();

    private bool ValidPositionsforPair(UTMCoordPointPair uTMCoordPointPair)
    {
      return !(uTMCoordPointPair.Left.X == Consts.NullReal || uTMCoordPointPair.Left.Y == Consts.NullReal || uTMCoordPointPair.Right.X == Consts.NullReal || uTMCoordPointPair.Right.Y == Consts.NullReal);
    }

    public List<UTMCoordPointPair> TranslatePositions(string projectCSIBFile, List<UTMCoordPointPair> coordPositions)
    {
      if (projectCSIBFile == string.Empty)
      {
        _log.LogError($"TranslatePositions. Missing project CSIB file.");
        return null;
      }

      if (coordPositions == null || coordPositions.Count == 0) return coordPositions;  // nothing todo

      try
      {

        var coreXWrapper = DIContext.Obtain<ICoreXWrapper>();
        if (coreXWrapper == null)
        {
          _log.LogError("TranslatePositions. IConvertCoordinates not implemented");
          return null;
        }

        byte currentUTMZone = 0;
        var currentUTMCSIBFile = string.Empty;

        for (var i = 0; i < coordPositions.Count; i++)
        {

          if (coordPositions[i].UTMZone != currentUTMZone || currentUTMCSIBFile == string.Empty)
          {
            currentUTMZone = coordPositions[i].UTMZone;

            var zone = UTMZoneHelper.GetZoneDetailsFromUTMZone(currentUTMZone);
            currentUTMCSIBFile = coreXWrapper.GetCSIBFromCSDSelection(zone.zoneGroup, zone.zoneName);
          }

          if (ValidPositionsforPair(coordPositions[i]))
          {
            // convert left point to WGS84 LL point
            var leftLLPoint = coreXWrapper.NEEToLLH(currentUTMCSIBFile, coordPositions[i].Left.ToCoreX_XYZ()).ToTRex_XYZ();
            // convert left WGS84 LL point to project NNE
            var leftNNEPoint = coreXWrapper.LLHToNEE(projectCSIBFile, leftLLPoint.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians).ToTRex_XYZ();

            // convert right point to WGS84 LL point
            var rightLLPoint = coreXWrapper.NEEToLLH(currentUTMCSIBFile, coordPositions[i].Right.ToCoreX_XYZ()).ToTRex_XYZ();
            // convert right WGS84 LL point to project NNE
            var rightNNEPoint = coreXWrapper.LLHToNEE(projectCSIBFile, rightLLPoint.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians).ToTRex_XYZ();

            coordPositions[i] = new UTMCoordPointPair(leftNNEPoint, rightNNEPoint, currentUTMZone);
          }

        }

      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Exception occurred while converting ACS coordinates");
        return null;
      }

      return coordPositions;
    }
  }
}
