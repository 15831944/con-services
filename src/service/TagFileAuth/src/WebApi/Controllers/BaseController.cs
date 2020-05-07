﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Common controller base.
  /// </summary>
  public abstract class BaseController : Controller
  {
    protected readonly IConfigurationStore _configStore;
    protected ICwsAccountClient _cwsAccountClient;
    protected IProjectInternalProxy _projectProxy;
    protected IDeviceInternalProxy _deviceProxy;
    protected IDictionary<string, string> _customHeaders;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore, IHttpContextAccessor httpContextAccessor,
      ICwsAccountClient cwsAccountClient, IProjectInternalProxy projectProxy, IDeviceInternalProxy deviceProxy,
      ITPaaSApplicationAuthentication authorization)
    {
      _configStore = configStore;
      _cwsAccountClient = cwsAccountClient;
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;

      // this is to pass along the requestID created on startup in the RequestIDMiddleware 
      _customHeaders = authorization.CustomHeaders().MergeDifference(httpContextAccessor.HttpContext.Request.Headers.GetCustomHeaders(new string[] { HeaderConstants.X_VSS_REQUEST_ID }));
    }
  }
}
