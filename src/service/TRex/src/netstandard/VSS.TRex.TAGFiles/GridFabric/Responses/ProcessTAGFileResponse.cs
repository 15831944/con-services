﻿using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  public class ProcessTAGFileResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public List<IProcessTAGFileResponseItem> Results { get; set; } = new List<IProcessTAGFileResponseItem>();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ProcessTAGFileResponse()
    {
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(Results.Count);
      Results.ForEach(result => result.ToBinary(writer));
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        var numResults = reader.ReadInt();
        Results = new List<IProcessTAGFileResponseItem>(numResults);

        for (var i = 0; i < numResults; i++)
        {
          Results.Add(new ProcessTAGFileResponseItem(reader));
        }
      }
    }
  }
}
