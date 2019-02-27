﻿using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.MDP
{
    public class TAGMDPValueMatcher : TAGValueMatcher
    {
        public TAGMDPValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICMDPTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteMDP = false;

            valueSink.SetICMDPValue(CellPassConsts.NullMDP);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteMDP)
                return false;

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((short)(valueSink.ICMDPValues.GetLatest()) + value) >= 0)
                        valueSink.SetICMDPValue((short)((short)(valueSink.ICMDPValues.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {       
            bool result = false;
         
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                state.HaveSeenAnAbsoluteMDP = true;

                valueSink.SetICMDPValue((short) value);
                result = true;
            }
         
            return result;
        }
    }
}
