﻿using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Caches;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
  /// <summary>
  /// Service metaphor providing access and management control over designs stored for site models
  /// </summary>
  [Serializable]
  public class SegmentRetirementQueueService : IService, ISegmentRetirementQueueService
  {
    [NonSerialized] private static readonly ILogger Log = Logging.Logger.CreateLogger<SegmentRetirementQueueService>();

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do. Set to 30 seconds.
    /// </summary>
    private const int kSegmentRetirementQueueServiceCheckIntervalMS = 30000;

    /// <summary>
    /// Flag set then Cancel() is called to instruct the service to finish operations
    /// </summary>
    [NonSerialized] private bool aborted;

    /// <summary>
    /// The event wait handle used to mediate sleep periods between operation epochs of the service
    /// </summary>
    [NonSerialized] private EventWaitHandle waitHandle;

    public TimeSpan retirementAge = new TimeSpan(0, 10, 0); // Set to 10 minutes as a maximum consistency window

    /// <summary>
    /// Default no-args constructor that tailors this service to apply to TAG processing node in the mutable data grid
    /// </summary>
    public SegmentRetirementQueueService()
    {
    }

    /// <summary>
    /// Initialises the service ready for accessing segment keys
    /// </summary>
    /// <param name="context"></param>
    public void Init(IServiceContext context)
    {
      Log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} initialising");
    }

    /// <summary>
    /// Executes the life cycle of the service until it is aborted
    /// </summary>
    /// <param name="context"></param>
    public void Execute(IServiceContext context)
    {
      Log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} starting executing");

      aborted = false;
      waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

      // Get the ignite grid and cache references

      IIgnite _ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());

      if (_ignite == null)
      {
        Log.LogError("Ignite reference in service is null - aborting service execution");
        return;
      }

      var queueCache = _ignite.GetCache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

      SegmentRetirementQueue queue = new SegmentRetirementQueue(DIContext.Obtain<ITRexGridFactory>().Grid(DIContext.Obtain<ISiteModels>().StorageProxy.Mutability));
      SegmentRetirementQueueItemHandler handler = new SegmentRetirementQueueItemHandler();

      // Cycle looking for new work to do until aborted...
      do
      {
        // Retrieve the list of segments to be retired
        var retirees = queue.Query(DateTime.Now - retirementAge);

        // Pass the list to the handler for action
        if ((retirees?.Count ?? 0) > 0)
          handler.Process(retirees);

        waitHandle.WaitOne(kSegmentRetirementQueueServiceCheckIntervalMS);
      } while (!aborted);

      Log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} completed executing");
    }

    /// <summary>
    /// Cancels the current operation context of the service
    /// </summary>
    /// <param name="context"></param>
    public void Cancel(IServiceContext context)
    {
      Log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} cancelling");

      aborted = true;
      waitHandle?.Set();
    }
  }
}
