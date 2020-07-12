﻿using System;
using FluentAssertions;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CMV;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.MDP;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.PassCount;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Temperature;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.CoordinateSystem;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Events;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Location;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Sensors;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Telematics;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Proofing;
using VSS.TRex.TAGFiles.Classes.ValueMatcher.Time;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Common.Time;
using VSS.TRex.Common.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGValueMatcherTests : IClassFixture<DILoggingFixture>
  {
    private void InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state)
    {
      sink = new TAGProcessorStateBase_Test(); // TAGProcessorStateBase();
      state = new TAGValueMatcherState();
    }

    [Fact()]
    public void Test_TAGValueMatcher_ApplicationVersion()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGApplicationVersionValueMatcher();

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), "Test"),
          "Matcher process function returned false");

      Assert.Equal("Test", sink.ApplicationVersion);
    }

    [Fact()]
    public void Test_TAGValueMatcher_CCATargetValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCATargetValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, sink.ICCCATargetValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_CCAValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCAValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, (byte)sink.ICCCAValues.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
          "Matcher process function returned false");

      Assert.Equal((byte)sink.ICCCAValues.GetLatest(), CellPassConsts.NullCCA);
    }

    [Fact()]
    public void Test_TAGValueMatcher_CCARightFrontValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCARightFrontValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, (byte)sink.ICCCARightFrontValues.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
          "Matcher process function returned false");

      Assert.Equal((byte)sink.ICCCARightFrontValues.GetLatest(), CellPassConsts.NullCCA);
    }

    [Fact()]
    public void Test_TAGValueMatcher_CCARightRearValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCARightRearValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, (byte)sink.ICCCARightRearValues.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
          "Matcher process function returned false");

      Assert.Equal((byte)sink.ICCCARightRearValues.GetLatest(), CellPassConsts.NullCCA);
    }

    [Fact()]
    public void Test_TAGValueMatcher_CCALeftFrontValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCALeftFrontValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, (byte)sink.ICCCALeftFrontValues.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
          "Matcher process function returned false");

      Assert.Equal((byte)sink.ICCCALeftFrontValues.GetLatest(), CellPassConsts.NullCCA);
    }


    [Fact()]
    public void Test_TAGValueMatcher_CCALeftRearValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCALeftRearValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, (byte)sink.ICCCALeftRearValues.GetLatest());

      // Test value asserts correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
          "Matcher process function returned false");

      Assert.Equal((byte)sink.ICCCALeftRearValues.GetLatest(), CellPassConsts.NullCCA);
    }


    [Fact()]
    public void Test_TAGValueMatcher_UsingCCA()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGUsingCCAValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              1),
          "Matcher process function returned false");

      Assert.True(sink.UsingCCA, "TAG value not processed as expected");

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              0),
          "Matcher process function returned false");

      Assert.False(sink.UsingCCA, "TAG value not processed as expected");
    }

    [Fact()]
    public void Test_TAGValueMatcher_CCVValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCCVValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (short)sink.ICCCVValues.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteCCV, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (short)sink.ICCCVValues.GetLatest());

      // Test value asserts correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteCCV, "Incorrect value after assignment");

      Assert.Equal((short)sink.ICCCVValues.GetLatest(), CellPassConsts.NullCCV);
    }

    [Fact()]
    public void Test_TAGValueMatcher_TargetCCVValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTargetCCVValueMatcher();

      Assert.Equal(sink.ICCCVTargetValue, CellPassConsts.NullCCV);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, sink.ICCCVTargetValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_MDPValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMDPValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (short)sink.ICMDPValues.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteMDP, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (short)sink.ICMDPValues.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteMDP, "Incorrect value after assignment");

      Assert.Equal((short)sink.ICMDPValues.GetLatest(), CellPassConsts.NullMDP);
    }

    [Fact()]
    public void Test_TAGValueMatcher_TargetMDPValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTargetMDPValueMatcher();

      Assert.Equal(sink.ICMDPTargetValue, CellPassConsts.NullMDP);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, sink.ICMDPTargetValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_PassCount()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTargetPassCountValueMatcher();

      Assert.Equal(0, sink.ICPassTargetValue);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(100, sink.ICPassTargetValue);

      matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)).Should().BeTrue();
    }

    [Fact()]
    public void Test_TAGValueMatcher_Temperature()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTemperatureValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (ushort)sink.ICTemperatureValues.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteTemperature, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (ushort)sink.ICTemperatureValues.GetLatest());

      // Test value assets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteTemperature, "Incorrect value after assignment");

      Assert.Equal((ushort)sink.ICTemperatureValues.GetLatest(), CellPassConsts.NullMaterialTemperatureValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_TemperatureWarningLevelMin()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTemperatureWarningLevelMinValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ICTempWarningLevelMinValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_TemperatureWarningLevelMax()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTemperatureWarningLevelMaxValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ICTempWarningLevelMaxValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Amplitude()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGAmplitudeValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (ushort)sink.ICAmplitudes.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteAmplitude, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (ushort)sink.ICAmplitudes.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteAmplitude, "Incorrect value after assignment");

      Assert.Equal((ushort)sink.ICAmplitudes.GetLatest(), CellPassConsts.NullAmplitude);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Frequency()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGFrequencyValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (ushort)sink.ICFrequencys.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteFrequency, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (ushort)sink.ICFrequencys.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteFrequency, "Incorrect value after assignment");

      Assert.Equal((ushort)sink.ICFrequencys.GetLatest(), CellPassConsts.NullFrequency);
    }

    [Fact()]
    public void Test_TAGValueMatcher_RMVJumpthreshold()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGRMVJumpThresholdValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              100),
          "Matcher process function returned false");
      Assert.Equal(100, sink.ICRMVJumpthreshold);
    }

    [Fact()]
    public void Test_TAGValueMatcher_RMV()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGRMVValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (short)sink.ICRMVValues.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteRMV, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (short)sink.ICRMVValues.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteRMV, "Incorrect value after assignment");

      Assert.Equal((short)sink.ICRMVValues.GetLatest(), CellPassConsts.NullRMV);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ICModeFlags()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGICModeFlagsValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              8),
          "Matcher process function returned false");
      Assert.Equal(8, sink.ICMode);
    }

    [Fact()]
    public void Test_TAGValueMatcher_LayerID()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLayerIDValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              8),
          "Matcher process function returned false");
      Assert.Equal(8, sink.ICLayerIDValue);
    }

    [Fact()]
    public void Test_TAGValueMatcher_TargetLiftThickness()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTargetLiftThicknessValueMatcher();

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t16bitUInt, 0),
              1000), // supplied as millimeters
          "Matcher process function returned false");
      Assert.Equal(1.0, sink.ICTargetLiftThickness); // Presented as meters

      // Test empty value is supported
      matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)).Should().BeTrue();
    }

    [Fact()]
    public void Test_TAGValueMatcher_VolkelMeasurementRangeUtil()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGVolkelMeasurementRangeUtilValueMatcher();

      // Test offset value before absolute value sets correctly
      Assert.False(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Offset accepted before absolute");

      // Test value sets correctly
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, (int)sink.VolkelMeasureUtilRanges.GetLatest());
      Assert.True(state.HaveSeenAnAbsoluteVolkelMeasUtilRange, "Incorrect value after assignment");

      Assert.True(matcher.ProcessIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
              100),
          "Matcher process function returned false");

      Assert.Equal(1100, (int)sink.VolkelMeasureUtilRanges.GetLatest());

      // Test value ussets correctly on an empty value
      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
          "Matcher process function returned false");

      Assert.False(state.HaveSeenAnAbsoluteVolkelMeasUtilRange, "Incorrect value after assignment");

      Assert.Equal((int)sink.VolkelMeasureUtilRanges.GetLatest(), CellPassConsts.NullVolkelMeasUtilRange);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ControlStateLeftLift()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGControlStateLeftLiftValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ControlStateLeftLift);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ControlStateRightLift()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGControlStateRightLiftValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ControlStateRightLift);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ControlStateLift()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      TAGControlStateLiftValueMatcher matcher = new TAGControlStateLiftValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ControlStateLift);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ControlStateTilt()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGControlStateTiltValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ControlStateTilt);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ControlStateSideShift()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGControlStateSideShiftValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
              1000),
          "Matcher process function returned false");
      Assert.Equal(1000, sink.ControlStateSideShift);
    }

    [Fact()]
    public void Test_TAGValueMatcher_CoordinateSystemType()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCoordinateSystemTypeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              0), // Unknown CS
          "Matcher process function returned false");
      Assert.Equal(CoordinateSystemType.NoCoordSystem, sink.CSType);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              1), // Project calibration (CSIB)
          "Matcher process function returned false");
      Assert.Equal(CoordinateSystemType.CSIB, sink.CSType);
      Assert.True(sink.IsCSIBCoordSystemTypeOnly, "Incorrect value after assignment");

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              2), // Automatic Coordinate System (ACS)
          "Matcher process function returned false");
      Assert.Equal(CoordinateSystemType.ACS, sink.CSType);
      Assert.False(sink.IsCSIBCoordSystemTypeOnly, "Incorrect value after assignment");

      Assert.False(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              3), // Invalid
          "Matcher process function returned false");
    }

    [Fact()]
    public void Test_TAGValueMatcher_UTMZone()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGUTMZoneValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
              50),
          "Matcher process function returned false");
      Assert.Equal(50, sink.UTMZone);

      sink.UTMZoneAtFirstPosition.Should().Be(50);
    }

    [Fact()]
    public void Test_TAGValueMatcher_MachineShutdown()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMachineShutdownValueMatcher();

      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(EpochStateEvent.MachineShutdown, ((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent);
    }

    [Fact()]
    public void Test_TAGValueMatcher_MachineStartup()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMachineStartupValueMatcher();

      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(EpochStateEvent.MachineStartup, ((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent);
    }

    [Fact()]
    public void Test_TAGValueMatcher_MapReset()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMapResetValueMatcher();

      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(EpochStateEvent.MachineMapReset, ((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent);
    }

    [Fact()]
    public void Test_TAGValueMatcher_UTSMode()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGUTSModeValueMatcher();

      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(EpochStateEvent.MachineInUTSMode, ((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Height()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGHeightValueMatcher();

      Assert.True(matcher.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
          "Matcher process function returned false");
      Assert.Equal(100.0, sink.LLHHeight);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Longitude()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLongitudeValueMatcher();

      Assert.True(matcher.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
          "Matcher process function returned false");
      Assert.Equal(100.0, sink.LLHLon);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Latitude()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLatitudeValueMatcher();

      Assert.True(matcher.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
          "Matcher process function returned false");
      Assert.Equal(100.0, sink.LLHLat);
    }

    [Fact()]
    public void Test_TAGValueMatcher_LLH()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);

      var matcherTime = new TAGTimeValueMatcher();
      matcherTime.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, TAGDataType.t32bitUInt, 0), 10000000);
      matcherTime.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileWeekTag, TAGDataType.t16bitUInt, 0), 2000);

      var matcherLat = new TAGLatitudeValueMatcher();
      matcherLat.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0);
      
      var matcherLon = new TAGLongitudeValueMatcher();
      matcherLon.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 101.0);

      var matcherHeight = new TAGHeightValueMatcher();
      matcherHeight.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 102.0);

      sink.LLHReceived.Should().BeTrue();
      sink.LLHLat.Should().Be(100);
      sink.LLHLon.Should().Be(101);
      sink.LLHHeight.Should().Be(102);

      sink.LLHHeightRecordedTime.Should().Be(GPS.GPSOriginTimeToDateTime(2000, 10000000));
      sink.LLHLonRecordedTime.Should().Be(GPS.GPSOriginTimeToDateTime(2000, 10000000));
    }

    [Fact()]
    public void Test_TAGValueMatcher_CompactorSensorType()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGCompactorSensorTypeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              0), // No sensor
          "Matcher process function returned false");
      Assert.Equal(CompactionSensorType.NoSensor, sink.ICSensorType);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              1), // MC 024
          "Matcher process function returned false");
      Assert.Equal(CompactionSensorType.MC024, sink.ICSensorType);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              2), // Volkel
          "Matcher process function returned false");
      Assert.Equal(CompactionSensorType.Volkel, sink.ICSensorType);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              3), // CAT factory fit
          "Matcher process function returned false");
      Assert.Equal(CompactionSensorType.CATFactoryFitSensor, sink.ICSensorType);

      Assert.False(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              4), // Invalid
          "Matcher process function returned false");
    }

    [Fact()]
    public void Test_TAGValueMatcher_VolkelMeasurementRange()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGVolkelMeasurementRangeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              100),
          "Matcher process function returned false");
      Assert.Equal(100, (int)sink.VolkelMeasureRanges.GetLatest());
    }

    [Fact()]
    public void Test_TAGValueMatcher_RadioSerial()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGRadioSerialValueMatcher();

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), "Test"),
          "Matcher process function returned false");

      Assert.Equal("Test", sink.RadioSerial);
    }

    [Fact()]
    public void Test_TAGValueMatcher_RadioType()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGRadioTypeValueMatcher();

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), "Test"),
          "Matcher process function returned false");

      Assert.Equal("Test", sink.RadioType);
    }

    [Fact()]
    public void Test_TAGValueMatcher_BladeOnGround()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGBladeOnGroundValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              0), // No
          "Matcher process function returned false");

      Assert.Equal(OnGroundState.No, (OnGroundState)sink.OnGrounds.GetLatest());

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              5), //Yes, Remote Switch
          "Matcher process function returned false");

      Assert.Equal(OnGroundState.YesRemoteSwitch, (OnGroundState)sink.OnGrounds.GetLatest());

      Assert.False(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              7), // Invalid
          "Matcher process function returned false");
    }

    [Fact()]
    public void Test_TAGValueMatcher_OnGround()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGOnGroundValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              0), // No
          "Matcher process function returned false");

      Assert.Equal(OnGroundState.No, (OnGroundState)sink.OnGrounds.GetLatest());

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              1), //Yes, Legacy
          "Matcher process function returned false");

      Assert.Equal(OnGroundState.YesLegacy, (OnGroundState)sink.OnGrounds.GetLatest());

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              2), // Invalid -> unknown
          "Matcher process function returned false");
      Assert.Equal(OnGroundState.Unknown, (OnGroundState)sink.OnGrounds.GetLatest());
    }

    [Fact()]
    public void Test_TAGValueMatcher_Design()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGDesignValueMatcher();

      Assert.True(matcher.ProcessUnicodeStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
              "Test"),
          "Matcher process function returned false");

      Assert.Equal("Test", sink.Design);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Gear()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGGearValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              3), // Sensor failed
          "Matcher process function returned false");
      Assert.Equal(MachineGear.SensorFailedDeprecated, sink.ICGear);
    }

    [Fact()]
    public void Test_TAGValueMatcher_StartProofingRun()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGStartProofingValueMatcher();

      string name = "Start Proofing Run";

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), name),
        "Matcher process function returned false");
      Assert.Equal(name, sink.StartProofing);
    }

    [Fact()]
    public void Test_TAGValueMatcher_EndProofingRun()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGEndProofingValueMatcher();

      string name = "End Proofing Run";

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), name),
        "Matcher process function returned false");
      Assert.Equal(name, sink.EndProofingName);
    }

    [Fact()]
    public void Test_TAGValueMatcher_StartProofingDataTime()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGEndProofingTimeValueMatcher();

      var time = (uint)DateTime.Now.Millisecond;

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingTimeTag, TAGDataType.t32bitUInt, 0),
          time),
        "Matcher process function returned false");
      Assert.Equal(time, sink.StartProofingTime);
    }

    [Fact()]
    public void Test_TAGValueMatcher_StartProofingDataWeek()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGEndProofingTimeValueMatcher();

      var weeks = 1000;

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingWeekTag, TAGDataType.t16bitUInt, 0),
          (ushort)weeks),
        "Matcher process function returned false");
      Assert.Equal(weeks, sink.StartProofingWeek);
    }

    [Fact()]
    public void Test_TAGValueMatcher_MachineID()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMachineIDValueMatcher();

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), "Test"),
          "Matcher process function returned false");

      Assert.Equal("Test", sink.MachineID);
    }

    [Fact()]
    public void Test_TAGValueMatcher_MachineSpeed()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMachineSpeedValueMatcher();

      Assert.True(matcher.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
          "Matcher process function returned false");
      Assert.Equal(100.0, (double)sink.ICMachineSpeedValues.GetLatest());
      Assert.True(state.HaveSeenMachineSpeed, "HaveSeenMachineSpeed not set");
    }

    [Fact()]
    public void Test_TAGValueMatcher_MachineType()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGMachineTypeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
          (byte)MachineType.Dozer),
          "Matcher process function returned false");
      Assert.Equal(MachineType.Dozer, sink.MachineType);
    }

    [Fact()]
    public void Test_TAGValueMatcher_ElevationMappingMode()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGElevationMappingModeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0), 0),
          "Matcher process function returned false");
      sink.ElevationMappingMode.Should().Be(ElevationMappingMode.LatestElevation);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0), 1),
          "Matcher process function returned false");
      sink.ElevationMappingMode.Should().Be(ElevationMappingMode.MinimumElevation);

      Assert.False(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0), 3),
          "Matcher process function returned false");
    }

    [Fact()]
    public void Test_TAGValueMatcher_Sequence()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGSequenceValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t32bitUInt, 0),
              100),
          "Matcher process function returned false");
      Assert.Equal(100U, sink.Sequence);
    }

    [Fact()]
    public void Test_TAGValueMatcher_WheelWidth()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGWheelWidthValueMatcher();

      Assert.True(matcher.ProcessDoubleValue(state, sink, new TAGDictionaryItem("", TAGDataType.tIEEEDouble, 0),
              1.0),
          "Matcher process function returned false");
      Assert.Equal(1.0, sink.MachineWheelWidth);
    }

    [Fact()]
    public void Test_TAGValueMatcher_LeftRightBlade()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLeftRightBladeValueMatcher();

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileLeftTag, TAGDataType.tEmptyType,
            0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Left, state.Side);

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileRightTag, TAGDataType.tEmptyType,
            0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Right, state.Side);
    }

    [Fact()]
    public void Test_TAGValueMatcher_LeftRightTrack()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLeftRightTrackValueMatcher();

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileLeftTrackTag,
            TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Left, state.TrackSide);

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileRightTrackTag,
            TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Right, state.TrackSide);
    }

    [Fact()]
    public void Test_TAGValueMatcher_LeftRightWheel()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLeftRightWheelValueMatcher();

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileLeftWheelTag,
            TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Left, state.WheelSide);

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileRightWheelTag,
            TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Right, state.WheelSide);
    }

    [Fact()]
    public void Test_TAGValueMatcher_LeftRightRear()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGLeftRightRearValueMatcher();

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileLeftRearTag,
            TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Left, state.RearSide);

      Assert.True(
          matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileRightRearTag,
            TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.Equal(TAGValueSide.Right, state.RearSide);
    }

    [Fact()]
    public void Test_TAGValueMatcher_BladeOrdinateValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGBladeOrdinateValueMatcher();

      state.Side = TAGValueSide.Left;

      TAGDictionaryItem absoluteItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingTag, TAGDataType.tIEEEDouble, 0);
      TAGDictionaryItem offsetItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingTag, TAGDataType.t32bitInt, 0);

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataLeft.X);
      Assert.True(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileNorthingTag;
      offsetItem.Name = TAGValueNames.kTagFileNorthingTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataLeft.Y);
      Assert.True(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileElevationTag;
      offsetItem.Name = TAGValueNames.kTagFileElevationTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataLeft.Z);
      Assert.True(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataLeft.Z);

      // Test an empty value clears the absolute value seen flag

      Assert.True(matcher.ProcessEmptyValue(state, sink, absoluteItem), "Matcher process function returned false");
      Assert.False(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");
    }

    [Fact()]
    public void Test_TAGValueMatcher_TrackOrdinateValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTrackOrdinateValueMatcher();

      state.TrackSide = TAGValueSide.Left;

      sink.HaveFirstTrackEpoch.Should().BeFalse();

      TAGDictionaryItem absoluteItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingTrackTag, TAGDataType.tIEEEDouble, 0);
      TAGDictionaryItem offsetItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingTrackTag, TAGDataType.t32bitInt, 0);

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataTrackLeft.X);
      Assert.True(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataTrackLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileNorthingTrackTag;
      offsetItem.Name = TAGValueNames.kTagFileNorthingTrackTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataTrackLeft.Y);
      Assert.True(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataTrackLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileElevationTrackTag;
      offsetItem.Name = TAGValueNames.kTagFileElevationTrackTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataTrackLeft.Z);
      Assert.True(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataTrackLeft.Z);

      // Test an empty value clears the absolute value seen flag

      Assert.True(matcher.ProcessEmptyValue(state, sink, absoluteItem), "Matcher process function returned false");
      Assert.False(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

      sink.HaveFirstTrackEpoch.Should().BeFalse();
    }

    [Fact()]
    public void Test_TAGValueMatcher_WheelOrdinateValue()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGWheelOrdinateValueMatcher();

      state.WheelSide = TAGValueSide.Left;

      TAGDictionaryItem absoluteItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingWheelTag, TAGDataType.tIEEEDouble, 0);
      TAGDictionaryItem offsetItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingWheelTag, TAGDataType.t32bitInt, 0);

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataWheelLeft.X);
      Assert.True(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataWheelLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileNorthingWheelTag;
      offsetItem.Name = TAGValueNames.kTagFileNorthingWheelTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataWheelLeft.Y);
      Assert.True(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataWheelLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileElevationWheelTag;
      offsetItem.Name = TAGValueNames.kTagFileElevationWheelTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataWheelLeft.Z);
      Assert.True(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataWheelLeft.Z);

      // Test an empty value clears the absolute value seen flag

      Assert.True(matcher.ProcessEmptyValue(state, sink, absoluteItem), "Matcher process function returned false");
      Assert.False(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");
    }

    [Fact()]
    public void Test_TAGValueMatcher_RearOrdinateValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGRearOrdinateValueMatcher();

      state.RearSide = TAGValueSide.Left;

      TAGDictionaryItem absoluteItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingRearTag, TAGDataType.tIEEEDouble, 0);
      TAGDictionaryItem offsetItem =
          new TAGDictionaryItem(TAGValueNames.kTagFileEastingRearTag, TAGDataType.t32bitInt, 0);

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataRearLeft.X);
      Assert.True(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataRearLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileNorthingRearTag;
      offsetItem.Name = TAGValueNames.kTagFileNorthingRearTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataRearLeft.Y);
      Assert.True(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataRearLeft.X);

      absoluteItem.Name = TAGValueNames.kTagFileElevationRearTag;
      offsetItem.Name = TAGValueNames.kTagFileElevationRearTag;

      Assert.True(matcher.ProcessDoubleValue(state, sink, absoluteItem, 1.0), "Matcher process function returned false");

      Assert.Equal(1.0, sink.DataRearLeft.Z);
      Assert.True(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

      // Offset the absolute item with a distance in millimeters
      Assert.True(matcher.ProcessIntegerValue(state, sink, offsetItem, 500), "Matcher process function returned false");
      Assert.Equal(1.5, sink.DataRearLeft.Z);

      // Test an empty value clears the absolute value seen flag

      Assert.True(matcher.ProcessEmptyValue(state, sink, absoluteItem), "Matcher process function returned false");
      Assert.False(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");
    }

    [Theory()]
    [InlineData(GPSAccuracy.Coarse)]
    [InlineData(GPSAccuracy.Fine)]
    [InlineData(GPSAccuracy.Medium)]
    [InlineData(GPSAccuracy.Unknown)]
    public void Test_TAGValueMatcher_GPSAccuracy(GPSAccuracy accuracy)
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGGPSAccuracyValueMatcher();

      // Set GPS accuracy word to be Medium accuracy with a 100mm error limit
      ushort GPSAccuracyWord = (ushort)(((int)accuracy << 14) | 100);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t16bitUInt, 0),
              GPSAccuracyWord),
          "Matcher process function returned false");
      Assert.True(sink.GPSAccuracy == accuracy && sink.GPSAccuracyErrorLimit == 100,
          "Incorrect value after assignment");
    }

    [Fact()]
    public void Test_TAGValueMatcher_GPSBasePosition()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGGPSBasePositionValueMatcher();

      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
          "Matcher process function returned false");
      Assert.True(state.GPSBasePositionReportingHaveStarted, "Incorrect value after assignment");
    }

    [Fact()]
    public void Test_TAGValueMatcher_GPSMode()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGGPSModeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              2), // Float RTK
          "Matcher process function returned false");
      Assert.Equal(GPSMode.Float, (GPSMode)sink.GPSModes.GetLatest());
    }

    [Fact()]
    public void Test_TAGValueMatcher_ValidPosition()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGValidPositionValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              100),
          "Matcher process function returned false");
      Assert.Equal(100, sink.ValidPosition);
    }

    [Fact()]
    public void Test_TAGValueMatcher_EndProofingTimeValue()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGEndProofingTimeValueMatcher();

      // Test the Time aspect
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingTimeTag, TAGDataType.t32bitUInt, 0),
              10000000),
          "Matcher process function returned false");
      Assert.Equal(10000000U, sink.StartProofingTime);
      Assert.True(state.HaveSeenAProofingRunTimeValue, "Incorrect value after assignment");

      // Test the Week aspect
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingWeekTag, TAGDataType.t16bitUInt, 0),
              10000),
          "Matcher process function returned false");
      Assert.Equal(10000, sink.StartProofingWeek);
      Assert.True(state.HaveSeenAProofingRunWeekValue, "Incorrect value after assignment");

      // Test the actual start proofing run time value is calculated correctly
      Assert.True(sink.StartProofingTime == 10000000 &&
                  sink.StartProofingWeek == 10000 &&
                  GPS.GPSOriginTimeToDateTime(sink.StartProofingWeek, sink.StartProofingTime) ==
                  sink.StartProofingDataTime,
          "Start proofing data time not as expected");
    }

    [Fact()]
    public void Test_TAGValueMatcher_EndProofing()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGEndProofingValueMatcher();

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), "TestEnd"),
          "Matcher process function returned false");

      Assert.Equal("TestEnd", sink.EndProofingName);
    }

    [Fact()]
    public void Test_TAGValueMatcher_StartProofing()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGStartProofingValueMatcher();

      // Set time and week values
      sink.GPSWeekTime = 10000000;
      state.HaveSeenATimeValue = true;
      sink.GPSWeekNumber = 10000;
      state.HaveSeenAWeekValue = true;

      sink.DataTime = GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime = 10000000);

      Assert.True(matcher.ProcessANSIStringValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0), "TestStart"),
          "Matcher process function returned false");

      Assert.Equal("TestStart", sink.StartProofing);

      // Test the start proofing data time is calculated correctly
      Assert.Equal(sink.StartProofingDataTime, sink.DataTime);

      Assert.True(matcher.ProcessEmptyValue(state, sink, new TAGDictionaryItem("", TAGDataType.tANSIString, 0)),
          "Matcher process function returned false");

      Assert.Equal("", sink.StartProofing);
    }

    [Fact()]
    public void Test_TAGValueMatcher_Time()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGTimeValueMatcher();

      // Test the Time aspect
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, TAGDataType.t32bitUInt, 0),
              10000000),
          "Matcher process function returned false");
      Assert.Equal(10000000U, sink.GPSWeekTime);
      Assert.True(state.HaveSeenATimeValue, "Incorrect HaveSeenATimeValue value after time assignment");
      Assert.False(state.HaveSeenAWeekValue, "Incorrect HaveSeenAWeekValue value after time assignment");

      // Test the absolute Week aspect
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileWeekTag, TAGDataType.t16bitUInt, 0),
              10000),
          "Matcher process function returned false");
      Assert.Equal(10000, sink.GPSWeekNumber);
      Assert.True(state.HaveSeenAWeekValue, "Incorrect value after assignment");

      // Test the epoch data time is calculated correctly
      Assert.True(sink.GPSWeekTime == 10000000 &&
                  sink.GPSWeekNumber == 10000 &&
                  GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime) == sink.DataTime,
          "Absolute epoch GPS data time not as expected");

      // Test the incremental time aspect
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, TAGDataType.t4bitUInt, 0),
              10), // Adds 10, 1/10 of a second intervals, for an increment of 1000ms
          "Matcher process function returned false");
      Assert.Equal(10001000U, sink.GPSWeekTime);
      Assert.True(state.HaveSeenAWeekValue, "Incorrect value after assignment");

      // Test the epoch data time is calculated correctly
      Assert.True(sink.GPSWeekTime == 10001000 &&
                  sink.GPSWeekNumber == 10000 &&
                  GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime) == sink.DataTime,
          "Incremented epoch GPS data time not as expected");
    }

    [Fact()]
    public void Test_TAGValueMatcher_AgeOfCorrections()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGAgeValueMatcher();

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
              10),
          "Matcher process function returned false");
      Assert.Equal(10, (byte)sink.AgeOfCorrections.GetLatest());
    }

    [Fact()]
    public void Test_TAGValueMatcher_3DSonic()
    {
      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAG3DSonicValueMatcher();
      var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

      for (uint i = 0; i < 3; i++)
      {
        Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, i),
            "Matcher process function returned false");
        Assert.Equal(sink.ICSonic3D, i);
      }

      Assert.False(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 3),
          "Matcher process function returned false");
    }

    [Fact()]
    public void Test_TAGValueMatcher_Direction()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGDirectionValueMatcher();
      var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

      // Note: Machine direction values from the machine are 1-based
      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 1), // Forwards
          "Matcher process function returned false");
      Assert.Equal(MachineDirection.Forward, sink.MachineDirection);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 2), // Reverse
          "Matcher process function returned false");
      Assert.Equal(MachineDirection.Reverse, sink.MachineDirection);
    }

    [Fact()]
    public void Test_TAGValueMatcher_AvoidanceZone()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGInAvoidanceZoneValueMatcher();
      var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

      for (uint i = 0; i < 4; i++)
      {
        Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, i),
            "Matcher process function returned false");
        Assert.Equal(sink.InAvoidZone, i);
      }

      Assert.False(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 5),
          "Matcher process function returned false");
    }

    [Fact()]
    public void Test_TAGValueMatcher_ResearchData()
    {

      InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
      var matcher = new TAGResearchDataValueMatcher();
      var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 0), // False
          "Matcher process function returned false");
      Assert.False(sink.ResearchData);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 1), // False
          "Matcher process function returned false");
      Assert.True(sink.ResearchData);

      Assert.True(matcher.ProcessUnsignedIntegerValue(state, sink, dictItem, 2), // Also true...
          "Matcher process function returned false");
      Assert.True(sink.ResearchData);
    }
  }
}
