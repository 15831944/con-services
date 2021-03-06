﻿using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    /// <summary>
    /// Handles the GPS accuracy aggregate message from the machine
    /// </summary>
    public class TAGGPSAccuracyValueMatcher : TAGValueMatcher
    {
        public TAGGPSAccuracyValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagGPSAccuracy };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = valueType.Type == TAGDataType.t16bitUInt;

            if (result)
            {
              GPSAccuracy Accuracy;

              // Shift bits right so that we can check the top 2 bits
              ushort WordToCheck = (ushort) (value >> 14); //16-bit UINT

              switch (WordToCheck)
              {
                case 0:
                  Accuracy = GPSAccuracy.Fine;
                  break;
                case 1:
                  Accuracy = GPSAccuracy.Medium;
                  break;
                case 2:
                  Accuracy = GPSAccuracy.Coarse;
                  break;
                default:
                  Accuracy = GPSAccuracy.Unknown;
                  break;
              }

              // Lose the top 2 bits; what remains is the error limit in mm
              ushort ErrorLimit = (ushort) (value & 0x3fff);

              valueSink.SetGPSAccuracyState(Accuracy, ErrorLimit);
            }

            return result;
        }
    }
}
