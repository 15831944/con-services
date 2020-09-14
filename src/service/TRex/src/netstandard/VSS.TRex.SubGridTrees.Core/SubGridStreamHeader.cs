﻿using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Defines the header written at the start of a stream containing sub grid information, either sub grid directory or sub grid segment.
    /// </summary>
    public struct SubGridStreamHeader : IBinaryReaderWriter
  {
        public const byte VERSION = 2;

        public const int kSubGridHeaderFlag_IsSubGridDirectoryFile = 0x1;
        public const int kSubGridHeaderFlag_IsSubGridSegmentFile = 0x2;

        public static readonly byte[] kICServerSubGridLeafFileMoniker = { 73, 67, 83, 71, 76, 69, 65, 70 }; // 'ICSGLEAF' 
        public static readonly byte[] kICServerSubGridDirectoryFileMoniker = { 73, 67, 83, 71, 68, 73, 82, 76 }; // 'ICSGDIRL';

        public byte[] Identifier { get; set; }

        public byte Version;

        public int Flags;
        public DateTime StartTime;
        public DateTime EndTime;

        // FLastUpdateTimeUTC records the time at which this sub grid was last updated
        // in the persistent store
        public DateTime LastUpdateTimeUTC;

        public bool IsSubGridDirectoryFile => (Flags & kSubGridHeaderFlag_IsSubGridDirectoryFile) != 0;
        public bool IsSubGridSegmentFile => (Flags & kSubGridHeaderFlag_IsSubGridSegmentFile) != 0;

        public void Read(BinaryReader reader)
        {
            Version = VersionSerializationHelper.CheckVersionByte(reader, VERSION);

            if (Version >= 1)
            {
              Identifier = reader.ReadBytes(8);

              Flags = reader.ReadInt32();

              StartTime = DateTime.FromBinary(reader.ReadInt64());
              EndTime = DateTime.FromBinary(reader.ReadInt64());
              LastUpdateTimeUTC = DateTime.FromBinary(reader.ReadInt64());
            }
        }

        public void Write(BinaryWriter writer)
        {
            VersionSerializationHelper.EmitVersionByte(writer, VERSION);

            writer.Write(Identifier);

            writer.Write(Flags); // | kSubGridHeaderFlag_SubGridContentIsVersioned);

            writer.Write(StartTime.ToBinary());
            writer.Write(EndTime.ToBinary());
            writer.Write(LastUpdateTimeUTC.ToBinary());
        }

        public SubGridStreamHeader(BinaryReader reader) : this()
        {
            Read(reader);
        }

        /// <summary>
        /// Determines if the header moniker in the stream matches the expected moniker for the context the stream is being read into
        /// </summary>
        public bool IdentifierMatches(byte[] identifier)
        {
            var result = Identifier.Length == identifier.Length;

            if (result)
            {
              for (var i = 0; i < Identifier.Length; i++)
              {
                result &= Identifier[i] == identifier[i];
              }
            }

            return result;
        }
    }
}
