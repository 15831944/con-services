﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.Notification.Executors;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.WebApi.Models.Services
{
  public interface IEnqueueItem<in T>
  {
    bool EnqueueItem(T item);
  }

  public class AddFileProcessingService : IHostedService, IEnqueueItem<ProjectFileDescriptor>
  {
    private readonly ConcurrentQueue<ProjectFileDescriptor> queue = new ConcurrentQueue<ProjectFileDescriptor>();
    private ILogger<AddFileProcessingService> log;
    private readonly IConfigurationStore configServiceStore;
    private readonly IFileRepository fileRepo;
    private readonly IASNodeClient raptorServiceClient;
    private readonly ILoggerFactory loggingFactory;
    private readonly ITileGenerator tileServiceGenerator;
    private CancellationToken token;
    private bool stopRequested = false;
    private SemaphoreSlim stopSemaphore = new SemaphoreSlim(1);

    public AddFileProcessingService(ILogger<AddFileProcessingService> logger, ILoggerFactory logFactory,
      IConfigurationStore configService, IFileRepository repositoryService, IASNodeClient raptorService,
      ITileGenerator tileService)
    {
      log = logger;
      configServiceStore = configService;
      fileRepo = repositoryService;
      loggingFactory = logFactory;
      raptorServiceClient = raptorService;
      tileServiceGenerator = tileService;
    }

    private async Task<Notification.Models.AddFileResult> ProcessItem(ProjectFileDescriptor file)
    {
      var executor = RequestExecutorContainerFactory.Build<AddFileExecutor>(loggingFactory, raptorServiceClient, null,
        configServiceStore, fileRepo, tileServiceGenerator);
      var result = (await executor.ProcessAsync(file) as Notification.Models.AddFileResult);
      log.LogInformation($"Processed file {file.File.fileName} with result {JsonConvert.SerializeObject(result)}");
      var eventAttributes = new Dictionary<string, object>
      {
        {"file", file.File.fileName},
        {"status", result.Code.ToString() },
        {"result", result.Message }
      };

      NewRelic.Api.Agent.NewRelic.RecordCustomEvent("3DPM_Request_files", eventAttributes);
      return result;
    }

    public void StartSpinCycle(CancellationToken cancellationToken)
    {
      log.LogInformation("Starting file processing thread");
      token = cancellationToken;
      var spinnerThread = new Thread(StartThread);
      spinnerThread.Start();
    }

    private void StartThread()
    {
      stopSemaphore.Wait(token);
      while (!token.IsCancellationRequested && !stopRequested)
      {
        if (queue.Count > 0)
        {
          if (queue.TryDequeue(out var descriptor))
          {
            log.LogInformation($"Processing file {JsonConvert.SerializeObject(descriptor)}");
            var task = ProcessItem(descriptor);
          }
        }
        else
          Thread.Sleep(500);
      }
      stopSemaphore.Release();
      log.LogInformation($"Stopped file processing thread");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      StartSpinCycle(cancellationToken);
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      log.LogInformation($"Stopping file processing thread");
      stopRequested = true;
      return stopSemaphore.WaitAsync(cancellationToken);
    }

    public bool EnqueueItem(ProjectFileDescriptor item)
    {
      queue.Enqueue(item);
      return true;
    }
  }
}
