﻿using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Types
{
	/// <summary>
	/// Contains measured temperature value as well as minimum and maximum temperature warning level values.
	/// </summary>
	public struct SubGridCellPassDataTemperatureEntryRecord
  {
    /// <summary>
    /// Measured temperature value.
    /// </summary>
    public ushort MeasuredTemperature { get; set; }

    /// <summary>
    /// Temperature warning levels.
    /// </summary>
    public TemperatureWarningLevelsRecord TemperatureLevels;

		/// <summary>
		/// Constractor with arguments.
		/// </summary>
		/// <param name="measuredTemperature"></param>
		/// <param name="temperatureLevels"></param>
		public SubGridCellPassDataTemperatureEntryRecord(ushort measuredTemperature, TemperatureWarningLevelsRecord temperatureLevels)
	  {
		  MeasuredTemperature = measuredTemperature;
		  TemperatureLevels = temperatureLevels;
	  }

		/// <summary>
		/// Initialises the measured temperature and its warning leveles with null values.
		/// </summary>
		public void Clear()
	  {
		  MeasuredTemperature = CellPass.NullMaterialTemperatureValue;
			TemperatureLevels.Clear();
	  }

    /// <summary>
    /// Defines a publically accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataTemperatureEntryRecord NullValue = SubGridCellPassDataTemperatureEntryRecord.Null();

    /// <summary>
    /// Implements the business logic to create the null value for this cell valuye type
    /// </summary>
    /// <returns></returns>
    public static SubGridCellPassDataTemperatureEntryRecord Null()
    {
      SubGridCellPassDataTemperatureEntryRecord Result = new SubGridCellPassDataTemperatureEntryRecord();
      Result.Clear();
      return Result;
    }

    /// <summary>
    /// Serialises content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredTemperature);
      TemperatureLevels.Write(writer);
    }

    /// <summary>
    /// Serialises comtent of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredTemperature = reader.ReadUInt16();
      TemperatureLevels.Read(reader);
    }

    public bool Equals(SubGridCellPassDataTemperatureEntryRecord other)
    {
      return MeasuredTemperature == other.MeasuredTemperature && TemperatureLevels.Equals(other.TemperatureLevels);
    }
  }
}
