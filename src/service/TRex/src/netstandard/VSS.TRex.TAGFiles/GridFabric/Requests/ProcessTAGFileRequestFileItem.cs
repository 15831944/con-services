﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
    /// <summary>
    /// Represents an internal TAG file item to be processed into a site model. It defines the underlying filename for 
    /// the TAG file, and the content of the file as a byte array
    /// </summary>
    public class ProcessTAGFileRequestFileItem : VersionCheckedBinarizableSerializationBase
    {
        private const byte VERSION_NUMBER = 3;
        private static byte[] VERSION_NUMBERS = {1, 2, 3};

        public string FileName { get; set; }

        public byte[] TagFileContent { get; set; }

        public Guid AssetId { get; set; }

        public bool IsJohnDoe { get; set; }
    
        /// <summary>
        /// States if the TAG fie should be added to the TAG file archive during processing
        /// </summary>
        public TAGFileSubmissionFlags SubmissionFlags { get; set; } = TAGFileSubmissionFlags.AddToArchive;
    
        /// <summary>
        /// The orign source that produced the TAG file, such as GCS900, Eathworjs etc
        /// </summary>
        public TAGFileOriginSource OriginSource { get; set; } = TAGFileOriginSource.LegacyTAGFileSource;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileRequestFileItem()
        {
        }

      /// <summary>
      /// Creates a new item and serialises its content from the supplied IBinaryRawReader
      /// </summary>
      public ProcessTAGFileRequestFileItem(IBinaryRawReader reader)
      {
        FromBinary(reader);
      }

      public override void InternalToBinary(IBinaryRawWriter writer)
      {
        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteString(FileName);
        writer.WriteGuid(AssetId);
        writer.WriteBoolean(IsJohnDoe);
        writer.WriteByteArray(TagFileContent);
        writer.WriteInt((int)SubmissionFlags);
        writer.WriteInt((int)OriginSource);
    }

      public override void InternalFromBinary(IBinaryRawReader reader)
      {
        var messageVersion = VersionSerializationHelper.CheckVersionsByte(reader, VERSION_NUMBERS);

        if (messageVersion >= 1)
        {
          FileName = reader.ReadString();
          AssetId = reader.ReadGuid() ?? Guid.Empty;
          IsJohnDoe = reader.ReadBoolean();
          TagFileContent = reader.ReadByteArray();
        }

        if (messageVersion >= 2)
        {
          SubmissionFlags = (TAGFileSubmissionFlags)reader.ReadInt();
        }
        else
        {
          SubmissionFlags = TAGFileSubmissionFlags.AddToArchive;
        }

        if (messageVersion >= 3)
        {
          OriginSource = (TAGFileOriginSource)reader.ReadInt();
        }
        else
        {
          OriginSource = TAGFileOriginSource.LegacyTAGFileSource;
        }
      }
    }
}
