﻿using System.Runtime.CompilerServices;

namespace VSS.TRex.Common.Utilities
{
  /// <summary>
  /// Useful helpers for manipulating bits
  /// </summary>
  public static class BitFlagHelper
  {
    /// <summary>
    /// [Byte] Set bit 'theBit' to on (1) in the passed value, returning the result.
    /// Note: Bit 0 is the least significant bit)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte BitOn(byte value, int theBit) => (byte)(value | (1 << theBit));

    /// <summary>
    /// [Byte] Clear bit 'theBit' to off (0) in the passed value, returning the result
    /// Note: Bit 0 is the least significant bit)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte BitOff(byte value, int theBit) => (byte)(value & ~(1 << theBit));

    /// <summary>
    /// [Byte] Set bit 'theBit' to on (1) in the passed value, returning the result.
    /// Note: Bit 0 is the least significant bit)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort BitOn(ushort value, int theBit) => (ushort)(value | (1 << theBit));

    /// <summary>
    /// [Byte] Clear bit 'theBit' to off (0) in the passed value, returning the result
    /// Note: Bit 0 is the least significant bit)
    /// </summary>
    public static ushort BitOff(ushort value, int theBit) => (ushort)(value & ~(1 << theBit));

    /// <summary>
    /// Set bit 'theBit' in value to 0 or 1 depending on the false or true state respectively of SetTo1
    /// Note: Bit 0 is the least significant bit)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(ref byte value, int theBit, bool SetTo1) => value = (byte)(SetTo1 ? value | (1 << theBit) : value & ~(1 << theBit));

    /// <summary>
    /// Determine if the nth bit in value (where bit 0 is the least significant bit) is set to 1
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitOn(int value, int theBit) => (value & (1 << theBit)) != 0;

    /// <summary>
    /// Determine if the nth bit in value (where bit 0 is the least significant bit) is set to 0
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitOff(int value, int theBit) => (value & (1 << theBit)) == 0;
  }
}
