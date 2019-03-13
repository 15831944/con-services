﻿using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CMV
{
    /// <summary>
    /// Handles absolute and offset Compaction Meter Values
    /// </summary>
    public class TAGCCVValueMatcher : TAGValueMatcher
    {
        public TAGCCVValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCVTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteCCV = false;

            valueSink.SetICCCVValue(CellPassConsts.NullCCV);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            bool result = false;

            if (!state.HaveSeenAnAbsoluteCCV)
                return false;

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                  if (((short) (valueSink.ICCCVValues.GetLatest()) + value) >= 0)
                  {
                    valueSink.SetICCCVValue((short) ((short) (valueSink.ICCCVValues.GetLatest()) + value));
                    result = true;
                  }

                  break;
            }

            return result;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteCCV = true;

            bool result = false;

            if (valueType.Type == TAGDataType.t12bitUInt)
            { 
                valueSink.SetICCCVValue((short)value);
                result = true;
            }

            return result;
        }
    }
}
