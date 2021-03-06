﻿using System;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Types;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.OEM.Volvo;
using VSS.TRex.TAGFiles.Classes.Swather;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.TAGFiles.Classes.Processors
{
  /// <summary>
  /// Coordinates reading and converting recorded information from compaction machines into the IC server database.
  /// </summary>
  public class TAGProcessor : TAGProcessorBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGProcessor>();

    public TAGProcessor()
    {
    }

    /// <summary>
    /// Primary constructor for the TAGProcessor. The arguments passed to it are:
    /// 1. The target SiteModel which is intended to be the recipient of the TAG information processed 
    /// 2. The target Machine in the site model which recorded the TAG information
    /// 3. The event lists related to the target machine in the target site model
    /// 4. A subgrid tree representing the aggregator for all the spatial cell pass information processed
    ///    from the TAG information as an independent entity.
    /// 5. A set of event lists representing the aggregator for all the machine events for the target machine
    ///    in the target site model that were processed from the TAG information as a separate entity.
    /// </summary>
    /// <param name="targetSiteModel"></param>
    /// <param name="targetMachine"></param>
    /// <param name="siteModelGridAggregator"></param>
    /// <param name="machineTargetValueChangesAggregator"></param>
    public TAGProcessor(ISiteModel targetSiteModel,
        IMachine targetMachine,
        IServerSubGridTree siteModelGridAggregator,
        ProductionEventLists machineTargetValueChangesAggregator) : this()
    {
      SiteModel = targetSiteModel;
      Machine = targetMachine;

      SiteModelGridAggregator = siteModelGridAggregator;
      MachineTargetValueChangesAggregator = machineTargetValueChangesAggregator;
      //            MachineTargetValueChangesAggregator.MarkAllEventListsAsInMemoryOnly;
    }

    // SiteModel is the site model that the read data is being contributed to
    public ISiteModel SiteModel { get; }

    // SiteModelAggregator is the site model that the read data is aggregated into
    // prior to being integrated into the model represented by SiteModel
    // This serves two functions:
    // 1. Performance: Cell passes and events are added to the relevant stores en-masse which
    //    is much faster than in piecemeal fashion.
    // 2. Contention: This reduces contention for the primary server interface lock between
    //    the tag file processor and client applications. }
    public IServerSubGridTree SiteModelGridAggregator { get; }

    // Machine is a reference to the intelligent compaction machine that
    // has collected the data being processed.
    private IMachine Machine;

    // FICMachineTargetValueChangesAggregator is an object that aggregates all the
    // machine state events of interest that we encounter while processing the
    // file. These are then integrated into the machine events in a single step
    // at a later point in processing
    public ProductionEventLists MachineTargetValueChangesAggregator { get; }

    /*
    // FOnProgressCheck provides a callback to the owner of the ST processing
    // currently underway. The owner may abort the processing by returning
    // false when the event is called.
    FOnProgressCheck : TSTProcessingProgressCheckEvent;

    // FOnAbortProcessing is an event that allows the processor to advise a third
    // party that the processing had been aborted.
    FOnAbortProcessing : TNotifyEvent; 
    */

    private DateTime TagFileStartTime = Consts.MIN_DATETIME_AS_UTC;
    private bool HasGPSModeBeenSet = false;

    /// <summary>
    /// EpochContainsProofingRunDescription determines if the current epoch
    /// contains a description of a proofing run.
    /// </summary>
    protected bool EpochContainsProofingRunDescription()
    {
      return (_StartProofingDataTime != Consts.MIN_DATETIME_AS_UTC) && // We have a start time for the run
             (_DataTime > _StartProofingDataTime); // The current epoch time is greater than the start
    }

    // If there has been sufficient information read in from the compaction
    // information file to identify a proofing pass made by the machine then
    // we process it here.
    protected bool ProcessProofingPassInformation()
    {
      string tempStr = _EndProofingName != string.Empty ?
        _EndProofingName : _Design == string.Empty ? "No Design" : _Design;

      DateTime localTime = _StartProofingDataTime + Common.Time.GPS.GetLocalGMTOffset();

      EndProofingName = $"{tempStr} ({localTime:yyyy/MM/dd} {localTime:HH:mm:ss})";

      // Create and add a new proofing run entry to represent this run.
      var result = SiteModel.SiteProofingRuns.CreateAndAddProofingRun(
                     _EndProofingName,
                     Machine.InternalSiteModelMachineIndex,
                     _StartProofingDataTime,
                     _DataTime,
                     ProofingRunExtent);

      ProofingRunExtent.SetInverted();

      return result;
    }

    /// <summary>
    /// At every epoch we process a set of state information that has been set into
    /// this processor by the active tag file value sink. Most of this information
    /// persists between epochs. However, some of this information is cleared at
    /// the end of each epoch. ClearEpochSpecificData performs this operation
    /// and is called at the end of ProcessEpochContext()
    /// </summary>
    protected override void ClearEpochSpecificData()
    {
      base.ClearEpochSpecificData();
    }

    private SwatherBase CreateSwather(Fence InterpolationFence)
    {
      // Decide which swather to create. 
       if (Machine.MachineType == MachineType.CutterSuctionDredge)
      {
        return new CSDSwather(this,
            MachineTargetValueChangesAggregator,
            SiteModel,
            SiteModelGridAggregator,
            InterpolationFence)
        {
          ProcessedEpochNumber = ProcessedEpochCount
        };
      }
      else
      {
        return new TerrainSwather(this,
            MachineTargetValueChangesAggregator,
            SiteModel,
            SiteModelGridAggregator,
            InterpolationFence)
        {
          ProcessedEpochNumber = ProcessedEpochCount
        }; 
      }
    }

    protected override void SetDataTime(DateTime Value)
    {
      bool RecordEvent = _DataTime == Consts.MIN_DATETIME_AS_UTC;

      base.SetDataTime(Value);

      if (RecordEvent)
      {
        MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.PutValueAtDate(Value,
            ProductionEventType.StartEvent);

        TagFileStartTime = Value;
      }
    }

    protected override void SetDesign(string Value)
    {
      // If the design being loaded changed, then update the extents of the design
      // in the designs list in the site model

      if (_Design != string.Empty && _Design != Value)
        UpdateCurrentDesignExtent();

      base.SetDesign(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
      {
        Value = Value.Trim();
        var siteModelMachineDesign = SiteModel.SiteModelMachineDesigns.Locate(Value);
        if (siteModelMachineDesign == null)
        {
          siteModelMachineDesign = SiteModel.SiteModelMachineDesigns.CreateNew(Value);
        }

        MachineTargetValueChangesAggregator.MachineDesignNameIDStateEvents.PutValueAtDate(_DataTime, siteModelMachineDesign.Id);
      }

      // Get the current design extent for the newly selected design
      SelectCurrentDesignExtent();
    }

    protected void SelectCurrentDesignExtent()
    {
      int DesignIndex = SiteModel.SiteModelDesigns.IndexOf(_Design);

      if (DesignIndex == -1)
      {
        // This may be because there is no selected design name, or that the
        // entry for this named design is not in the list. If the former, just clear the
        // design extents. If the latter, create a new design extents entry

        // Clear the design extent being maintained in the processor.
        DesignExtent.SetInverted();

        if (_Design != string.Empty)
          SiteModel.SiteModelDesigns.CreateNew(_Design, DesignExtent);
      }
      else
        DesignExtent = SiteModel.SiteModelDesigns[DesignIndex].Extents;
    }

    /// <summary>
    /// Records a change in the 'ICMode' flags from the compaction system. These flags also drive two 
    /// other events: vibration events and automatics vibration events
    /// </summary>
    /// <param name="value"></param>
    protected override void SetICMode(byte value)
    {
      base.SetICMode(value);

      VibrationState TempVibrationState = VibrationState.Invalid;
      AutoVibrationState TempAutoVibrationState = AutoVibrationState.Unknown;

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
      {
        switch (_ICSensorType)
        {
          case CompactionSensorType.Volkel:
            {
              TempVibrationState = (value & ICModeFlags.IC_VOLKEL_SENSOR_VIBRATION_ON_MASK) == ICModeFlags.IC_VOLKEL_SENSOR_VIBRATION_ON_MASK ? 
                VibrationState.On : 
                VibrationState.Off;

              TempAutoVibrationState = AutoVibrationState.Unknown;

              break;
            }

          case CompactionSensorType.MC024:
          case CompactionSensorType.CATFactoryFitSensor:
          case CompactionSensorType.NoSensor:
            {
              // Per TFS US 37212: Machines that do not report a compaction sensor type will
              // report vibration state information directly from the machine ECM in the FLAGS TAG.
              TempVibrationState = (VibrationState)((value & ICModeFlags.IC_TEMPERATURE_VIBRATION_STATE_MASK) >> ICModeFlags.IC_TEMPERATURE_VIBRATION_STATE_SHIFT);
              TempAutoVibrationState = (AutoVibrationState)(value & ICModeFlags.IC_TEMPERATURE_AUTO_VIBRATION_STATE_MASK);
              break;
            }
          default:
            throw new TRexTAGFileProcessingException($"Unknown sensor type: {(int)_ICSensorType}");
        }

        MachineTargetValueChangesAggregator.VibrationStateEvents.PutValueAtDate(_DataTime, TempVibrationState);
        MachineTargetValueChangesAggregator.AutoVibrationStateEvents.PutValueAtDate(_DataTime, TempAutoVibrationState);
        MachineTargetValueChangesAggregator.ICFlagsStateEvents.PutValueAtDate(_DataTime, value);
      }
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICMode',slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Adds the CCV target value set on the machine into the target CCV list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICCCVTargetValue(short Value)
    {
      base.SetICCCVTargetValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetCCVStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICCCVTargetValue', slpmcDebug); 
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Adds the CCA target value set on the machine into the target CCA list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICCCATargetValue(byte Value)
    {
      base.SetICCCATargetValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetCCAStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICCCATargetValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Adds the MDP target value set on the machine into the target MDP list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICMDPTargetValue(short Value)
    {
      base.SetICMDPTargetValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetMDPStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICMDPTargetValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Adds the MDP target value set on the machine into the target MDP list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICPassTargetValue(ushort Value)
    {
      base.SetICPassTargetValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetPassCountStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICPassTargetValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Converts the machine direction indicated by Value into a forwards or reverse gear, and injects it into the machine gear events list
    /// </summary>
    /// <param name="Value"></param>
    public override void SetMachineDirection(MachineDirection Value)
    {
      base.SetMachineDirection(Value);

      if (GearValueReceived)
        return;

      MachineGear Gear = MachineGear.Null;

      if (Value == MachineDirection.Forward)
        Gear = MachineGear.Forward;
      else if (Value == MachineDirection.Reverse)
        Gear = MachineGear.Reverse;

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC && (Gear == MachineGear.Forward || Gear == MachineGear.Reverse))
        MachineTargetValueChangesAggregator.MachineGearStateEvents.PutValueAtDate(_DataTime, Gear);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 or Gear not Forward/Reverse in SetMachineDirection', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Sets the machine gear into the machine gear events list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICGear(MachineGear Value)
    {
      base.SetICGear(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC && Value != MachineGear.SensorFailedDeprecated)
        MachineTargetValueChangesAggregator.MachineGearStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICGear', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Sets the target minimum material temperature into the machine target material temperature events list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICTempWarningLevelMinValue(ushort Value)
    {
      base.SetICTempWarningLevelMinValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetMinMaterialTemperature.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICTempWarningLevelMinValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Sets the target maximum material temperature into the machine target material temperature events list
    /// </summary>
    /// <param name="Value"></param>

    protected override void SetICTempWarningLevelMaxValue(ushort Value)
    {
      base.SetICTempWarningLevelMaxValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetMaxMaterialTemperature.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICTempWarningLevelMaxValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    /// <summary>
    /// Sets the target lift thickness into the machine target lift thickness events list
    /// </summary>
    /// <param name="Value"></param>
    protected override void SetICTargetLiftThickness(float Value)
    {
      base.SetICTargetLiftThickness(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.TargetLiftThicknessStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICTargetLiftThickness', slpmcDebug);
        //{$ENDIF}
      //}
    }

    public override void SetICCCVValue(short Value)
    {
      if (Value == 0)
        Value = CellPassConsts.NullCCV; 
      base.SetICCCVValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICRMVValue(short Value)
    {
      base.SetICRMVValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICMDPValue(short Value)
    {
      if (Value == 0)
        Value = CellPassConsts.NullMDP;
      base.SetICMDPValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICCCAValue(byte Value)
    {
      if (Value == 0)
        Value = CellPassConsts.NullCCA;
      base.SetICCCAValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICCCALeftFrontValue(byte Value)
    {
      base.SetICCCALeftFrontValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICCCARightFrontValue(byte Value)
    {
      base.SetICCCARightFrontValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICCCALeftRearValue(byte Value)
    {
      base.SetICCCALeftRearValue(Value);
      Machine.CompactionDataReported = true;
    }

    public override void SetICCCARightRearValue(byte Value)
    {
      base.SetICCCARightRearValue(Value);
      Machine.CompactionDataReported = true;
    }

    protected override void SetRMVJumpThresholdValue(short Value)
    {
      base.SetRMVJumpThresholdValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.RMVJumpThresholdEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetRMVJumpThresholdValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    protected override void SetICSensorType(CompactionSensorType Value)
    {
      base.SetICSensorType(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
      {
        // Tell the machine object itself what the current sensor type is
        Machine.CompactionSensorType = Value;
      }
      //else
      //{
        // {$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICSensorType', slpmcDebug);
        //{$ENDIF}
      //}
    }

    protected override void SetAutomaticsMode(AutomaticsType Value)
    {
      base.SetAutomaticsMode(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.MachineAutomaticsStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetAutomaticsMode', slpmcDebug);
        //{$ENDIF}
      //}
    }

    protected override void SetICLayerIDValue(ushort Value)
    {
      base.SetICLayerIDValue(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.LayerIDStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetICLayerIDValue', slpmcDebug);
        //{$ENDIF}
      //}
    }

    public override void SetGPSMode(GPSMode Value)
    {
      base.SetGPSMode(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
      {
        if (PositioningTech == TRex.Types.PositioningTech.Unknown || PositioningTech == TRex.Types.PositioningTech.UTS)
        {
          if (Value != GPSMode.NoGPS)
            MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(_DataTime, TRex.Types.PositioningTech.GPS);
          else
            MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(_DataTime, TRex.Types.PositioningTech.UTS);
        }

        MachineTargetValueChangesAggregator.GPSModeStateEvents.PutValueAtDate(_DataTime, Value);

        HasGPSModeBeenSet = true;
      }
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetGPSMode', slpmcDebug);
        //{$ENDIF}
      //}
    }

    public override void SetElevationMappingModeState(ElevationMappingMode Value)
    {
      base.SetElevationMappingModeState(Value);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.ElevationMappingModeStateEvents.PutValueAtDate(_DataTime, Value);
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetElevationMappingModeState', slpmcDebug);
        //{$ENDIF}
      //}
    }

    public override void SetGPSAccuracyState(GPSAccuracy accuracy, ushort tolerance)
    {
      base.SetGPSAccuracyState(accuracy, tolerance);

      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
        MachineTargetValueChangesAggregator.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(_DataTime, new GPSAccuracyAndTolerance(accuracy, tolerance));
      //else
      //{
        //{$IFDEF DENSE_TAG_FILE_LOGGING}
        //SIGLogProcessMessage.Publish(Self, '_DataTime = 0 in SetGPSAccuracyState', slpmcDebug);
        //{$ENDIF}
      //}
    }

    protected override bool IgnoreInvalidPositions() => SiteModel.IgnoreInvalidPositions;

    /*
   function MaxEpochInterval: Double; override;


   procedure SetICSonic3D                  (const Value :Byte                  ); override;
   procedure SetInAvoidZoneState(const Value: TICInAvoidZoneState); override;
   procedure SetAgeOfCorrection(const Value: Byte); override;
   */

    /// <summary>
    /// Updates the bounding box surrounding the area of the project worked on with the current
    /// design name selected on the machine.
    /// </summary>
    protected void UpdateCurrentDesignExtent()
    {
      int DesignIndex = SiteModel.SiteModelDesigns.IndexOf(_Design);

      if (DesignIndex != -1)
      {
        SiteModel.SiteModelDesigns[DesignIndex].Extents = DesignExtent;
      }

      // Clear the design extent being maintained in the processor.
      DesignExtent.SetInverted();
    }

    /// <summary>
    /// Reference to the persistent swather used for swathing all epoch contexts within the file
    /// being processed
    /// </summary>
    private SwatherBase Swather;

    /// <summary>
    /// DoProcessEpochContext is the method that does the actual processing
    /// of the epoch intervals into the appropriate data structures. Descendant
    /// classes must override this function.
    /// </summary>
    /// <param name="InterpolationFence"></param>
    /// <param name="machineSide"></param>
    public override void DoProcessEpochContext(Fence InterpolationFence, MachineSide machineSide)
    {
      (Swather ?? (Swather = CreateSwather(null))).InterpolationFence = InterpolationFence;

      // Primary e.g. blade, front drum
      Swather.PerformSwathing(FrontHeightInterpolator1, FrontHeightInterpolator2, FrontTimeInterpolator1,
                              FrontTimeInterpolator2, HasRearAxleInThisEpoch, PassType.Front, machineSide);

      // rear positions
      if (HasRearAxleInThisEpoch)
        Swather.PerformSwathing(RearHeightInterpolator1, RearHeightInterpolator2, RearTimeInterpolator1,
                                RearTimeInterpolator2, HasRearAxleInThisEpoch, PassType.Rear, machineSide);

      // track positions
      if (HasTrackInThisEpoch)
        Swather.PerformSwathing(TrackHeightInterpolator1, TrackHeightInterpolator2, TrackTimeInterpolator1,
                                TrackTimeInterpolator2, false, PassType.Track, machineSide);

      // wheel positions
      if (HasWheelInThisEpoch)
        Swather.PerformSwathing(WheelHeightInterpolator1, WheelHeightInterpolator2, WheelTimeInterpolator1,
                                WheelTimeInterpolator2, false, PassType.Wheel, machineSide);
    }

    /// <summary>
    /// DoPostProcessFileAction is called immediately after the file has been
    /// processed. It allows a descendent class to implement appropriate actions
    /// such as saving data when the reading process is complete.
    /// SuccessState reflects the success or failure of the file processing.
    /// </summary>
    /// <param name="successState"></param>
    public override void DoPostProcessFileAction(bool successState)
    {
      // Record the last data time as the data end event
      if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
      {
        MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.PutValueAtDate(_DataTime, ProductionEventType.EndEvent);

        if (!HasGPSModeBeenSet)
        {
          MachineTargetValueChangesAggregator.GPSModeStateEvents.PutValueAtDate(TagFileStartTime, GPSMode.NoGPS);
          MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(TagFileStartTime, TRex.Types.PositioningTech.UTS);
        }
      }

      // Take into account the fact that the site model extent computed from TAG file swathing
      // operations bounds the cell center points and does not take into account that cells have
      // an area. Expand the computed site model extent by half a cell size to ensure the reported
      // site model extent covers the extent of the cells created by swathing the TAG file.
      SiteModel.SiteModelExtent.Expand(SiteModelGridAggregator.CellSize / 2, SiteModelGridAggregator.CellSize / 2);

      // Update the design extent...
      if (!string.IsNullOrEmpty(_Design))
      {
        UpdateCurrentDesignExtent();
      }
    }

    /// <summary>
    /// Handles a specific set of events that cause modifications to the epoch state, such as positioning
    /// technology, machine start/stop events and map resets
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns></returns>
    public override bool DoEpochStateEvent(EpochStateEvent eventType)
    {
      switch (eventType)
      {
        case EpochStateEvent.MachineStartup:
          if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
            MachineTargetValueChangesAggregator.MachineStartupShutdownEvents.PutValueAtDate(_DataTime, ProductionEventType.StartEvent);
          break;

        case EpochStateEvent.MachineShutdown:
          if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
            MachineTargetValueChangesAggregator.MachineStartupShutdownEvents.PutValueAtDate(_DataTime, ProductionEventType.EndEvent);
          break;

        case EpochStateEvent.MachineMapReset:
          // Todo: Map reset events not implemented yet
          //if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
          //MachineTargetValueChangesAggregator.MapResetEvents.PutValueAtDate(_DataTime, Design);
          break;

        case EpochStateEvent.MachineInUTSMode:
          if (_DataTime != Consts.MIN_DATETIME_AS_UTC)
          {
            PositioningTech = PositioningTech.UTS;
            MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(_DataTime, PositioningTech);
          }
          break;

        default:
          throw new TRexTAGFileProcessingException($"Unknown epoch state event type: {eventType}");
      }

      return true;
    }

    /// <summary>
    /// DoEpochPreProcessAction is called in ProcessEpochContext immediately
    /// before any processing of the epoch information is done. It allows a
    /// descendent class to implement appropriate actions such as inspecting
    /// or processing other information in the epoch not directly related
    /// to the epoch interval itself (such as proofing run information in
    /// intelligent compaction tag files.
    /// </summary>
    /// <returns></returns>
    public override bool DoEpochPreProcessAction()
    {
      return !EpochContainsProofingRunDescription() || ProcessProofingPassInformation();
    }
  }
}
