﻿using System;
using VSS.TRex.Types;

namespace VSS.TRex.Common.CellPasses
{
  public static class CellPassConsts
  {
    /// <summary>
    /// Null GPS tolerance value
    /// </summary>
    public const ushort NullGPSTolerance = ushort.MaxValue;

    /// <summary>
    /// Null machine speed value
    /// </summary>
    public const ushort NullMachineSpeed = Consts.NullMachineSpeed;

    /// <summary>
    /// Null Pass Count value
    /// </summary>
    public const ushort NullPassCountValue = ushort.MinValue;

    /// <summary>
    /// Conversion ratio between temperature in whole degrees and tenths of degrees reported by some measurements from machines
    /// </summary>
    public const short MaterialTempValueRatio = 10;

    /// <summary>
    /// Value representing a minimum material temperature encoded as an IEEE ushort
    /// </summary>
    public const ushort MinMaterialTempValue = 0;

    /// <summary>
    /// Value representing a maximum temperature value, that may be reported, encoded as an IEEE ushort
    /// </summary>
    public const ushort MaxMaterialTempValue = 4095;

    // Value representing a null material temperature encoded as an IEEE ushort
    public const ushort NullMaterialTemperatureValue = MaxMaterialTempValue + 1;

    /// <summary>
    /// Null machine ID. This is the null site model machine reference ID, not the null Guid for machines
    /// </summary>
    //public const long NullMachineID = 0;
    public const short NullInternalSiteModelMachineIndex = short.MinValue;

    public static DateTime NullTime = DateTime.MinValue;

    /// <summary>
    /// Null GPSMode value
    /// </summary>
    public const GPSMode NullGPSMode = GPSMode.NoGPS;

    /// <summary>
    /// NUll height (NEE Elevation) value. This is an IEEE Single (Float) value
    /// </summary>
    public const float NullHeight = Consts.NullHeight;

    /// <summary>
    /// Null CCV value
    /// </summary>
    public const short NullCCV = short.MaxValue;

    /// <summary>
    /// Conversion ratio 
    /// </summary>
    public const short CCVvalueRatio = 10;

    /// <summary>
    /// Null value for reported CCV percentage
    /// </summary>
    public const int NullCCVPercentage = -1;

    /// <summary>
    /// Maximum Pass Count value
    /// </summary>
    public const ushort MaxPassCountValue = ushort.MaxValue;

    /// <summary>
    /// Null radio correction latency value
    /// </summary>
    public const byte NullRadioLatency = byte.MaxValue; // This is the same value as kSVOAsBuiltNullRadio

    /// <summary>
    /// Null Resonance Meter Value
    /// </summary>
    public const short NullRMV = short.MaxValue;

    /// <summary>
    /// Null vibratory drum vibration frequency value
    /// </summary>
    public const ushort NullFrequency = ushort.MaxValue;

    /// <summary>
    /// Null vibratory drum amplitude value
    /// </summary>
    public const ushort NullAmplitude = ushort.MaxValue;

    /// <summary>
    /// Conversion ratio 
    /// </summary>
    public const ushort AmplitudeRatio = 100;

    /// <summary>
    /// Null Machine Drive Power compaction value
    /// </summary>
    public const short NullMDP = short.MaxValue;

    /// <summary>
    /// Conversion ratio 
    /// </summary>
    public const short MDPvalueRatio = 10;

    /// <summary>
    /// Null value for reported MDP percentage
    /// </summary>
    public const int NullMDPPercentage = -1;

    /// <summary>
    /// Null Caterpillar Compaction Algorithm value
    /// </summary>
    public const byte NullCCA = byte.MaxValue;

    /// <summary>
    /// Null Caterpillar Compaction Algorithm target value
    /// </summary>
    public const byte NullCCATarget = byte.MaxValue;

    /// <summary>
    /// The CCA value, which a lift is marked as thick at.
    /// </summary>
    public const byte ThickLiftCCAValue = 120;

    /// <summary>
    /// Null machine type value
    /// </summary>
    public const MachineType MachineTypeNull = (MachineType)0;

    /// <summary>
    /// Null value for the Volkel compaction sensor measurement range (defined as int, but null is byte.MaxValue)
    /// </summary>
    public const int NullVolkelMeasRange = byte.MaxValue;

    /// <summary>
    /// The null value for the Volkel compaction machine measurement util range 
    /// </summary>
    public const int NullVolkelMeasUtilRange = -1;

    /// <summary>
    /// The null value for machine gear
    /// </summary>
    public const MachineGear NullMachineGear = MachineGear.Null;

    /// <summary>
    /// Null layer ID value
    /// </summary>
    public const ushort NullLayerID = ushort.MaxValue;

    /// <summary>
    /// Null 3D sonic sensor value
    /// </summary>
    public const byte Null3DSonic = byte.MaxValue;

    /// <summary>
    /// Null value for target lift thickness override value specified from a machine
    /// </summary>
    public const float NullOverridingTargetLiftThicknessValue = Consts.NullHeight;

    /// <summary>
    /// Null value for a Universal Transverse Mercator zone reference
    /// </summary>
    public const byte NullUTMZone = 0;

    /// <summary>
    /// The CCA value, which a lift is marked as thick at.
    /// </summary>
    public const byte THICK_LIFT_CCA_VALUE = 120;

    /// <summary>
    /// The mask to be applied to the GPSModeStore member of the cell pass to access the GPSMode enumeration value
    /// </summary>
    public const byte GPSModeStoreMask = 0b00001111;
  }
}
