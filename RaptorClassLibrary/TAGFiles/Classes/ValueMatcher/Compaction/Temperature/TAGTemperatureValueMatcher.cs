﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Temperature
{
    /// <summary>
    ///  Handles temperature measurements supplied by asphalt compactors
    /// </summary>
    public class TAGTemperatureValueMatcher : TAGValueMatcher
    {
        public TAGTemperatureValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTemperatureTag };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteTemperature = false;

            valueSink.SetICTemperatureValue(CellPass.NullMaterialTemp);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteTemperature)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if ((ushort)valueSink.ICTemperatureValues.GetLatest() + value < 0)
                    {
                        return false;
                    }

                    valueSink.SetICTemperatureValue((ushort)((ushort)valueSink.ICTemperatureValues.GetLatest() + value));

                    break;
                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is abosulte temperature value
            state.HaveSeenAnAbsoluteTemperature = true;

            if (valueType.Type != TAGDataType.t12bitUInt)
            {
                return false;
            }

            valueSink.SetICTemperatureValue((ushort)value);

            return true;
        }
    }
}
