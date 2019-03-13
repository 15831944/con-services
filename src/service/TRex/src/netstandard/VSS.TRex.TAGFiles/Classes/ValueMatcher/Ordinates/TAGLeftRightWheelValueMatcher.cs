﻿using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Ordinates
{
    /// <summary>
    /// Handles left/right (side) empty values for Wheel axle positioning
    /// </summary>
    public class TAGLeftRightWheelValueMatcher : TAGValueMatcher
    {
        public TAGLeftRightWheelValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileLeftWheelTag, TAGValueNames.kTagFileRightWheelTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            bool result = false;

            if (valueType.Name == TAGValueNames.kTagFileLeftWheelTag)
            {
                state.WheelSide = TAGValueSide.Left;
                result = true;
            }
            else if (valueType.Name == TAGValueNames.kTagFileRightWheelTag)
            {
                state.WheelSide = TAGValueSide.Right;
                result = true;
            }

            return result;
        }

    }
}
