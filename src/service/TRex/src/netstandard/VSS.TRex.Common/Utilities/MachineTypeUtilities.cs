﻿using VSS.TRex.Types;

namespace VSS.TRex.Common.Utilities
{
  /// <summary>
  /// Utilities relating to the machine types supported
  /// </summary>
  public static class MachineTypeUtilities
  {
    /// <summary>
    /// Notes if the pass counting basis for the machine type in in terms of 'half passes', ie: one half pass per tracked axle of the machine
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsHalfPassCompactorMachine(MachineType type) => type == MachineType.FourDrumLandfillCompactor;
  }
}
