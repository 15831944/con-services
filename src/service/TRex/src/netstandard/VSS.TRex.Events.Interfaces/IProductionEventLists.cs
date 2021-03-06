﻿using VSS.MasterData.Models.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Events.Models;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Events.Interfaces
{
  public interface IProductionEventLists
  {
    /// <summary>
    /// The ID of the machine these events were recorded by. The ID is the (short) internal machine ID
    /// used within the data model, not the GUID descriptor for the machine
    /// </summary>
    short InternalSiteModelMachineIndex { get; set; }

    IStartEndProductionEvents MachineStartupShutdownEvents { get; }

    /// <summary>
    /// Events recording the Start and Stop events for recording production data on a machine
    /// </summary>
    IStartEndProductionEvents StartEndRecordedDataEvents { get; }

    /// <summary>
    /// Events recording vibration state changes for vibratory drum compactor operation
    /// </summary>
    IProductionEvents<VibrationState> VibrationStateEvents { get; }

    /// <summary>
    /// Events recording automatics vibration state changes for vibratory drum compactor operation
    /// </summary>
    IProductionEvents<AutoVibrationState> AutoVibrationStateEvents { get; }

    /// <summary>
    /// Events recording changes to the prevailing GPSMode (eg: RTK Fixed, RTK Float, Differential etc) at the time 
    /// production measurements were being made
    /// </summary>
    IProductionEvents<GPSMode> GPSModeStateEvents { get; }

    /// <summary>
    /// Records the positioning technology (eg: GPS or UTS) being used at the time production measurements were being made
    /// </summary>
    IProductionEvents<PositioningTech> PositioningTechStateEvents { get; }

    /// <summary>
    /// Records the IDs of the designs selected on a machine at the time production measurements were being made
    /// </summary>
    IProductionEvents<int> MachineDesignNameIDStateEvents { get; }

    /// <summary>
    /// Records the state of the automatic machine control on the machine at the time measurements were being made.
    /// </summary>
    IProductionEvents<AutomaticsType> MachineAutomaticsStateEvents { get; }

    /// <summary>
    /// Records the state of the selected machine gear at the time measurements were being made
    /// </summary>
    IProductionEvents<MachineGear> MachineGearStateEvents { get; }

    /// <summary>
    /// Records the state of minimum elevation mapping on the machine at the time measurements were being made
    /// </summary>
    IProductionEvents<ElevationMappingMode> ElevationMappingModeStateEvents { get; }

    /// <summary>
    /// Records the state of GPSAccuracy and accompanying GPSTolerance on the machine at the time measurements were being made
    /// </summary>
    IProductionEvents<GPSAccuracyAndTolerance> GPSAccuracyAndToleranceStateEvents { get; }

    /// <summary>
    /// Records the selected Layer ID on the machine at the time measurements were being made
    /// </summary>
    IProductionEvents<ushort> LayerIDStateEvents { get; }
    
    /// <summary>
    /// ICFlags control flags change events
    /// </summary>
    IProductionEvents<byte> ICFlagsStateEvents { get; }

    /// <summary>
    /// Records the target CCV value configured on the machine control system
    /// </summary>
    IProductionEvents<short> TargetCCVStateEvents { get; }

    /// <summary>
    /// Records the target CCA value configured on the machine control system
    /// </summary>
    IProductionEvents<byte> TargetCCAStateEvents { get; }

    /// <summary>
    /// Records the target MDP value configured on the machine control system
    /// </summary>
    IProductionEvents<short> TargetMDPStateEvents { get; }

    /// <summary>
    /// Records the target MDP value configured on the machine control system
    /// </summary>
    IProductionEvents<ushort> TargetPassCountStateEvents { get; }

    /// <summary>
    /// Records the target minimum temperature value configured on the machine control system
    /// </summary>
    IProductionEvents<ushort> TargetMinMaterialTemperature { get; }

    /// <summary>
    /// Records the target maximum temperature value configured on the machine control system
    /// </summary>
    IProductionEvents<ushort> TargetMaxMaterialTemperature { get; }

    /// <summary>
    /// Records the target lift thickness value configured on the machine control system
    /// </summary>
    IProductionEvents<float> TargetLiftThicknessStateEvents { get; }

    /// <summary>
    /// Records the Resonance Meter Value jump threshold configured on the machine control system
    /// </summary>
    IProductionEvents<short> RMVJumpThresholdEvents { get; }

    /// <summary>
    /// Records the selected Layer ID overriding the id on the machine at the time measurements were being made
    /// </summary>
    IProductionEvents<OverrideEvent<ushort>> LayerOverrideEvents { get; }

    /// <summary>
    /// Records the selected Design overriding the design on the machine at the time measurements were being made
    /// </summary>
    IProductionEvents<OverrideEvent<int>> DesignOverrideEvents { get; }

    /// <summary>
    /// Returns an array containing all the event lists for a machine
    /// </summary>
    /// <returns></returns>
    IProductionEvents[] GetEventLists();

    /// <summary>
    /// Loads a specific event list into the events lists for a machine and returns a reference to it
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns></returns>
    IProductionEvents GetEventList(ProductionEventType eventType);

    /// <summary>
    /// Saves the event lists for this machine to the persistent store
    /// </summary>
    void SaveMachineEventsToPersistentStore(IStorageProxy storageProxy);

    /// <summary>
    /// Saves the event lists for this machine to the persistent store
    /// </summary>
    void RemoveMachineEventsFromPersistentStore(IStorageProxy storageProxy, bool exceptOverrideEvents);

    bool LoadEventsForMachine(IStorageProxy storageProxy);

    /// <summary>
    /// Provides a reference to the TAG file processing start/end events list
    /// </summary>
    /// <returns></returns>
    IProductionEventPairs GetStartEndRecordedDataEvents();
  }
}
