﻿using System;
using VSS.TRex.Common.Types;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Events
{
  public class ProductionEventsFactory : IProductionEventsFactory
  {
    /// <summary>
    /// Create an event list of the requested type
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="eventType"></param>
    /// <param name="machineID"></param>
    /// <returns></returns>
    public IProductionEvents NewEventList(short machineID, Guid siteModelID, ProductionEventType eventType)
    {
      switch (eventType)
      {
        case ProductionEventType.TargetCCV: return new ProductionEvents<short>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt16());
        case ProductionEventType.TargetPassCount: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16());

        case ProductionEventType.MachineMapReset: return null; //throw new NotImplementedException("ProductionEventType.MachineMapReset not implemented");

        case ProductionEventType.TargetLiftThickness: return new ProductionEvents<float>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadSingle());
        case ProductionEventType.GPSModeChange: return new ProductionEvents<GPSMode>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (GPSMode)r.ReadByte());
        case ProductionEventType.VibrationStateChange: return new ProductionEvents<VibrationState>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (VibrationState)r.ReadByte());
        case ProductionEventType.AutoVibrationStateChange: return new ProductionEvents<AutoVibrationState>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (AutoVibrationState)r.ReadByte());
        case ProductionEventType.MachineGearChange: return new ProductionEvents<MachineGear>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (MachineGear)r.ReadByte());
        case ProductionEventType.MachineAutomaticsChange: return new ProductionEvents<MachineAutomaticsMode>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (MachineAutomaticsMode)r.ReadByte());
        case ProductionEventType.MachineRMVJumpValueChange: return new ProductionEvents<short>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt16());
        case ProductionEventType.ICFlagsChange: return new ProductionEvents<byte>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadByte());
        case ProductionEventType.ElevationMappingModeStateChange: return new ProductionEvents<ElevationMappingMode>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (ElevationMappingMode)r.ReadByte());

        case ProductionEventType.GPSAccuracyChange:
          return new ProductionEvents<GPSAccuracyAndTolerance>(machineID, siteModelID, eventType,
            (w, s) => { w.Write(s.GPSTolerance); w.Write((byte)s.GPSAccuracy); },
            r => new GPSAccuracyAndTolerance((GPSAccuracy)r.ReadByte(), r.ReadUInt16()));

        case ProductionEventType.PositioningTech: return new ProductionEvents<PositioningTech>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (PositioningTech)r.ReadByte());
        case ProductionEventType.TempWarningLevelMinChange: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16());
        case ProductionEventType.TempWarningLevelMaxChange: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16());
        case ProductionEventType.TargetMDP: return new ProductionEvents<short>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt16());
        case ProductionEventType.LayerID: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16());

        case ProductionEventType.DesignOverride: return null; //throw new NotImplementedException("ProductionEventType.DesignOverride not implemented");
        case ProductionEventType.LayerOverride: return null; // throw new NotImplementedException("ProductionEventType.LayerOverride not implemented");

        case ProductionEventType.TargetCCA: return new ProductionEvents<byte>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadByte());
        case ProductionEventType.StartEndRecordedData: return new StartEndProductionEvents(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (ProductionEventType)r.ReadByte());
        case ProductionEventType.MachineStartupShutdown: return new StartEndProductionEvents(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (ProductionEventType)r.ReadByte());

        case ProductionEventType.DesignChange: return new ProductionEvents<int>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt32());
        default: return null;
      }

    }
  }
}
