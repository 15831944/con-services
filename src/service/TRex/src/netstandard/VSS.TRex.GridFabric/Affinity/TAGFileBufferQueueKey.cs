﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{

  /// <summary>
  /// The key used to identify TAG files in the TAG file buffer queue
  /// </summary>
  public struct TAGFileBufferQueueKey : ITAGFileBufferQueueKey, IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The name of the TAG file being processed
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The project to process that TAG file into.
    /// This field also provides the affinity key mapping to the nodes in the mutable data grid
    /// </summary>
    public Guid ProjectUID { get; set; }

    public Guid AssetUID { get; set; }

    /// <summary>
    /// TAG File Buffer Queue key constructor taking project, asset and filename
    /// </summary>
    public TAGFileBufferQueueKey(string fileName, Guid projectID, Guid assetUid)
    {
      FileName = fileName;
      ProjectUID = projectID;
      AssetUID = assetUid;
    }

    /// <summary>
    /// Provides string representation of the state of the key
    /// </summary>
    public override string ToString() => $"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}"; //$"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}";

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUID);
      writer.WriteGuid(AssetUID);
      writer.WriteString(FileName);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectUID = reader.ReadGuid() ?? Guid.Empty;
        AssetUID = reader.ReadGuid() ?? Guid.Empty;
        FileName = reader.ReadString();
      }
    }
  }
}
