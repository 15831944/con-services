﻿using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.Common.Authentication
{
  /// <summary>
  /// Tile service Authentication middleware
  /// </summary>
  public class TileAuthentication : TIDAuthentication
  {
    private readonly IProjectProxy projectProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="TileAuthentication"/> class.
    /// </summary>
    public TileAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
      this.projectProxy = projectProxy;
    }

    protected override List<string> IgnoredPaths => new List<string> { "/swagger/", "/testtile/" };

    /// <summary>
    /// Create 3dpm principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName, 
      string userEmail, bool isApplicationContext, IDictionary<string, string> contextHeaders, string tpaasApplicationName = "")
    {
      //Delegate customer->project association resolution to the principal object for now as it has execution context and can invalidate cache if required
      // note that userUid may actually be the ApplicationId if isApplicationContext
      return new TilePrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, projectProxy, contextHeaders, tpaasApplicationName);
    }

  }
}
