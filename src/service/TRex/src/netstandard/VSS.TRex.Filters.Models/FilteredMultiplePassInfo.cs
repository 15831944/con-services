﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.ConfigurationStore;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Types;

namespace VSS.TRex.Filters.Models
{
  /// <summary>
  /// FilteredMultiplePassInfo records all the information that a filtering operation
  ///   selected from an IC grid cell containing all the recorded machine passes.
  /// </summary>
  public class FilteredMultiplePassInfo : IFromToBinary
  {
    /// <summary>
    /// PassCount keeps track of the actual number of passes in the list
    /// </summary>
    public int PassCount { get; set; }

    private FilteredPassData[] filteredPassData;

    /// <summary>
    /// The set of passes selected by the filtering operation
    /// </summary>
    public FilteredPassData[] FilteredPassData => filteredPassData;

    public FilteredMultiplePassInfo()
    {
    }

    public FilteredMultiplePassInfo(FilteredPassData[] filteredPasses)
    {
      SetFilteredPasses(filteredPasses);
    }

    public void SetFilteredPasses(FilteredPassData[] filteredPasses)
    {
      filteredPassData = filteredPasses;
      PassCount = filteredPasses.Length;
    }

    private int CellPassAggregationListSizeIncrement() => DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT", Consts.VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT);

    private void CheckArrayCapacity()
    {
      // Increase the length of the passes array
      if (filteredPassData == null)
      {
        filteredPassData = new FilteredPassData[CellPassAggregationListSizeIncrement()];
      }
      else
      {
        if (PassCount == filteredPassData.Length)
        {
          Array.Resize(ref filteredPassData, PassCount + CellPassAggregationListSizeIncrement());
        }
      }
    }

    /// <summary>
    /// Adds a pass to the set of passes beign constructed as a result of the filtering operation.
    /// </summary>
    /// <param name="pass"></param>
    /// <param name="passesOrderedInIncreasingTime"></param>
    public void AddPass(CellPass pass /*, bool passesOrderedInIncreasingTime = true*/)
    {
      /*TODO convert when C# equivalent of IFOPT C+ is understood
       {$IFOPT C+}
        if PassesOrderedInIncreasingTime then
          begin
            if (FPassCount > 0) and(FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.Time + OneSecond)) then
             Assert(False, Format('Passes not added to filtered pass list in increasing time order (1) (Time1 vs Time2 = %.6f (%s) vs %.6f (%s)',
                                   [FilteredPassData[FPassCount - 1].FilteredPass.Time,
                                    FormatCellPassTimeValue(FilteredPassData[FPassCount - 1].FilteredPass.Time, cpftWithMilliseconds, False),
                                     Pass.Time,
                                     FormatCellPassTimeValue(Pass.Time, cpftWithMilliseconds, False)])); {SKIP}
          end
        else
          begin
            Assert(((FPassCount = 0) or
                    (FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.Time - OneSecond))),
                   'Passes not added to filtered pass list in decreasing time order'); {SKIP}
          end;
      {$ENDIF}
      */

      CheckArrayCapacity();

      // Add the pass to the list
      FilteredPassData[PassCount].FilteredPass = pass;
      PassCount++;
    }

    public void AddPass(FilteredPassData pass /*, bool passesOrderedInIncreasingTime*/)
    {
      /* TODO include when IFOPT C+ equivalent is identified
      {$IFOPT C+}
      if PassesOrderedInIncreasingTime then
        begin
        if (FPassCount > 0) and(FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.FilteredPass.Time + OneSecond)) then
         Assert(False, Format('Passes not added to filtered pass list in increasing time order (2) (Time1 vs Time2 = %.6f vs %.6f', [FilteredPassData[FPassCount - 1].FilteredPass.Time, Pass.FilteredPass.Time])); { SKIP}
      end
    else
      begin
        Assert(((FPassCount = 0) or
        (FilteredPassData[FPassCount - 1].FilteredPass.Time > (Pass.FilteredPass.Time - OneSecond))),
       'Passes not added to filtered pass list in decreasing time order'); { SKIP}
      end;
      {$ENDIF}
      */

      CheckArrayCapacity();

      // Add the pass to the list
      filteredPassData[PassCount] = pass;
      PassCount++;
    }

    /// <summary>
    /// Assigns (copies) the set of filtered passes from another instance to this instance
    /// </summary>
    /// <param name="Source"></param>
    public void Assign(FilteredMultiplePassInfo Source)
    {
      if (PassCount < Source.PassCount)
        filteredPassData = new FilteredPassData[Source.PassCount];

      PassCount = Source.PassCount;

      Array.Copy(Source.FilteredPassData, filteredPassData, PassCount);
    }

    /// <summary>
    /// Clear the set of filtered cell passes
    /// </summary>
    public void Clear()
    {
      PassCount = 0;
    }

    /// <summary>
    /// Returns the time of the first cell pass in the set of filtered cell passes
    /// </summary>
    public DateTime FirstPassTime => PassCount > 0 ? filteredPassData[0].FilteredPass.Time : DateTime.MinValue;

    /// <summary>
    /// Determines the time of the cell pass with the highest elevation in the set of cell passes
    /// </summary>
    /// <returns></returns>
    public DateTime HighestPassTime()
    {
      float TempHeight = Consts.NullHeight;

      DateTime Result = DateTime.MinValue;

      // todo: benchmark this against taking a copy of the Filteredpass in each loop iteration. Apply results to other similar contexts in this class
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (TempHeight == Consts.NullHeight)
        {
          TempHeight = filteredPassData[i].FilteredPass.Height;
          Result = filteredPassData[i].FilteredPass.Time;
        }
        else
        {
          if (filteredPassData[i].FilteredPass.Height > TempHeight)
          {
            TempHeight = filteredPassData[i].FilteredPass.Height;
            Result = filteredPassData[i].FilteredPass.Time;
          }
        }
      }

      return Result;
    }


    /// <summary>
    /// Determine the time of the last cell pass in the set of filtered cell passes
    /// </summary>
    /// <returns></returns>
    public DateTime LastPassTime() => PassCount > 0 ? filteredPassData[PassCount - 1].FilteredPass.Time : DateTime.MinValue;


    public ushort LastPassValidAmp()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.Amplitude != CellPassConsts.NullAmplitude)
          return filteredPassData[i].FilteredPass.Amplitude;

      return CellPassConsts.NullAmplitude;
    }

    public void LastPassValidCCVDetails(out short aCCV, out short aTarget)
    {
      aCCV = CellPassConsts.NullCCV;
      aTarget = CellPassConsts.NullCCV;
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (filteredPassData[i].TargetValues.TargetCCV != CellPassConsts.NullCCV && aTarget == CellPassConsts.NullCCV) 
          aTarget = filteredPassData[i].TargetValues.TargetCCV; // just in case ccv is missing but not target

        if (filteredPassData[i].FilteredPass.CCV != CellPassConsts.NullCCV)
        {
          aCCV = filteredPassData[i].FilteredPass.CCV;
          aTarget = filteredPassData[i].TargetValues.TargetCCV; // update target with this record
          return;
        }
      }
    }

    public byte LastPassValidCCA()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.CCA != CellPassConsts.NullCCA)
          return filteredPassData[i].FilteredPass.CCA;

      return CellPassConsts.NullCCA;
    }

    public void LastPassValidCCADetails(out byte aCCA, out byte aTarget)
    {
      aCCA = CellPassConsts.NullCCA;
      aTarget = CellPassConsts.NullCCA;
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (filteredPassData[i].TargetValues.TargetCCA != CellPassConsts.NullCCA && aTarget == CellPassConsts.NullCCA)
          aTarget = filteredPassData[i].TargetValues.TargetCCA; // just in case cca is missing but not target

        if (filteredPassData[i].FilteredPass.CCA != CellPassConsts.NullCCA)
        {
          aCCA = filteredPassData[i].FilteredPass.CCA;
          aTarget = filteredPassData[i].TargetValues.TargetCCA; // update target with this record
          return;
        }
      }
    }

    public short LastPassValidCCV()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.CCV != CellPassConsts.NullCCV)
          return filteredPassData[i].FilteredPass.CCV;

      return CellPassConsts.NullCCV;
    }

    public double LastPassValidCCVPercentage()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.CCV != CellPassConsts.NullCCV)
        {
          short CCVtarget = filteredPassData[i].TargetValues.TargetCCV;
          if (CCVtarget != 0 && CCVtarget != CellPassConsts.NullCCV)
            return filteredPassData[i].FilteredPass.CCV / (1.0 * CCVtarget);

          return CellPassConsts.NullCCVPercentage;
        }

      return CellPassConsts.NullCCVPercentage;
    }

    public ushort LastPassValidFreq()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.Frequency != CellPassConsts.NullFrequency)
          return filteredPassData[i].FilteredPass.Frequency;

      return CellPassConsts.NullFrequency;
    }

    public short LastPassValidMDP()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.MDP != CellPassConsts.NullMDP)
          return filteredPassData[i].FilteredPass.MDP;

      return CellPassConsts.NullMDP;
    }

    public void LastPassValidMDPDetails(out short aMDP, out short aTarget)
    {
      aMDP = CellPassConsts.NullMDP;
      aTarget = CellPassConsts.NullMDP;
      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (filteredPassData[i].TargetValues.TargetMDP != CellPassConsts.NullMDP && aTarget == CellPassConsts.NullMDP)
          aTarget = filteredPassData[i].TargetValues.TargetMDP; // just in case ccv is missing but not target

        if (filteredPassData[i].FilteredPass.MDP != CellPassConsts.NullMDP)
        {
          aMDP = filteredPassData[i].FilteredPass.MDP;
          aTarget = filteredPassData[i].TargetValues.TargetMDP; // update target with this record
          return;
        }
      }
    }
    public double LastPassValidMDPPercentage()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.MDP != CellPassConsts.NullMDP)
        {
          short MDPtarget = filteredPassData[i].TargetValues.TargetMDP;
          if (MDPtarget != 0 && MDPtarget != CellPassConsts.NullMDP)
            return filteredPassData[i].FilteredPass.MDP / (1.0 * MDPtarget);

          return CellPassConsts.NullMDPPercentage;
        }

      return CellPassConsts.NullMDPPercentage;
    }

    public GPSMode LastPassValidGPSMode()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.gpsMode != CellPassConsts.NullGPSMode)
          return filteredPassData[i].FilteredPass.gpsMode;

      return CellPassConsts.NullGPSMode;
    }

    public byte LastPassValidRadioLatency()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.RadioLatency != CellPassConsts.NullRadioLatency)
          return filteredPassData[i].FilteredPass.RadioLatency;

      return CellPassConsts.NullRadioLatency;
    }

    public short LastPassValidRMV()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.RMV != CellPassConsts.NullRMV)
          return filteredPassData[i].FilteredPass.RMV;

      return CellPassConsts.NullRMV;
    }

    public DateTime LowestPassTime()
    {
      float TempHeight = Consts.NullHeight;
      DateTime Result = DateTime.MinValue;

      for (int i = PassCount - 1; i >= 0; i--)
      {
        if (TempHeight == Consts.NullHeight)
        {
          TempHeight = filteredPassData[i].FilteredPass.Height;
          Result = filteredPassData[i].FilteredPass.Time;
        }
        else 
          if (filteredPassData[i].FilteredPass.Height < TempHeight)
            Result = filteredPassData[i].FilteredPass.Time;
      }

      return Result;
    }

    public ushort LastPassValidMaterialTemperature()
    {
      for (int i = PassCount - 1; i >= 0; i--)
        if (filteredPassData[i].FilteredPass.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
          return filteredPassData[i].FilteredPass.MaterialTemperature;

      return CellPassConsts.NullMaterialTemperatureValue;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteInt(PassCount);

      writer.WriteBoolean(filteredPassData != null);
      if (filteredPassData != null)
      {
        writer.WriteInt(filteredPassData.Length);

        for (int i = 0; i < filteredPassData.Length; i++)
          filteredPassData[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      PassCount = reader.ReadInt();

      if (reader.ReadBoolean())
      {
        var count = reader.ReadInt();
        filteredPassData = new FilteredPassData[count];

        for (int i = 0; i < filteredPassData.Length; i++)
          filteredPassData[i].FromBinary(reader);
      }
    }
  }
}
