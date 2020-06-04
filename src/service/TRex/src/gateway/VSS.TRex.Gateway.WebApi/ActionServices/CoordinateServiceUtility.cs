﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.WebApi.ActionServices
{
  /// <summary>
  /// Check that the site model exists, and appropriate designs exist.
  /// </summary>
  public class CoordinateServiceUtility : ICoordinateServiceUtility
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CoordinateServiceUtility>();

    /// <summary>
    /// Converts XYZ to LLH
    /// </summary>
    public async Task<int> PatchLLH(string CSIB, List<MachineStatus> machines)
    {
      if (!machines.Any())
        return ContractExecutionStatesEnum.ExecutedSuccessfully;

      var NEECoords = new List<XYZ>();
      foreach (var machine in machines)
      {
        if (machine.lastKnownX != null && machine.lastKnownY != null &&
            machine.lastKnownX != Consts.NullDouble && machine.lastKnownY != Consts.NullDouble)
          NEECoords.Add(new XYZ(machine.lastKnownX.Value, machine.lastKnownY.Value, 0.0)); // Note: 2D conversion only, elevation set to 0
      }

      if (NEECoords.Count > 0)
      {
        var (errorCode, LLHCoords) = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(CSIB, NEECoords.ToArray());
     
        // if the count returned is different to that sent, then we can't match with the machines list
        if (errorCode == RequestErrorStatus.OK && NEECoords.Count == LLHCoords.Length)
        {
          var coordPointer = 0;
          foreach (var machine in machines)
          {
            if (machine.lastKnownX != null && machine.lastKnownY != null &&
                machine.lastKnownX != Consts.NullDouble && machine.lastKnownY != Consts.NullDouble)
            {
              machine.lastKnownLatitude = LLHCoords[coordPointer].Y;
              machine.lastKnownLongitude = LLHCoords[coordPointer].X;
            }
            coordPointer++;
          }
        }
        else
        {
          var message = $"{nameof(CoordinateServiceUtility)} Failed to convert Coordinates. ErrorCode: {errorCode} CSIB: {CSIB} Coords: {(LLHCoords == null ? "null LLH returned" : JsonConvert.SerializeObject(LLHCoords))}, NEECoords: {JsonConvert.SerializeObject(NEECoords)}";
          Log.LogError(message);
          return ContractExecutionStatesEnum.InternalProcessingError;
        }
      }
      else
      {
        Log.LogError(
          $"{nameof(CoordinateServiceUtility)} No coordinates need converting as no machines have lastKnownXY. Machines: {JsonConvert.SerializeObject(machines)}");
      }

      return ContractExecutionStatesEnum.ExecutedSuccessfully;
    }
  }
}
