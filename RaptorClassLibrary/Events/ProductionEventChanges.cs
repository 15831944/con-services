﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Events.Interfaces;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// A wrapper for all the event information related to a particular machine's activities within a particular
    /// site model.
    /// </summary>
    public class ProductionEventChanges
    {
        /// <summary>
        /// The SiteModel these events relate to
        /// </summary>
        SiteModel SiteModel { get; set; } = null;

        /// <summary>
        /// The ID of the machine these events were recorded by
        /// </summary>
        public long MachineID { get; set; } = 0;

        /// <summary>
        /// Events recording the Start and Stop events for recording production data on a machine
        /// </summary>
        /// 
        public StartEndRecordedDataChangeList StartEndRecordedDataEvents = null;
        
        /// <summary>
        /// Events recording vibration state changes for vibratory drum compactor operation
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<VibrationState>, VibrationState> VibrationStateEvents = null;

        /// <summary>
        /// Events recording changes to the prevailing GPSMode (eg: RTK Fixed, RTK Float, Differential etc) at the time 
        /// production measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<GPSMode>, GPSMode> GPSModeStateEvents = null;

        /// <summary>
        /// Records the positioning technology (eg: GPS or UTS) being used at the time 
        /// production measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<PositioningTech>, PositioningTech> PositioningTechStateEvents = null;

        /// <summary>
        /// Records the IDs of the designs selected on a machine at the time production measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<int>, int> DesignNameIDStateEvents = null;

        /// <summary>
        /// Records the state of the automatic machine control on the machine at the time measurements were being made.
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode> MachineAutomaticsStateEvents = null;

        /// <summary>
        /// Records the state of the selected machine gear at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<MachineGear>, MachineGear> MachineGearStateEvents = null;

        /// <summary>
        /// Records the state of minimum elevation mapping on the machine at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<bool>, bool> MinElevMappingStateEvents = null;

        /// <summary>
        /// Records the state of GPSAccuracy and accompanying GPSTolerance on the machine at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance> GPSAccuracyAndToleranceStateEvents = null;

        /// <summary>
        /// Records the selected Layer ID on the machine at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort> LayerIDStateEvents = null;

        /// <summary>
        /// Create all defined event lists in one operation.
        /// </summary>
        private void CreateEventLists()
        {
            StartEndRecordedDataEvents = new StartEndRecordedDataChangeList(MachineID, SiteModel.ID, ProductionEventType.StartRecordedData);
            VibrationStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<VibrationState>, VibrationState>(MachineID, SiteModel.ID, ProductionEventType.VibrationStateChange);
            GPSModeStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<GPSMode>, GPSMode>(MachineID, SiteModel.ID, ProductionEventType.GPSModeChange);
            PositioningTechStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<PositioningTech>, PositioningTech>(MachineID, SiteModel.ID, ProductionEventType.PositioningTech);
            DesignNameIDStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<int>, int>(MachineID, SiteModel.ID, ProductionEventType.DesignChange);
            MachineAutomaticsStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode>(MachineID, SiteModel.ID, ProductionEventType.MachineAutomaticsChange);
            MachineGearStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<MachineGear>, MachineGear>(MachineID, SiteModel.ID, ProductionEventType.MachineGearChange);
            MinElevMappingStateEvents = new ProductionEventChangeList< ProductionEventChangeBase<bool>, bool>(MachineID, SiteModel.ID, ProductionEventType.MinElevMappingStateChange);
            GPSAccuracyAndToleranceStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance>(MachineID, SiteModel.ID, ProductionEventType.GPSAccuracyChange);
            LayerIDStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort>(MachineID, SiteModel.ID, ProductionEventType.LayerID);
       }

        /// <summary>
        /// Returns an array containing all the event lists for a machine
        /// </summary>
        /// <returns></returns>
        public IProductionEventChangeList[] GetEventLists()
        {
            return new IProductionEventChangeList[]
            {
                StartEndRecordedDataEvents,
                VibrationStateEvents,
                GPSModeStateEvents,
                PositioningTechStateEvents,
                DesignNameIDStateEvents,
                MachineAutomaticsStateEvents,
                MachineGearStateEvents,
                MinElevMappingStateEvents,
                GPSAccuracyAndToleranceStateEvents,
                LayerIDStateEvents
            };
        }
    /// <summary>
    /// Primary constructor for events recorded by a single machine within a single site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="machineID"></param>
    public ProductionEventChanges(SiteModel siteModel, long machineID)
        {
            //  FFileSystem := Nil;
            SiteModel = siteModel;

            MachineID = machineID;

            CreateEventLists();
        }

        /// <summary>
        /// Saves the event lists for this machine to the persistent store
        /// </summary>
        /// <param name="storageProxy"></param>
        public void SaveMachineEventsToPersistentStore(IStorageProxy storageProxy)
        {
            foreach (IProductionEventChangeList list in GetEventLists())
            {
                list.SaveToStore(storageProxy);
            }
        }

        public bool LoadEventsForMachine(IStorageProxy storageProxy)
        {
            StartEndRecordedDataEvents = StartEndRecordedDataEvents.LoadFromStore(storageProxy) as StartEndRecordedDataChangeList;
            VibrationStateEvents = VibrationStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<VibrationState>, VibrationState>;
            GPSModeStateEvents = GPSModeStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<GPSMode>, GPSMode>;
            PositioningTechStateEvents = PositioningTechStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<PositioningTech>, PositioningTech>;
            DesignNameIDStateEvents = DesignNameIDStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<int>, int>;
            MachineAutomaticsStateEvents = MachineAutomaticsStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode>;
            MachineGearStateEvents = MachineGearStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<MachineGear>, MachineGear>;
            MinElevMappingStateEvents = MinElevMappingStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<bool>, bool>;
            GPSAccuracyAndToleranceStateEvents = GPSAccuracyAndToleranceStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance>;
            LayerIDStateEvents = LayerIDStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort>;

            return true;
        }
    }
}
