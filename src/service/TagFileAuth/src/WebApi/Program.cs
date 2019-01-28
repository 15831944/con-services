﻿using System.IO;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.WebApi.Common;


namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Application entry point.
  /// </summary>
  public class Program
  {
    /// <summary>
    /// Default constructory.
    /// </summary>
    public static void Main(string[] args)
    {
      var kestrelConfig = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .Build();

      var host = new WebHostBuilder()
        .UseKestrel()
        .UseLibuv(opts =>
        {
          opts.ThreadCount = 32;
        })
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseConfiguration(kestrelConfig)
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LoggerRepoName;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Debug);
          builder.AddConfiguration(kestrelConfig);
        })
        .UsePrometheus()
        .UseStartup<Startup>()
        .Build();

      ThreadPool.SetMaxThreads(1024, 2048);
      ThreadPool.SetMinThreads(1024, 2048);

      //Check how many requests we can execute
      ServicePointManager.DefaultConnectionLimit = 128;
      host.Run();
    }
  }
}
