﻿using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;

namespace VSS.MasterData.Models.ResultHandling.Abstractions
{
  public class FilterValidationErrorCodesProvider : ContractExecutionStatesEnum
  {
    public FilterValidationErrorCodesProvider()
    {
      DynamicAddwithOffset("Invalid Date Range. StartUTC must be earlier than EndUTC.", 29);
      DynamicAddwithOffset("Invalid Date Range. If using a date range both dates must be provided.", 30);
      DynamicAddwithOffset("Invalid designUid.", 31);
      DynamicAddwithOffset("Invalid spatial filter boundary. Too few points for filter polygon", 35);
      DynamicAddwithOffset("Invalid alignment filter. alignment File Uid is invalid.", 64);
      DynamicAddwithOffset("Invalid alignment filter. Start or end station are invalid.", 65);
      DynamicAddwithOffset("Invalid alignment filter. Left or right offset are invalid.", 66);
      DynamicAddwithOffset("Invalid alignment filter. Parameters are incomplete.", 67);
      DynamicAddwithOffset("Invalid Date Filter. Either EndUTC or DateRangeType must be provided for an as-at date filter.", 68);
      DynamicAddwithOffset("Invalid boundary polygon WKT. Should not be null.", 69);
      DynamicAddwithOffset("Invalid boundary polygon WKT. Should be > 3 points.", 70);
      DynamicAddwithOffset("Invalid boundary polygon WKT. Invalid format.", 71);
      DynamicAddwithOffset("Invalid temperature range filter. Both minimum and maximum must be provided.", 72);
      DynamicAddwithOffset("Invalid pass count range filter. Both minimum and maximum must be provided.", 73);
      DynamicAddwithOffset("Invalid temperature range filter. Minimum must be less than maximum.", 74);
      DynamicAddwithOffset("Invalid pass count range filter. Minimum must be less than maximum.", 75);
      DynamicAddwithOffset($"Invalid temperature range filter. Range must be between {ValidationConstants.MIN_TEMPERATURE} and {ValidationConstants.MAX_TEMPERATURE}.", 76);
      DynamicAddwithOffset($"Invalid pass count range filter. Range must be between {ValidationConstants.MIN_PASS_COUNT} and {ValidationConstants.MAX_PASS_COUNT}.", 77);
      DynamicAddwithOffset("Invalid filter UID in FilterUids", 79);
      DynamicAddwithOffset("Invalid combination role type in FilterUids", 80);
      DynamicAddwithOffset("No master filter referenced in FilterUids", 81);
      DynamicAddwithOffset("More than one general widget filter present in FilterUids", 82);
      DynamicAddwithOffset("More than one volumes widget filter present in FilterUids", 83);
      DynamicAddwithOffset("Filters with undefined combination role present in FilterUids", 84);
    }
  }
}
