﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.SiteModels.Interfaces.Requests;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelMetaData : VersionCheckedBinarizableSerializationBase, IRebuildSiteModelMetaData
  {
    private static byte VERSION_NUMBER = 1;

    /// <summary>
    /// The current phase of project rebuilding this project is in
    /// </summary>
    public RebuildSiteModelPhase Phase { get; set; }

    /// <summary>
    /// A set of flags governing aspects of site model rebuilding, such as archival of TAG files processed during the rebuild
    /// </summary>
    public RebuildSiteModelFlags Flags { get; set; }

    /// <summary>
    /// The UTC date at which the last update to this metadata was made
    /// </summary>
    public long LastUpdateUtcTicks { get; set; }

    /// <summary>
    /// Project being rebuilt
    /// </summary>
    public Guid ProjectUID { get; set; }

    /// <summary>
    /// Defines how selective the site model delete operation should be.
    /// Selectivity allows certain portions of a site model to be deleted to help with operations
    /// like rebuilding projects on demand.
    /// </summary>
    public DeleteSiteModelSelectivity DeletionSelectivity { get; set; }

    /// <summary>
    /// The result of the deletion stage of the project rebuild
    /// </summary>
    public DeleteSiteModelResult DeletionResult { get; set; }

    /// <summary>
    /// The result of this rebuild request. As this process may be long, this response will chiefly indicate the
    /// success or failure of starting the overall process of rebuilding a project.
    /// </summary>
    public RebuildSiteModelResult RebuildResult { get; set; }

    /// <summary>
    /// Denotes the type of S3 transfer proxy the site model rebuilder should use to scan and source TAG files for reprocessing
    /// This allows different sources such as the primary TAG file archive, or a prepared project migration bucket to be referenced
    /// </summary>
    public TransferProxyType OriginS3TransferProxy { get; set; }

    /// <summary>
    /// The number of tag files extracted from the S3 repository ready to submit for processing
    /// </summary>
    public int NumberOfTAGFilesFromS3 { get; set; }

    /// <summary>
    /// The number of collections of tag file keys extracted from the S3 repository have been submitted into the file cache
    /// </summary>
    public int NumberOfTAGFileKeyCollections { get; set; }

    /// <summary>
    /// The last known submitted TAG file
    /// </summary>
    public string LastSubmittedTagFile { get; set; }

    /// <summary>
    /// The last known processed TAG file
    /// </summary>
    public string LastProcessedTagFile { get; set; }

    /// <summary>
    /// The number of TAG files submitted to the TAG file processor
    /// </summary>
    public int NumberOfTAGFilesSubmitted { get; set; }

    /// <summary>
    /// The number of TAG file reported processed by the TAG file processor
    /// </summary>
    public int NumberOfTAGFilesProcessed { get; set; }

    public override string ToString()
    {
      return $"Project: {ProjectUID}, Flags: {Flags}, Deletion selectivity: {DeletionSelectivity}, OriginTransferProxy:{OriginS3TransferProxy}, TAG files:{NumberOfTAGFilesFromS3}/{NumberOfTAGFileKeyCollections}/{NumberOfTAGFilesSubmitted}/{NumberOfTAGFilesProcessed}, Phase: {Phase}, {LastSubmittedTagFile}/{LastProcessedTagFile}";
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectUID = reader.ReadGuid() ?? Guid.Empty;
        Flags = (RebuildSiteModelFlags) reader.ReadByte();
        DeletionSelectivity = (DeleteSiteModelSelectivity) reader.ReadInt();

        OriginS3TransferProxy = (TransferProxyType) reader.ReadByte();
        NumberOfTAGFilesFromS3 = reader.ReadInt();
        NumberOfTAGFileKeyCollections = reader.ReadInt();
        NumberOfTAGFilesSubmitted = reader.ReadInt();
        NumberOfTAGFilesProcessed = reader.ReadInt();

        LastUpdateUtcTicks = reader.ReadLong();
        Phase = (RebuildSiteModelPhase) reader.ReadByte();
        LastSubmittedTagFile = reader.ReadString();
        LastProcessedTagFile = reader.ReadString();

        DeletionResult = (DeleteSiteModelResult) reader.ReadByte();
        RebuildResult = (RebuildSiteModelResult) reader.ReadByte();
      }
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUID);
      writer.WriteByte((byte)Flags);
      writer.WriteInt((int)DeletionSelectivity);

      writer.WriteByte((byte)OriginS3TransferProxy);
      writer.WriteInt(NumberOfTAGFilesFromS3);
      writer.WriteInt(NumberOfTAGFileKeyCollections);
      writer.WriteInt(NumberOfTAGFilesSubmitted);
      writer.WriteInt(NumberOfTAGFilesProcessed);

      writer.WriteLong(LastUpdateUtcTicks);
      writer.WriteByte((byte)Phase);
      writer.WriteString(LastSubmittedTagFile);
      writer.WriteString(LastProcessedTagFile);

      writer.WriteByte((byte)DeletionResult);
      writer.WriteByte((byte)RebuildResult);
    }
  }
}
