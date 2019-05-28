﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  public class SummaryVolumeProfileCell : ProfileCellBase, ISummaryVolumeProfileCell
  {
    public float LastCellPassElevation1;
    public float LastCellPassElevation2;

    public SummaryVolumeProfileCell()
    {
      LastCellPassElevation1 = Consts.NullHeight;
      LastCellPassElevation2 = Consts.NullHeight;
      DesignElev = Consts.NullHeight;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteFloat(LastCellPassElevation1);
      writer.WriteFloat(LastCellPassElevation2);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      LastCellPassElevation1 = reader.ReadFloat();
      LastCellPassElevation2 = reader.ReadFloat();
    }

    /// <summary>
    /// Summary volume cells can be calculated be comparison of a cell pass elevation and a design elevation,
    /// or by comparison of two cell pass elevations together.
    /// </summary>
    /// <returns></returns>
    public override bool IsNull() => !((DesignElev != Consts.NullHeight && LastCellPassElevation1 != Consts.NullHeight) ||
                                       (LastCellPassElevation1 != Consts.NullHeight && LastCellPassElevation2 != Consts.NullHeight));
  }
}
