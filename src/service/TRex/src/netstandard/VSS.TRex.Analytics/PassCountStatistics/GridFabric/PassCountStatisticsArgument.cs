﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Records;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a Pass Count statistics request
  /// </summary>    
  public class PassCountStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Pass Count details values.
    /// </summary>
    public int[] PassCountDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteIntArray(PassCountDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      PassCountDetailValues = reader.ReadIntArray();
    }
  }
}
