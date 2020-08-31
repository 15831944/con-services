﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class BaseDesignRequestResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public DesignProfilerRequestResult RequestResult { get; set; } = DesignProfilerRequestResult.UnknownError;

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByte((byte)RequestResult);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        RequestResult = (DesignProfilerRequestResult) reader.ReadByte();
      }
    }
  }
}
