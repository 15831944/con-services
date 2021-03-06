﻿using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling
{
  public class PreferenceErrorCodesProvider : ContractExecutionStatesEnum
  {
    public PreferenceErrorCodesProvider()
    {
      this.DynamicAddwithOffset("Invalid parameters.", 1);
      this.DynamicAddwithOffset("Unable to create preference key. {0}", 2);
      this.DynamicAddwithOffset("Unable to update preference key. {0}", 3);
      this.DynamicAddwithOffset("Duplicate preference key name. {0}", 4);
      this.DynamicAddwithOffset("Duplicate preference key UID. {0}", 5);
      this.DynamicAddwithOffset("Cannot delete preference key as user preferences exist. {0}", 6);
      this.DynamicAddwithOffset("Unable to delete preference key. {0}", 7);
      this.DynamicAddwithOffset("Access denied.", 8);
      this.DynamicAddwithOffset("Missing user UID.", 9);
      this.DynamicAddwithOffset("Unable to create user preference. {0}", 10);
      this.DynamicAddwithOffset("Unable to update user preference. {0}", 11);
      this.DynamicAddwithOffset("Unable to delete user preference. {0}", 12);
      this.DynamicAddwithOffset("User preference already exists. {0}", 13);
    }
  }
}
