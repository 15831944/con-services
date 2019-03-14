﻿using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Implements a management thread that periodically checks the contexts in the cache for ones
  /// that are marked for removal and removes them. This is done in a single mutually exclusive lock
  /// within the main cache.
  /// </summary>
  public class TRexSpatialMemoryCacheContextRemover : IDisposable
  {
    private static readonly ILogger log = Logging.Logger.CreateLogger<TRexSpatialMemoryCacheContextRemover>();

    private readonly ITRexSpatialMemoryCache _cache;
    private Thread _removalThread;
    private readonly int _sleepTimeMS;
    private readonly int _removalWaitTimeSeconds;
    private bool _cancelled;

    private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

    private void PerformRemovalOperation()
    {
      while (!_cancelled)
      {
        try
        {
          // Instruct the cache to perform the cleanup...
          // Wait a time period minutes to remove items marked for removal
          _cache.RemoveContextsMarkedForRemoval(_removalWaitTimeSeconds);
        }
        catch (Exception e)
        {
          log.LogError(e, "Exception thrown during RemoveContextsMarkedForRemoval()");
        }

        if (!_cancelled)
          _waitHandle.WaitOne(_sleepTimeMS);
      }
    }

    public void StopRemovalOperations()
    {
      _cancelled = true;
      _waitHandle.Set();
    }

    public void Dispose()
    {
      _cancelled = true;
      _waitHandle.Set();
      _removalThread = null;
    }

    public TRexSpatialMemoryCacheContextRemover(ITRexSpatialMemoryCache cache, int sleepTimeSeconds, int removalWaitTimeSeconds)
    {
      _cache = cache;
      _removalWaitTimeSeconds = removalWaitTimeSeconds;
      _sleepTimeMS = sleepTimeSeconds * 1000;
      _removalThread = new Thread(PerformRemovalOperation);
      _removalThread.Start();
    }
  }
}
