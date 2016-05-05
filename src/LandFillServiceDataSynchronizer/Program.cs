﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Topshelf;
using Topshelf.Runtime;

namespace LandFillServiceDataSynchronizer
{
  class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void ConfigureLogging()
    {
      var layout = new PatternLayout("%utcdate [%thread] %-5level %method - %message%newline");
      var appender = new ConsoleAppender(layout);

      FileAppender appenderF = new FileAppender();
      appenderF.Name = "logfile";
      appenderF.File = "LandFillServiceServiceSync.log";
      appenderF.AppendToFile = true;

      PatternLayout layoutF = new PatternLayout();
      layoutF.ConversionPattern = "%utcdate [%thread] %-5level %method - %message%newline";
      layoutF.ActivateOptions();

      appenderF.Layout = layoutF;
      appenderF.ActivateOptions();

      appender.Threshold = Level.All;
      layout.ActivateOptions();
      appender.ActivateOptions();
      BasicConfigurator.Configure(appender);

      Logger l = (Logger)Log.Logger;

      l.AddAppender(appenderF);
    }

    static void Main(string[] args)
    {
      ConfigureLogging();
      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_LandfillDataSyncService");
        c.SetDisplayName("_LandfillDataSyncService");
        c.SetDescription("Service for syncing data between landfill app and raptor");
        c.RunAsLocalSystem();
        c.StartAutomatically();
        c.EnableServiceRecovery(cfg =>
        {
          cfg.RestartService(1);
          cfg.RestartService(1);
          cfg.RestartService(1);
        });
        c.Service<ServiceController>(svc =>
        {
          svc.ConstructUsing(ServiceFactory);
          svc.WhenStarted(s => s.Start());
          svc.WhenStopped(s => s.Stop());
        });
        c.UseLog4Net();
      });

      if (exitCode == TopshelfExitCode.Ok)
      {
        Log.InfoFormat("Lanfill datasync service - {0}", exitCode);
      }
      else
      {
        Log.DebugFormat("Lanfill datasync service - {0}", exitCode);
      }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      if (e.IsTerminating)
      {
        Log.Fatal("A fatal unhandled exception has occurred", e.ExceptionObject as Exception);
      }
      else
      {
        Log.Error("A non-fatal unhandled exception has occurred", e.ExceptionObject as Exception);
      }
    }

    private static ServiceController ServiceFactory(HostSettings settings)
    {
      return new ServiceController();
    }
  }

  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static Timer SyncTimer = null;

    private static void ConfigureLogging()
    {
      var layout = new PatternLayout("%utcdate [%thread] %-5level %method - %message%newline");

      FileAppender appenderF = new FileAppender();
      appenderF.Name = "logfile";
      appenderF.File = "LandFillServiceSync.log";
      appenderF.AppendToFile = true;

      PatternLayout layoutF = new PatternLayout();
      layoutF.ConversionPattern = "%utcdate [%thread] %-5level %method - %message%newline";
      layoutF.ActivateOptions();

      appenderF.Layout = layoutF;
      appenderF.ActivateOptions();
      layout.ActivateOptions();
      Logger l = (Logger)Log.Logger;
      l.AddAppender(appenderF);
    }

    public void Start()
    {
      ConfigureLogging();
      var dataSync = new DataSynchronizer(Log);     

      Log.Debug("Starting service...");
      SyncTimer = new System.Threading.Timer(dataSync.RunUpdateDataFromRaptor);
      var sleepTime = ConfigurationManager.AppSettings["HoursToSleepForVolumes"];
      var hoursToSleep = string.IsNullOrEmpty(sleepTime) ? 2 : double.Parse(sleepTime);
      SyncTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromHours(hoursToSleep));
    }

    public void Stop()
    {
    }
  }
}
