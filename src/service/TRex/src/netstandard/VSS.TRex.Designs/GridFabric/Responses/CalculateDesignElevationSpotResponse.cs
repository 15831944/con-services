﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class CalculateDesignElevationSpotResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public CalculateDesignElevationSpotResponse()
    {
      CalcResult = DesignProfilerRequestResult.UnknownError;
      Elevation = Consts.NullDouble;
    }

    public double Elevation { get; set; }
    public DesignProfilerRequestResult CalcResult { get; set; }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Elevation = reader.ReadDouble();
        CalcResult = (DesignProfilerRequestResult) reader.ReadInt();
      }
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(Elevation);
      writer.WriteInt((int)CalcResult);
    }
  }
}
