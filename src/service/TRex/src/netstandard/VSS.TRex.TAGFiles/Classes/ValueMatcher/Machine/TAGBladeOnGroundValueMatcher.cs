﻿using System;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    /// <summary>
    /// Handles the blade of ground flag as reported fom the machine
    /// </summary>
    public class TAGBladeOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGBladeOnGroundValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileBladeOnGroundTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(OnGroundState), (byte)value))
            {
                return false;
            }

            valueSink.SetOnGround((OnGroundState)value);
            return true;
        }
    }
}
