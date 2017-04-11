﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Time;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Proofing
{
    /// <summary>
    /// Handles proofing run start week and time TAGs
    /// </summary>
    public class TAGEndProofingTimeValueMatcher : TAGValueMatcher
    {
        public TAGEndProofingTimeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileStartProofingTimeTag, TAGValueNames.kTagFileStartProofingWeekTag };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Name == TAGValueNames.kTagFileStartProofingTimeTag)
            {
                // Every time record marks the end of the collected data for an epoch
                // Thus, we instruct the value sink to process its context whenever we recieve a time value.
                if (state.HaveSeenAProofingRunTimeValue)
                {
                    if (!valueSink.ProcessEpochContext())
                    {
                        return false;
                    }
                }

                if (valueType.Type != TAGDataType.t32bitUInt)
                {
                    return false;
                }

                valueSink.StartProofingTime = value;                                  // Time value is GPS milliseconds since start of week
                state.HaveSeenAProofingRunTimeValue = true;
            }

            if (valueType.Name == TAGValueNames.kTagFileStartProofingWeekTag)
            {
                if (valueType.Type != TAGDataType.t16bitUInt)
                {
                    return false;
                }

                valueSink.StartProofingWeek = (short)value;
                state.HaveSeenAProofingRunWeekValue = true;
            }

            // if we have seen both a GPS week and time then we can compute the DataTime
            // value for the value sink
            if (state.HaveSeenAProofingRunTimeValue && state.HaveSeenAProofingRunWeekValue)
            {
                valueSink.StartProofingDataTime = GPS.GPSOriginTimeToDateTime(valueSink.StartProofingWeek, valueSink.StartProofingTime);
            }

            return true;
        }
    }
}
