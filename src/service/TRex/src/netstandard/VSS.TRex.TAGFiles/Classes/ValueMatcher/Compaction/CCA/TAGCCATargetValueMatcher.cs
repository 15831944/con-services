﻿using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCATargetValueMatcher : TAGValueMatcher
    {
        public TAGCCATargetValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCATargetTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICCCATargetValue = (byte) value;
                result = true;
            }

            return result;
        }
    }
}
