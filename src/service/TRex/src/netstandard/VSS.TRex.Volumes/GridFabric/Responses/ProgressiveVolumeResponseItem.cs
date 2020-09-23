﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Volumes.GridFabric.Responses
{
  public class ProgressiveVolumeResponseItem : BaseRequestResponse, IAggregateWith<ProgressiveVolumeResponseItem>
  {
    private const byte VERSION_NUMBER = 1;

    public DateTime Date { get; set; }
    public SimpleVolumesResponse Volume = new SimpleVolumesResponse();

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(Date.ToBinary());
      writer.WriteBoolean(Volume != null);
      Volume?.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Date = DateTime.FromBinary(reader.ReadLong());
        if (reader.ReadBoolean())
        {
          (Volume ??= new SimpleVolumesResponse()).FromBinary(reader);
        }
      }
    }

    public ProgressiveVolumeResponseItem AggregateWith(ProgressiveVolumeResponseItem other)
    {
      Volume.AggregateWith(other.Volume);

      return this;
    }
  }
}
