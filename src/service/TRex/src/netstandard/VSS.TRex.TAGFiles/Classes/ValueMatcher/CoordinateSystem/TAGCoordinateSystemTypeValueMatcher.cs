﻿using System;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.CoordinateSystem
{
  /// <summary>
  /// Handles the type of grid coordinate system being used by the machine control system
  /// </summary>
  public class TAGCoordinateSystemTypeValueMatcher : TAGValueMatcher
  {
    public TAGCoordinateSystemTypeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
    {
    }

    private static readonly string[] valueTypes = {TAGValueNames.kTagCoordSysType};

    public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      if (valueType.Type == TAGDataType.t4bitUInt &&
          Enum.IsDefined(typeof(CoordinateSystemType), (byte) value))
      {
        valueSink.CSType = (CoordinateSystemType) value;

        if (valueSink.CSType == CoordinateSystemType.ACS)
          valueSink.IsCSIBCoordSystemTypeOnly = false;

        result = true;
      }

      return result;
    }
  }
}
