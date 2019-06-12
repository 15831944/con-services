﻿using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    /// <summary>
    /// Handle the flag indicating the compactor machine is using the Caterpillar Compaction Algorithm
    /// </summary>
    public class TAGUsingCCAValueMatcher : TAGValueMatcher
    {
        public TAGUsingCCAValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagUsingCCA };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            valueSink.UsingCCA = value != 0;

            return true;
        }
    }
}
