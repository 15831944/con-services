﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Cells
{
    /// <summary>
    ///  Stores the values of various 'event' values at the time the pass information was captured.
    /// </summary>
    public struct CellEvents
    {
        /// <summary>
        /// Value to indicate a null design name ID
        /// </summary>
        public const int NoDesignNameID = 0;

        /// <summary>
        /// Null value for GPS Tolerance measurements
        /// </summary>
        public const short NullGPSTolerance = short.MaxValue;

        /// <summary>
        /// Null value for ID of Layer selected by the machine control system operator
        /// </summary>
        public const ushort NullLayerID = ushort.MaxValue;

        /// <summary>
        /// Is the machine performing minimum elevation mapping
        /// </summary>
        public bool EventMinElevMapping { get; set; }

        /// <summary>
        /// Is the machine is a defined avoisance zone
        /// </summary>
        public byte EventInAvoidZoneState { get; set; }

        /// <summary>
        /// The ID of the loaded design on the machine control system
        /// </summary>
        public int EventDesignNameID { get; set; }

        /// <summary>
        /// Compaction drum vibration state
        /// </summary>
        public VibrationState EventVibrationState { get; set; }

        /// <summary>
        /// Automatics control state for vibratory drums
        /// </summary>
        public AutoVibrationState EventAutoVibrationState { get; set; }

        /// <summary>
        /// Transmission gear the machine is in
        /// </summary>
        public MachineGear EventMachineGear { get; set; }

        /// <summary>
        /// Resonance Meter Threshold value above which decoupling exists at the drum/ground boundary
        /// </summary>
        public short EventMachineRMVThreshold { get; set; }

        /// <summary>
        /// Automatics controls state the machine control system is operating in
        /// </summary>
        public MachineAutomaticsMode EventMachineAutomatics { get; set; }

        /// <summary>
        /// Is the machine implementin contact with the ground, and how is this state determined
        /// </summary>
        public OnGroundState EventOnGroundState { get; set; }

        /// <summary>
        /// Positioning technology used to calculate machine position
        /// </summary>
        public PositioningTech PositioningTechnology { get; set; }

        /// <summary>
        /// GPS error tolerance metric published by machine control system
        /// </summary>
        public short GPSTolerance { get; set; }

        /// <summary>
        /// GPS accuracy band (find/medium/coarse) for positioning reported by the machine control system
        /// </summary>
        public GPSAccuracy GPSAccuracy { get; set; }

        /// <summary>
        /// The date at which the closest preceding Map Reset event was triggered on a machine control system
        /// </summary>
        public DateTime MapReset_PriorDate { get; set; }

        /// <summary>
        /// The ID of the design loaded at the time of the closest preceding Map Reset event triggered on a machine control system
        /// </summary>
        public int MapReset_DesignNameID { get; set; }

        /// <summary>
        /// Layer ID selected by the machine control system operator
        /// </summary>
        public ushort LayerID { get; set; }

        /// <summary>
        /// Grouped flags related to compaction and earthworks state
        /// </summary>
        public byte EventFlags { get; set; }

        public void Clear()
        {
            EventDesignNameID = NoDesignNameID;
            EventVibrationState = VibrationState.Invalid;
            EventAutoVibrationState = AutoVibrationState.Unknown;
            EventFlags = 0;
            EventMachineGear = MachineGear.Neutral;
            EventMachineRMVThreshold = CellPass.NullRMV;
            EventMachineAutomatics = MachineAutomaticsMode.Unknown;
            EventMinElevMapping = false;
            EventInAvoidZoneState = 0;

            MapReset_PriorDate = CellPass.NullTime;
            MapReset_DesignNameID = NoDesignNameID;

            GPSAccuracy = GPSAccuracy.Unknown;
            GPSTolerance = NullGPSTolerance;
            PositioningTechnology = PositioningTech.Unknown;
            EventOnGroundState = OnGroundState.Unknown;

            LayerID = NullLayerID;
        }

        /// <summary>
        /// Assign the contents of one Cell Events instance to this instance
        /// </summary>
        /// <param name="source"></param>
        public void Assign(CellEvents source)
        {
            EventDesignNameID = source.EventDesignNameID;
            EventVibrationState = source.EventVibrationState;
            EventAutoVibrationState = source.EventAutoVibrationState;
            EventFlags = source.EventFlags;
            EventMachineGear = source.EventMachineGear;
            EventMachineRMVThreshold = source.EventMachineRMVThreshold;
            EventMachineAutomatics = source.EventMachineAutomatics;
            EventMinElevMapping = source.EventMinElevMapping;
            EventInAvoidZoneState = source.EventInAvoidZoneState;

            MapReset_PriorDate = source.MapReset_PriorDate;
            MapReset_DesignNameID = source.MapReset_DesignNameID;

            GPSAccuracy = source.GPSAccuracy;
            GPSTolerance = source.GPSTolerance;
            PositioningTechnology = source.PositioningTechnology;
            EventOnGroundState = source.EventOnGroundState;

            LayerID = source.LayerID;
        }
    }
}
