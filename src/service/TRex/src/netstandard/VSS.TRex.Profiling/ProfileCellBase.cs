﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  public abstract class ProfileCellBase : IProfileCellBase
  {
    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    public double Station { get; set; }

    /// <summary>
    /// The real-world length of that part of the profile line which crosses the underlying cell;
    /// used to determine the width of the profile column as displayed in the client GUI
    /// </summary>
    public double InterceptLength { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public int OTGCellX { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public int OTGCellY { get; set; }

    public float DesignElev { get; set; }

    public abstract bool IsNull();

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public virtual void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Station);
      writer.WriteDouble(InterceptLength);

      writer.WriteInt(OTGCellX);
      writer.WriteInt(OTGCellY);

      writer.WriteFloat(DesignElev);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public virtual void FromBinary(IBinaryRawReader reader)
    {
      Station = reader.ReadDouble();
      InterceptLength = reader.ReadDouble();

      OTGCellX = reader.ReadInt();
      OTGCellY = reader.ReadInt();

      DesignElev = reader.ReadFloat();
    }
  }
}
