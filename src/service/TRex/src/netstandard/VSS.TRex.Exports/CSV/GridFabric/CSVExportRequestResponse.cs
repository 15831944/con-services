﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// The response returned from the CSVExport request executor
  ///   that contains the response code and the set of formatted rows
  ///   extracted from the sub grids for the export question.
  /// </summary>
  public class CSVExportRequestResponse : SubGridsPipelinedResponseBase
  {
    private static byte VERSION_NUMBER = 1;

    public string fileName = string.Empty;

    public CSVExportRequestResponse()
    { }
    
    
     /// <summary>
     /// Serializes content to the writer
     /// </summary>
     public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(fileName);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        fileName = reader.ReadString();
      }
    }
  }
}
