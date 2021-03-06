﻿using System;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;

namespace CCSS.Productivity3D.Preferences.Abstractions.Interfaces
{
  public interface IPreferenceRepository
  {
    Task<int> StoreEvent(IPreferenceEvent evt);

    Task<PrefKeyDataModel> GetPreferenceKey(Guid? prefKeyUID = null, string prefKeyName = null);
    Task<UserPrefKeyDataModel> GetUserPreference(Guid userUID, string prefKeyName);
    Task<bool> UserPreferenceExistsForKey(Guid prefKeyUID);
  }
}
