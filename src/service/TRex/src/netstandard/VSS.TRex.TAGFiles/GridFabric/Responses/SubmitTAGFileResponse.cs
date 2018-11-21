﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  /// <summary>
  /// Code if negative means it was generated by mutable SubmitTAGFileExecutor checks. If positive it means its the code from TFA service validation
  /// </summary>
  public class SubmitTAGFileResponse : BaseRequestResponse
  {
    private const byte versionNumber = 1;

    public string FileName { get; set; }

    public bool Success { get; set; }

    public int Code { get; set; }

    public string Message { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubmitTAGFileResponse()
    {
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(versionNumber);
      writer.WriteString(FileName);
      writer.WriteBoolean(Success);
      writer.WriteInt(Code);
      writer.WriteString(Message);
    }

    public override void FromBinary(IBinaryRawReader reader)
    { 
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != versionNumber)
        throw new TRexSerializationVersionException(versionNumber, readVersionNumber);

      FileName = reader.ReadString();
      Success = reader.ReadBoolean();
      Code = reader.ReadInt();
      Message = reader.ReadString();
    }
  }
}
