﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Now3D
{
  public class Startup : BaseStartup
  {
    public Startup(IHostingEnvironment env) : base(env, "3d-Now")
    {
    }

    public override string ServiceName => "3D Now Composite API";

    public override string ServiceDescription => "A service to manage requests to multiple services for each of use for external customers";

    public override string ServiceVersion => "v1";

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvc();

      // Required for authentication
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddTransient<IProjectListProxy, ProjectListProxy>();
      services.AddTransient<IFileListProxy, FileListProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<IFilterServiceProxy, FilterServiceProxy>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, Now3DExecutionStates>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<Now3DAuthentication>();
      app.UseMvc();
    }
  }
}
