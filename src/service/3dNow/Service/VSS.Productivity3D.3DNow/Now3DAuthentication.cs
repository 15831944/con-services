﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Now3D
{
  public class Now3DAuthentication : TIDAuthentication
  {
    public Now3DAuthentication(RequestDelegate next, ICwsAccountClient accountProxy, IConfigurationStore store, ILoggerFactory logger, IEntitlementProxy entitlementProxy, IServiceExceptionHandler serviceExceptionHandler) 
      : base(next, accountProxy, store, logger, entitlementProxy, serviceExceptionHandler)
    { }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return false;
    }
  }
}
