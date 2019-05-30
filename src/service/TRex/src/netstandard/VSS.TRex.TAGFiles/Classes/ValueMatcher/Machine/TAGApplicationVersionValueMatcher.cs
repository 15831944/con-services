﻿using System.Text;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the machine control application version TAG
    /// </summary>
    public class TAGApplicationVersionValueMatcher : TAGValueMatcher
    {
        public TAGApplicationVersionValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileApplicationVersion };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.ApplicationVersion = Encoding.ASCII.GetString(value);

            return true;
        }
    }
}
