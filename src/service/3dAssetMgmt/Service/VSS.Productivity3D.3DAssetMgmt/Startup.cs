﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.AssetMgmt3D
{
  public class Startup : BaseStartup
  {
    public Startup(IHostingEnvironment env) : base(env, "3d-assetmanagement")
    {
    }

    public override string ServiceName => "3D Asset Management API";

    public override string ServiceDescription => "A service to match 3D assets with telematics and manage legacy asset identifiers in 3D";

    public override string ServiceVersion => "v1";

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvc();

      // Required for authentication
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddTransient<IProjectListProxy, ProjectV4ListServiceDiscoveryProxy>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();

      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, AssetMgmt3DExecutionStates>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();

      services.AddServiceDiscovery();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<AssetMgmt3DAuthentication>();
      app.UseMvc();
    }
  }
}