﻿using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.FIlters;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Base Startup class which takes care of a lot of repetitive setup, such as logger, swagger etc
  /// </summary>
  public abstract class BaseStartup
  {
    //Backing field
    private ILogger _logger;
    private IConfigurationStore _configuration;

    protected IServiceCollection Services { get; private set; }
    protected ServiceProvider ServiceProvider { get; private set; }

    protected IConfigurationStore Configuration
    {
      get { return _configuration ??= new GenericConfiguration(new NullLoggerFactory(), ServiceProvider.GetRequiredService<IConfigurationRoot>()); }
      set => _configuration = value;
    }

    /// <summary>
    /// Gets the ILogger type used for logging.
    /// </summary>
    protected ILogger Log
    {
      get { return _logger ??= ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().Name); }
      set => _logger = value;
    }

    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    public abstract string ServiceName { get; }

    /// <summary>
    /// The service description, used for swagger documentation
    /// </summary>
    public abstract string ServiceDescription { get; }

    /// <summary>
    /// The service version, used for swagger documentation
    /// </summary>
    public abstract string ServiceVersion { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      var corsPolicies = GetCors();
      services.AddCors(options =>
      {
        foreach (var (name, corsPolicy) in corsPolicies)
        {
          options.AddPolicy(name, corsPolicy);
        }
      });

      services.AddHttpClient();

      services.AddCommon<BaseStartup>(ServiceName, ServiceDescription, ServiceVersion);
      services.AddJaeger(ServiceName);
      services.AddServiceDiscovery();

      services.AddMvcCore(config =>
      {
        // for jsonProperty validation
        config.Filters.Add(new ValidationFilterAttribute());
      }).AddMetricsCore();

      var metrics = AppMetrics.CreateDefaultBuilder()
        .Configuration.Configure(options =>
        {
          options.Enabled = true;
          options.ReportingEnabled = true;
          options.AddServerTag();
          options.AddAppTag(appName: ServiceName);
        })
        .OutputMetrics.AsPrometheusPlainText()
        .Build();

      ServiceProvider = services.BuildServiceProvider();
      ConfigureAdditionalServices(services);

      services.AddMvc(
        config =>
        {
          config.Filters.Add(new ValidationFilterAttribute());
          config.EnableEndpointRouting = false;
        }
      );

      services.AddControllers().AddNewtonsoftJson(options =>
      {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
      });

      services.AddMetrics(metrics);
      services.AddMetricsTrackingMiddleware();
      services.AddMetricsEndpoints(options =>
      {
        options.MetricsEndpointOutputFormatter =
          metrics.OutputMetricsFormatters.GetType<MetricsPrometheusTextOutputFormatter>();
      });

      Services = services;
      ServiceProvider = services.BuildServiceProvider();

      Configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();

      services.AddMetricsReportingHostedService();

      StartServices(ServiceProvider);
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This method gets called by the run time
    /// </summary>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseRouting();

      foreach (var corsPolicyName in GetCors().Select(c => c.Item1))
      {
        app.UseCors(corsPolicyName);
      }

      app.UseMetricsAllEndpoints();
      app.UseMetricsAllMiddleware();
      app.UseCommon(ServiceName);

      if (Configuration.GetValueBool("newrelic") == true)
      {
        NewRelicMiddleware.ServiceName = ServiceName;
        app.UseMiddleware<NewRelicMiddleware>();
        Log.LogInformation("NewRelic is enabled");
      }

      Services.AddSingleton(loggerFactory);
      ConfigureAdditionalAppSettings(app, env, loggerFactory);

      app.UseEndpoints(AddEndpoints);
    }

    /// <summary>
    /// Add endpoints, this should only be called once
    /// Allow for different applications to extend endpoints if needed
    /// </summary>
    protected virtual void AddEndpoints(IEndpointRouteBuilder endpoints)
    {
      endpoints.MapControllers();
    }

    /// <summary>
    /// Start any services once the service provider has been built
    /// </summary>
    protected virtual void StartServices(IServiceProvider serviceProvider)
    { }

    /// <summary>
    /// Extra configuration that would normally be in ConfigureServices
    /// This is useful for binding interfaces to implementations
    /// </summary>
    protected abstract void ConfigureAdditionalServices(IServiceCollection services);

    /// <summary>
    /// Extra app and env setup options
    /// Useful for adding ASP related options, such as filter MiddleWhere
    /// </summary>
    protected abstract void ConfigureAdditionalAppSettings(IApplicationBuilder app,
      IWebHostEnvironment env,
      ILoggerFactory factory);

    /// <summary>
    /// Get the required CORS Policies, by default the VSS Specific cors policy is added
    /// If you extend, call the base method unless you have a good reason.
    /// </summary>
    protected virtual IEnumerable<(string, CorsPolicy)> GetCors()
    {
      yield return ("VSS", new CorsPolicyBuilder().AllowAnyOrigin()
        .WithHeaders(HeaderConstants.ORIGIN,
          HeaderConstants.X_REQUESTED_WITH,
          HeaderConstants.CONTENT_TYPE,
          HeaderConstants.ACCEPT,
          HeaderConstants.AUTHORIZATION,
          HeaderConstants.X_VISION_LINK_CUSTOMER_UID,
          HeaderConstants.X_VISION_LINK_USER_UID,
          HeaderConstants.X_JWT_ASSERTION,
          HeaderConstants.X_VISION_LINK_CLEAR_CACHE,
          HeaderConstants.CACHE_CONTROL)
        .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE")
        .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
        .Build());
    }
  }
}
