﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  /// <summary>
  /// The response returned from the Grid request executor that contains the response code and the set of
  /// sub grids extracted for the grid report in question
  /// </summary>
  public class DeleteSiteModelRequestResponse : BaseRequestResponse
  {
    private static byte VERSION_NUMBER = 1;

    public DeleteSiteModelResult Result;
    public long NumRemovedElements;

    public DeleteSiteModelRequestResponse()
    {
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)Result);
      writer.WriteLong(NumRemovedElements);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Result = (DeleteSiteModelResult)reader.ReadInt();
      NumRemovedElements = reader.ReadLong();
    }
  }
}
