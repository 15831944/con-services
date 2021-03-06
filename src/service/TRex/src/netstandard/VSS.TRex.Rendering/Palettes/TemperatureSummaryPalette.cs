﻿using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Rendering.Palettes
{
  public class TemperatureSummaryPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const ushort TEMPERATURE_LEVELS_DEFAULT_MIN = 10;
    private const ushort TEMPERATURE_LEVELS_DEFAULT_MAX = 150;

    /// <summary>
    /// The color, which Material Temperature summary data displayed in on a plan view map, where material memperature values are greater than temperature warning levels.
    /// </summary>
    public Color AboveMaxLevelColour = Color.Red;

    /// <summary>
    /// The color, which Material Temperature summary data displayed in on a plan view map, where material temperature values are within temperature warning levels.
    /// </summary>
    public Color WithinLevelsColour = Color.Lime;

    /// <summary>
    /// The color, which Material Temperature summary data displayed in on a plan view map, where material temperature values are less than temperature warning levels.
    /// </summary>
    public Color BelowMinLevelColour = Color.Blue;

    /// <summary>
    /// The flag is to indicate whether or not the machine temperature warning levels range to be user overrides.
    /// </summary>
    public bool UseMachineTempWarningLevels = false;

    /// <summary>
    /// Pass Count target range.
    /// </summary>
    public TemperatureWarningLevelsRecord TemperatureLevels = new TemperatureWarningLevelsRecord(TEMPERATURE_LEVELS_DEFAULT_MIN, TEMPERATURE_LEVELS_DEFAULT_MAX);

    /// <summary>
    /// Simple palette for rendering raw Material Temperature summary data
    /// </summary>
    public TemperatureSummaryPalette() : base(null)
    {
      // ...
    }

    public Color ChooseColour(ushort measuredTemperature, ushort minTemperatureLevel, ushort maxTemperatureLevel)
    {
      if (!UseMachineTempWarningLevels)
      {
        minTemperatureLevel = TemperatureLevels.Min;
        maxTemperatureLevel = TemperatureLevels.Max;
      }

      if (minTemperatureLevel == CellPassConsts.NullMaterialTemperatureValue || maxTemperatureLevel == CellPassConsts.NullMaterialTemperatureValue)
        return Color.Empty;

      if (measuredTemperature > maxTemperatureLevel)
        return AboveMaxLevelColour;

      if (measuredTemperature < minTemperatureLevel)
        return BelowMinLevelColour;

      return WithinLevelsColour;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(AboveMaxLevelColour.ToArgb());
      writer.WriteInt(WithinLevelsColour.ToArgb());
      writer.WriteInt(BelowMinLevelColour.ToArgb());

      writer.WriteBoolean(UseMachineTempWarningLevels);

      TemperatureLevels.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        AboveMaxLevelColour = Color.FromArgb(reader.ReadInt());
        WithinLevelsColour = Color.FromArgb(reader.ReadInt());
        BelowMinLevelColour = Color.FromArgb(reader.ReadInt());

        UseMachineTempWarningLevels = reader.ReadBoolean();

        TemperatureLevels.FromBinary(reader);
      }
    }
  }
}
