﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public class CwsUserClient : CwsProfileManagerClient, ICwsUserClient
  {
    public CwsUserClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    ///   user token
    ///   userId is used for our caching. It must match the userId in the JWT. JWT should be in the customHeaders and should be a user token, not application token.
    /// used by our preferenceService to get details from ProfileX via cws
    /// </summary>
    public Task<UserResponseModel> GetUser(Guid userId, IHeaderDictionary customHeaders = null)
    {
      return GetData<UserResponseModel>("/users/me", null, userId, null, customHeaders);
    }
  }
}
