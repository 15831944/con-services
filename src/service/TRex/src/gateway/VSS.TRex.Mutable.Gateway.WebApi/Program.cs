﻿using System;
using Microsoft.AspNetCore.Hosting;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Gateway.Common.Converters;
using VSS.WebApi.Common;

namespace VSS.TRex.Mutable.Gateway.WebApi
{
  public class Program
  {
    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.TAGFiles.Executors.SubmitTAGFileExecutor),
        typeof(VSS.TRex.TAGFiles.GridFabric.NodeFilters.TAGProcessorRoleBasedNodeFilter),
        typeof(VSS.TRex.SiteModelChangeMaps.GridFabric.NodeFilters.SiteModelChangeProcessorRoleBasedNodeFilter),
        typeof(VSS.TRex.TAGFiles.GridFabric.ComputeFuncs.SubmitTAGFileComputeFunc)
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    public static void Main(string[] args)
    {
      try
      {
        EnsureAssemblyDependenciesAreLoaded();

        AppDomain.CurrentDomain.UnhandledException += TRexAppDomainUnhandledExceptionHandler.Handler;

        var webHost = BuildWebHost(args);

        webHost.Run();
      }
      catch (Exception e)
      {
        Console.WriteLine($"Unhandled exception: {e}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
      }
    }

    public static IWebHost BuildWebHost(string[] args)
    {

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      return new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost()
          .UseStartup<Startup>()
          .Build();
      });
    }
  }
}
