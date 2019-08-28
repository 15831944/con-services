﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Cells
{
  /// <summary>
  /// Stores the values of various 'target' values that were active on the machine at the time the pass information was captured.
  /// </summary>
  public struct CellTargets
  {
    /// <summary>
    /// Null value for the CCA target configured at the time of cell pass measurement
    /// </summary>
    public const byte NullCCATarget = byte.MaxValue;

    /// <summary>
    /// Null value for the pass count target configured at the time of cell pass measurement
    /// </summary>
    public const ushort NullPassCountTarget = 0;

    /// <summary>
    /// Null value for the override target lift thickness value to be used in place of the target lift thickness value
    /// configured at the time of cell pass measurement
    /// </summary>
    public const float NullOverridingTargetLiftThicknessValue = Consts.NullSingle;

    /// <summary>
    /// Target Compaction Meter Value at the time a cell pass was recorded
    /// </summary>
    public short TargetCCV;

    /// <summary>
    ///  Target Machine Drive Power at the time a cell pass was recorded
    /// </summary>
    public short TargetMDP;

    /// <summary>
    /// Target material layer thickness at the time a cell pass was recorded
    /// </summary>
    public float TargetLiftThickness;

    /// <summary>
    /// Target machine pass count at the time a cell pass was recorded
    /// </summary>
    public ushort TargetPassCount;

    /// <summary>
    /// Target minimum temperature sensor warning level at the time a cell pass was recorded
    /// </summary>
    public ushort TempWarningLevelMin;

    /// <summary>
    /// Target maximum temperature sensor warning level at the time a cell pass was recorded
    /// </summary>
    public ushort TempWarningLevelMax;

    /// <summary>
    /// Target Caterpillar Compaction algorithm value at the time a cell pass was recorded
    /// </summary>
    public byte TargetCCA;

    /// <summary>
    /// Set all state in this structure to null values
    /// </summary>
    public void Clear()
    {
      TargetCCV = CellPassConsts.NullCCV;
      TargetMDP = CellPassConsts.NullMDP;
      TargetLiftThickness = NullOverridingTargetLiftThicknessValue;
      TargetPassCount = NullPassCountTarget;
      TempWarningLevelMin = CellPassConsts.NullMaterialTemperatureValue;
      TempWarningLevelMax = CellPassConsts.NullMaterialTemperatureValue;
      TargetCCA = NullCCATarget;
    }

    /// <summary>
    /// Assigns the contents of a CellTargets to this CellTargets
    /// </summary>
    /// <param name="source"></param>
    public void Assign(CellTargets source)
    {
      TargetCCA = source.TargetCCA;
      TargetCCV = source.TargetCCV;
      TargetMDP = source.TargetMDP;
      TargetPassCount = source.TargetPassCount;
      TargetLiftThickness = source.TargetLiftThickness;
      TempWarningLevelMax = source.TempWarningLevelMax;
      TempWarningLevelMin = source.TempWarningLevelMin;
    }

    //Procedure ReadFromStream(const Stream : TStream);
    //Procedure WriteToStream(const Stream : TStream);

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteShort(TargetCCV);
      writer.WriteShort(TargetMDP);
      writer.WriteFloat(TargetLiftThickness);
      writer.WriteInt(TargetPassCount);
      writer.WriteInt(TempWarningLevelMin);
      writer.WriteInt(TempWarningLevelMax);
      writer.WriteByte(TargetCCA);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      TargetCCV = reader.ReadShort();
      TargetMDP = reader.ReadShort();
      TargetLiftThickness = reader.ReadFloat();
      TargetPassCount = (ushort)reader.ReadInt();
      TempWarningLevelMin = (ushort)reader.ReadInt();
      TempWarningLevelMax = (ushort)reader.ReadInt();
      TargetCCA = reader.ReadByte();
    }
  }
}
