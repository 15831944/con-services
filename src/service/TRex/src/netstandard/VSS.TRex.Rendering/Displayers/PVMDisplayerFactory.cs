﻿using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Rendering.Displayers
{
  public static class PVMDisplayerFactory
  {
    public static ProductionPVMDisplayer GetDisplayer(DisplayMode mode /*, FICOptions*/)
    {
      switch (mode)
      {
        case DisplayMode.Height:
          return new PVMDisplayer_Height();
        case DisplayMode.MachineSpeed:
          return new PVMDisplayer_MachineSpeed();
        case DisplayMode.TargetSpeedSummary:
          return new PVMDisplayer_MachineSpeedSummary();
        case DisplayMode.CCV:
          return new PVMDisplayer_CMV();
        case DisplayMode.CCVPercentSummary:
          return new PVMDisplayer_CMVSummary();
        case DisplayMode.CMVChange:
          return new PVMDisplayer_CMVPercentChange();
        case DisplayMode.MDP:
          return new PVMDisplayer_MDP();
        case DisplayMode.MDPPercentSummary:
          return new PVMDisplayer_MDPSummary();
        case DisplayMode.PassCount:
          return new PVMDisplayer_PassCount();
        case DisplayMode.PassCountSummary:
          return new PVMDisplayer_PassCountSummary();
        case DisplayMode.TemperatureDetail:
          return new PVMDisplayer_Temperature();
        case DisplayMode.TemperatureSummary:
          return new PVMDisplayer_TemperatureSummary();
        case DisplayMode.CutFill:
          return new PVMDisplayer_CutFill();
        case DisplayMode.CCA:
          return new PVMDisplayer_CCA();
        case DisplayMode.CCASummary:
          return new PVMDisplayer_CCASummary();
        case DisplayMode.CompactionCoverage:
          return new PVMDisplayer_CompactionCoverage();

        default:
          throw new TRexException($"Unknown display mode to create a displayer for: {mode}");
      }

      // Complete legacy implementation is below
      /*
      Result:= Nil;
case Mode of
icdmCCV: Result:= TSVOICPVMDisplayer_CCV.Create(icdmCCV);
      icdmCCVSummary:
      begin
        Result := TSVOICPVMDisplayer_CCV.Create(icdmCCV);
      (Result as TSVOICPVMDisplayer_CCV).CCVSummaryRequired := Options.CCVSummary;
      end;

      icdmCCVPercent: Result:= TSVOICPVMDisplayer_CCVPercent.Create(icdmCCVPercent);
      icdmCCVPercentSummary:
      begin
        Result := TSVOICPVMDisplayer_CCVPercent.Create(icdmCCVPercent);
      (Result as TSVOICPVMDisplayer_CCVPercent).CCVSummaryRequired := Options.CCVSummary;
      end;

      icdmLatency: Result:= TSVOICPVMDisplayer_Latency.Create(icdmLatency);
      icdmHeight: Result:= TSVOICPVMDisplayer_Height.Create(icdmHeight);
      icdmPassCount: Result:= TSVOICPVMDisplayer_PassCount.Create(icdmPassCount);
      icdmPassCountSummary: Result:= TSVOICPVMDisplayer_PassCount.Create(icdmPassCountSummary);
      icdmRMV: Result:= TSVOICPVMDisplayer_RMV.Create(icdmRMV);
      icdmFrequency: Result:= TSVOICPVMDisplayer_Frequency.Create(icdmFrequency);
      icdmAmplitude: Result:= TSVOICPVMDisplayer_Amplitude.Create(icdmAmplitude);
      icdmMoisture: Result:= TSVOICPVMDisplayer_Moisture.Create(icdmMoisture);
      icdmTemperatureSummary: Result:= TSVOICPVMDisplayer_TemperatureSummary.Create(icdmTemperatureSummary);
      icdmCutFill: Result:= TSVOICPVMDisplayer_CutFill.Create(icdmCutFill);
      icdmGPSMode: Result:= TSVOICPVMDisplayer_GPSMode.Create(icdmGPSMode);

      icdmCompactionCoverage: Result:= ICPVMDisplayer_BaseCoverageMode.Create(icdmCompactionCoverage);
      icdmVolumeCoverage: Result:= ICPVMDisplayer_VolumesCoverageMode.Create(icdmVolumeCoverage);

      icdmMDP: Result:= TSVOICPVMDisplayer_MDP.Create(icdmMDP);
      icdmMDPSummary:
      begin
        Result := TSVOICPVMDisplayer_MDP.Create(icdmMDP);
      (Result as TSVOICPVMDisplayer_MDP).MDPSummaryRequired := Options.MDPSummary;
      end;

      icdmMDPPercent: Result:= TSVOICPVMDisplayer_MDPPercent.Create(icdmMDPPercent);
      icdmMDPPercentSummary:
      begin
        Result := TSVOICPVMDisplayer_MDPPercent.Create(icdmMDPPercent);
      (Result as TSVOICPVMDisplayer_MDPPercent).MDPSummaryRequired := Options.MDPSummary;
      end;

      icdmMachineSpeed: Result:= TSVOICPVMDisplayer_MachineSpeed.Create(icdmMachineSpeed);
      icdmCCVPercentChange:
      begin
         Result := TSVOICPVMDisplayer_CCVPercent.Create(icdmCCVPercent);
      (Result as TSVOICPVMDisplayer_CCVPercent).CompareWithPreviousRequired := true;
      end;
      icdmTargetThicknessSummary: Result:= ICPVMDisplayer_VolumesCoverageMode.Create(icdmTargetThicknessSummary);
      icdmTargetSpeedSummary: Result:= TSVOICPVMDisplayer_SpeedSummary.Create(icdmTargetSpeedSummary);
      icdmCCVChange:
      begin
         Result := TSVOICPVMDisplayer_CCVPercent.Create(icdmCCVChange);
      (Result as TSVOICPVMDisplayer_CCVPercent).CompareWithPreviousRequired := true;
      (Result as TSVOICPVMDisplayer_CCVPercent).UseAbsoluteValues := true;
      end;
      icdmCCA: Result:= TSVOICPVMDisplayer_CCADetail.Create(Mode);
      icdmCCASummary: Result:= TSVOICPVMDisplayer_CCASummary.Create(Mode);
      end;

      if Result = Nil then
SIGLogMessage.Publish(Nil, Format('Mode %s is not supported in CreateAppropriatePVMDisplayer', { SKIP}
                    [ICDisplayModeString(Mode)]), slmcDebug);
                    */
    }
  }
}
