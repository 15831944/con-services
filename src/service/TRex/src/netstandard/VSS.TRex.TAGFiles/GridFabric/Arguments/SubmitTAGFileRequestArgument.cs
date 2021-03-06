﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
  public class SubmitTAGFileRequestArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 4;
    private static byte[] VERSION_NUMBERS = {1, 2, 3, 4};

    /// <summary>
    /// Overridden ID of the project to process the TAG files into
    /// </summary>
    public Guid? ProjectID { get; set; }

    /// <summary>
    /// Overridden ID of the asset to process the TAG files into
    /// </summary>
    //public long AssetUID { get; set; } = -1;
    public Guid? AssetID { get; set; }

    /// <summary>
    /// Indicates that this TAG file should be treated as from a John Doe asset when processed.
    /// Optional: Defaults to false
    /// </summary>
    public bool TreatAsJohnDoe { get; set; } // = false;

    /// <summary>
    /// Name of physical tag file
    /// </summary>
    public string TAGFileName { get; set; } = string.Empty;

    /// <summary>
    /// The content of the TAG file being submitted
    /// </summary>
    public byte[] TagFileContent { get; set; }

    /// <summary>
    /// Helps TFA service determine correct project
    /// </summary>
    public string TCCOrgID { get; set; } = string.Empty;

    /// <summary>
    /// States if the TAG fie should be added to the TAG file archive during processing
    /// </summary>
    public TAGFileSubmissionFlags SubmissionFlags { get; set; } = TAGFileSubmissionFlags.AddToArchive;

    /// <summary>
    /// The origin source that produced the TAG file, such as GCS900, Earthworks etc
    /// </summary>
    public TAGFileOriginSource OriginSource { get; set; } = TAGFileOriginSource.LegacyTAGFileSource;

    /// <summary>
    ///  Default no-arg constructor
    /// </summary>
    public SubmitTAGFileRequestArgument()
    {
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetID);
      writer.WriteString(TAGFileName);
      writer.WriteString(TCCOrgID);
      writer.WriteByteArray(TagFileContent);
      writer.WriteBoolean(TreatAsJohnDoe);
      writer.WriteInt((int)SubmissionFlags);
      writer.WriteInt((int)OriginSource);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var messageVersion = VersionSerializationHelper.CheckVersionsByte(reader, VERSION_NUMBERS);

      if (messageVersion >= 1)
      {
        ProjectID = reader.ReadGuid();
        AssetID = reader.ReadGuid();
        TAGFileName = reader.ReadString();
        TCCOrgID = reader.ReadString();
        TagFileContent = reader.ReadByteArray();
      }

      if (messageVersion >= 2)
      {
        TreatAsJohnDoe = reader.ReadBoolean();
      }

      SubmissionFlags = TAGFileSubmissionFlags.AddToArchive;
      if (messageVersion >= 3)
      {
        SubmissionFlags = (TAGFileSubmissionFlags)reader.ReadInt();
      }

      if (messageVersion >= 4)
      {
        OriginSource = (TAGFileOriginSource)reader.ReadInt();
      }
    }
  }
}
