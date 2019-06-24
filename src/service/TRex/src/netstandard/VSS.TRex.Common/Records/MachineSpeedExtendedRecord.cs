﻿using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.CellPasses;

namespace VSS.TRex.Common.Records
{
	/// <summary>
	/// Contains minimum and maximum machine speed values.
	/// </summary>
	public struct MachineSpeedExtendedRecord
  {
    /// <summary>
    /// Minimum machine speed value.
    /// </summary> 
    public ushort Min;

    /// <summary>
    /// Maximum machine speed value.
    /// </summary>
    public ushort Max;

	  /// <summary>
	  /// Return an indicative size for memory consumption of this class to be used in cache tracking
	  /// </summary>
	  /// <returns></returns>
	  public static int IndicativeSizeInBytes() => 2 * sizeof(ushort);

	  /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public MachineSpeedExtendedRecord(ushort min, ushort max)
		{
			Min = min;
			Max = max;
		}

	  /// <summary>
	  /// Initialises the Min and Max properties with null values.
	  /// </summary>
	  public void Clear()
	  {
	    Min = CellPassConsts.NullMachineSpeed;
	    Max = CellPassConsts.NullMachineSpeed;
	  }

	  /// <summary>
	  /// Initialises the Min and Max properties with values.
	  /// </summary>
	  /// <param name="min"></param>
	  /// <param name="max"></param>
	  public void SetMinMax(ushort min, ushort max)
	  {
	    Min = min;
	    Max = max;
	  }

    /// <summary>
    /// Serialises content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
	  {
	    writer.Write(Min);
	    writer.Write(Max);
	  }

    /// <summary>
    /// Serialises comtent of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
	  {
	    Min = reader.ReadUInt16();
	    Max = reader.ReadUInt16();
	  }

	  /// <summary>
	  /// Serialises content of the cell to the writer
	  /// </summary>
	  /// <param name="writer"></param>
	  public void ToBinary(IBinaryRawWriter writer)
	  {
	    writer.WriteInt(Min);
	    writer.WriteInt(Max);
	  }

	  /// <summary>
	  /// Serialises content of the cell from the writer
	  /// </summary>
	  /// <param name="reader"></param>
	  public void FromBinary(IBinaryRawReader reader)
	  {
	    Min = (ushort)reader.ReadInt();
	    Max = (ushort)reader.ReadInt();
	  }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static MachineSpeedExtendedRecord NullValue = MachineSpeedExtendedRecord.Null();

	  /// <summary>
	  /// Implements the business logic to create the null value for this cell value type
	  /// </summary>
	  /// <returns></returns>
	  public static MachineSpeedExtendedRecord Null()
	  {
	    MachineSpeedExtendedRecord Result = new MachineSpeedExtendedRecord();
	    Result.Clear();
	    return Result;
	  }

	  public bool Equals(MachineSpeedExtendedRecord other)
	  {
	    return Min == other.Min && Max == other.Max;
	  }
  }
}
