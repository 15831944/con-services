﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MegalodonSvc
{

  public class ServiceB : IHostedService, IDisposable
  {
    private int executionCount = 0;
    private readonly ILogger<ServiceB> _logger;
    private Timer _timer;

    public ServiceB(ILogger<ServiceB> logger)
    {
      _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service running.");

      _timer = new Timer(DoWork, null, TimeSpan.Zero,
          TimeSpan.FromSeconds(5));

      return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
      executionCount++;

      _logger.LogInformation(
          "Timed Hosted Service is working. Count: {Count}", executionCount);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service is stopping.");

      _timer?.Change(Timeout.Infinite, 0);

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }

  }
}