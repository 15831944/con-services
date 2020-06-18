﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using MockProjectWebApi.Utils;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace MockProjectWebApi
{
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Mock Project Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to mock all CRUD requests within the 3DP service architecture.";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public new IConfigurationRoot Configuration { get; }

    /// <inheritdoc />
    public Startup(IWebHostEnvironment env)
    {
      var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container
    /// </summary>
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
          .WithMethods("OPTIONS", "TRACE", "GET", "POST", "DELETE", "PUT", "HEAD"));
      });

      services.AddMvc();
      services.AddTransient<IWebRequest, GracefulWebRequest>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IFiltersService, FiltersService>();
      services.AddSingleton<IImportedFilesService, ImportedFilesService>();
      services.AddSingleton<IProjectService, ProjectService>();
      //services.AddTransient<IWebRequest, GracefulWebRequest>();
      //services.AddTransient<IEntitlementProxy, EntitlementProxy>(); // required for license validation
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {
      //Enable CORS before TID so OPTIONS works without authentication
      app.UseExceptionDummyPostMiddleware();
    }
  }
}
