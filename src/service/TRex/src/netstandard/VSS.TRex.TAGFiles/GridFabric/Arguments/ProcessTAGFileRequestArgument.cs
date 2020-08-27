﻿using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
  public class ProcessTAGFileRequestArgument : BaseRequestArgument
  {
    public const byte VERSION_NUMBER = 1;

    /// <summary>
    /// ID of the project to process the TAG files into
    /// </summary>
    public Guid ProjectID { get; set; } = Guid.Empty;

    /// <summary>
    /// A dictionary mapping TAG file names to the content of each file
    /// </summary>
    public List<ProcessTAGFileRequestFileItem> TAGFiles { get; set; }

    /// <summary>
    ///  Default no-arg constructor
    /// </summary>
    public ProcessTAGFileRequestArgument()
    {
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);

      writer.WriteInt(TAGFiles?.Count ?? 0);
      if (TAGFiles != null)
      {
        foreach (var tagFile in TAGFiles)
          tagFile.ToBinary(writer);
      }
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectID = reader.ReadGuid() ?? Guid.Empty;

        var numTagFiles = reader.ReadInt();
        TAGFiles = new List<ProcessTAGFileRequestFileItem>(numTagFiles);

        for (var i = 0; i < numTagFiles; i++)
          TAGFiles.Add(new ProcessTAGFileRequestFileItem(reader));
      }
    }
  }
}
