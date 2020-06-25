﻿using System;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications.Changes;

namespace VSS.Productivity3D.Push.Clients.Notifications
{
  public class CacheInvalidationService
  {
    private readonly IDataCache cache;

    public CacheInvalidationService(IDataCache cache)
    {
      this.cache = cache;
    }

    [Notification(NotificationUidType.Project, ProjectChangedNotification.PROJECT_CHANGED_KEY)]
    [Notification(NotificationUidType.Customer, CustomerChangedNotification.CUSTOMER_CHANGED_KEY)]
    [Notification(NotificationUidType.User, UserChangedNotification.USER_CHANGED_KEY)]
    public void InvalidateTags(Guid guid)
    {
      cache.RemoveByTag(guid.ToString());
    }

    // This must match the cache invalidation signature, of void method(object)
    [Notification(NotificationUidType.CacheUpdate, CacheChangeNotification.CACHE_CHANGED_KEY)]
    public void InvalidateCacheItem(object o)
    {
      if(o != null)
        cache.RemoveByTag(o.ToString());
    }
  }
}
