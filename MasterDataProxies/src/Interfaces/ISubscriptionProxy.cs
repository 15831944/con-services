﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.Common.Interfaces
{
    public interface ISubscriptionProxy
    {
        Task AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
            IDictionary<string, string> customHeaders = null);
    }
}

