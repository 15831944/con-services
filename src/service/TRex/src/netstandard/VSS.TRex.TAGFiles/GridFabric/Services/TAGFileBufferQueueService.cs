﻿using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
  /// <summary>
  /// Service metaphor providing access and management control over designs stored for site models
  /// </summary>
  public class TAGFileBufferQueueService : IService, ITAGFileBufferQueueService, IBinarizable, IFromToBinary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileBufferQueueService>();

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do
    /// </summary>
    private const int kTAGFileBufferQueueServiceCheckIntervalMS = 1000;

    /// <summary>
    /// Flag set then Cancel() is called to instruct the service to finish operations
    /// </summary>
    private bool aborted;

    /// <summary>
    /// The event wait handle used to mediate sleep periods between operation epochs of the service
    /// </summary>
    private EventWaitHandle waitHandle;

    /// <summary>
    /// Default no-args constructor that tailors this service to apply to TAG processing node in the mutable data grid
    /// </summary>
    public TAGFileBufferQueueService()
    {
    }

    /// <summary>
    /// Initializes the service ready for accessing buffered TAG files and providing them to processing contexts
    /// </summary>
    /// <param name="context"></param>
    public void Init(IServiceContext context)
    {
      Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} initializing");
    }

    /// <summary>
    /// Executes the life cycle of the service until it is aborted
    /// </summary>
    /// <param name="context"></param>
    public void Execute(IServiceContext context)
    {
      Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} starting executing");

      aborted = false;
      waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

      // Get the ignite grid and cache references

      IIgnite _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());

      if (_ignite == null)
      {
        Log.LogError("Ignite reference in service is null - aborting service execution");
        return;
      }

      var queueCache = _ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

      TAGFileBufferQueueItemHandler handler = new TAGFileBufferQueueItemHandler();

      // Construct the continuous query machinery
      // Set the initial query to return all elements in the cache
      // Instantiate the queryHandle and start the continuous query on the remote nodes
      // Note: Only cache items held on this local node will be handled here
      // var = IContinuousQueryHandle<ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>>
      using (var queryHandle = queueCache.QueryContinuous
      (qry: new ContinuousQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener(handler)) {Local = true},
        initialQry: new ScanQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> {Local = true}))
      {
        // Perform the initial query to grab all existing elements and add them to the grouper
        foreach (var item in queryHandle.GetInitialQueryCursor())
        {
          handler.Add(item.Key);
        }

        // Cycle looking for new work to do as TAG files arrive until aborted...
        do
        {
          waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
        } while (!aborted);
      }

      Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} completed executing");
    }

    /// <summary>
    /// Cancels the current operation context of the service
    /// </summary>
    /// <param name="context"></param>
    public void Cancel(IServiceContext context)
    {
      Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} cancelling");

      aborted = true;
      waitHandle?.Set();
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, readVersionNumber);
    }
  }
}
