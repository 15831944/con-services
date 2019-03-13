﻿using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using System;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {
    const string LIBUV_THREAD_COUNT = "LIBUV_THREAD_COUNT";
    const string MAX_WORKER_THREADS = "MAX_WORKER_THREADS";
    const string MAX_IO_THREADS = "MAX_IO_THREADS";
    const string MIN_WORKER_THREADS = "MAX_WORKER_THREADS";
    const string MIN_IO_THREADS = "MIN_IO_THREADS";
    const string DEFAULT_CONNECTION_LIMIT = "DEFAULT_CONNECTION_LIMIT";

    /// <summary>
    /// Default program entry point.
    /// </summary>
    public static void Main(string[] args)
    {
      var isService = args.Contains("--service");

      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .Build();

      //Log4NetProvider.RepoName = Startup.LoggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(Startup.LoggerRepoName);
      //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);

      var libuvConfigured = int.TryParse(Environment.GetEnvironmentVariable(LIBUV_THREAD_COUNT), out var libuvThreads);
      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel(opts =>
        {
          opts.Limits.MaxResponseBufferSize = 131072;//128K for large exports (default is 64K)
        })
        //.UseUrls("http://127.0.0.1:5002") //DO NOT REMOVE (used for local debugging of long running veta exports)
        .UseLibuv(opts =>
        {
          if (libuvConfigured)
          {
            opts.ThreadCount = libuvThreads;
          }

        })
        .UseContentRoot(pathToContentRoot)
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LoggerRepoName;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Debug);
          builder.AddConfiguration(config);
        })
        .UsePrometheus()
        .UseStartup<Startup>()
        .Build();

      var log = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
      Console.WriteLine("Productivity3D service starting");
      log.LogInformation("Productivity3D service starting");
      log.LogInformation($"Num Libuv Threads = {(libuvConfigured ? libuvThreads.ToString() : "Default")}");
      
      if (int.TryParse(Environment.GetEnvironmentVariable(MAX_WORKER_THREADS), out var maxWorkers) &&
          int.TryParse(Environment.GetEnvironmentVariable(MAX_IO_THREADS), out var maxIo))
      {
        ThreadPool.SetMaxThreads(maxWorkers, maxIo);
        log.LogInformation($"Max Worker Threads = {maxWorkers}");
        log.LogInformation($"Max IO Threads = {maxIo}");
      }
      else
      {
        log.LogInformation($"Max Worker Threads = Default");
        log.LogInformation($"Max IO Threads = Default");
      }

      if (int.TryParse(Environment.GetEnvironmentVariable(MIN_WORKER_THREADS), out var minWorkers) &&
          int.TryParse(Environment.GetEnvironmentVariable(MIN_IO_THREADS), out var minIo))
      {
        ThreadPool.SetMinThreads(minWorkers, minIo);
        log.LogInformation($"Min Worker Threads = {minWorkers}");
        log.LogInformation($"Min IO Threads = {minIo}");
      }
      else
      {
        log.LogInformation($"Min Worker Threads = Default");
        log.LogInformation($"Min IO Threads = Default");
      }

      if (int.TryParse(Environment.GetEnvironmentVariable(DEFAULT_CONNECTION_LIMIT), out var connectionLimit))
      {
        //Check how many requests we can execute
        ServicePointManager.DefaultConnectionLimit = connectionLimit;
        log.LogInformation($"Default connection limit = {connectionLimit}");
      }
      else
      {
        log.LogInformation($"Default connection limit = Default");
      }

      if (!isService)
        host.Run();
      else
        host.RunAsService();
    }
  }
}
