﻿using CoreX.Interfaces;
using CoreX.Wrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Webtools
{
  public class Startup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="env">The env.</param>
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
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    private IConfigurationRoot Configuration { get; }


    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

      services.AddControllers().AddNewtonsoftJson(options =>
      {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
      });

      services.AddSingleton(new VSS.TRex.IO.RecyclableMemoryStreamManager
      {
        // Allow up to 256Mb worth of freed small blocks used by the recyclable streams for later reuse
        // NOte: The default value for this setting is zero which means every block allocated to a
        // recyclable stream is freed when the stream is disposed.
        MaximumFreeSmallPoolBytes = 256 * 1024 * 1024
      });

      //services.AddCommon<Startup>(SERVICE_TITLE, "API for TRex Gateway");

      //Set up logging etc. for TRex
      DIContext.Inject(services.BuildServiceProvider());

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      //Set up configuration for TRex
      DIContext.Inject(services.BuildServiceProvider());

      services.AddSingleton<ICoreXWrapper, CoreXWrapper>();
      TRexGridFactory.AddGridFactoriesToDI(services);
      Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI(services);

      DIContext.Inject(services.BuildServiceProvider());

      services.AddTransient<ISiteModelMetadata>(factory => new SiteModelMetadata()); 
      services.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory());

      services.AddTransient<ISiteModels>(factory => SwitchableGridContext.SwitchableSiteModelsContext());
      services.AddSingleton<ISiteModelFactory>(new SiteModelFactory());
      services.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory());
      services.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces());
      services.AddTransient<IDesigns>(factory => new Designs.Storage.Designs());
      services.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory());
      services.AddSingleton<IMutabilityConverter>(new MutabilityConverter());

      services.AddSingleton(new ImmutableClientServer("Webtools-Immutable"));
      services.AddSingleton(new MutableClientServer("Webtools-Mutable"));

      // Register the listener for site model attribute change notifications
      services.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName()));
      services.AddSingleton<ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender());
      services.AddSingleton<IDesignManager>(factory => new DesignManager(StorageMutability.Immutable));
      services.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager(StorageMutability.Immutable));

      services.AddTransient<IAlignments>(factory => new Alignments.Alignments());
      services.AddSingleton<IAlignmentManager>(factory => new AlignmentManager(StorageMutability.Immutable));

      services.AddSingleton<ISiteModelMetadataManager>(factory => new SiteModelMetadataManager(StorageMutability.Mutable));
      services.AddSingleton<ITransferProxyFactory>(factory => new TransferProxyFactory(factory.GetRequiredService<IConfigurationStore>(), factory.GetRequiredService<ILoggerFactory>()));

      ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI(services);

      services.AddSingleton<IExistenceMaps>(factory => new ExistenceMaps.ExistenceMaps());
      
      DIContext.Inject(services.BuildServiceProvider());

      // In production, the Angular files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "ClientApp/dist";
      });

      // Start listening to site model change notifications
      DIContext.Obtain<ISiteModelAttributesChangedEventListener>().StartListening();
    }
    
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseRouting();

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseSpaStaticFiles();

      app.UseEndpoints(endpoints => endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}"));

      app.UseSpa(spa =>
      {
        // To learn more about options for serving an Angular SPA from ASP.NET Core,
        // see https://go.microsoft.com/fwlink/?linkid=864501

        spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment())
        {
          spa.UseAngularCliServer(npmScript: "start");
        }
      });
    }
  }
}
