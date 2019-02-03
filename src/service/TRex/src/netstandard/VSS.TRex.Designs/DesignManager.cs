﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// Service metaphor providing access and management control over designs stored for site models
  /// </summary>
  public class DesignManager : IDesignManager
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignManager>();

    private readonly IStorageProxy WriteStorageProxy;
    private readonly IStorageProxy ReadStorageProxy;

    private const string DESIGNS_STREAM_NAME = "Designs";

    /// <summary>
    /// Default no-arg constructor that sets the grid and cache name to default values
    /// </summary>
    public DesignManager() 
    {
       WriteStorageProxy = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();
       ReadStorageProxy = DIContext.Obtain<ISiteModels>().StorageProxy;
    }

    /// <summary>
    /// Loads the set of designs for a site model. If none exist an empty list is returned.
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <returns></returns>
    private IDesigns Load(Guid siteModelID)
    {
      try
      {
        ReadStorageProxy.ReadStreamFromPersistentStore(siteModelID, DESIGNS_STREAM_NAME, FileSystemStreamType.Designs, out MemoryStream ms);

        IDesigns designs = DIContext.Obtain<IDesigns>();
        if (ms != null)
        {
          using (ms)
          {
            designs.FromStream(ms);
          }
        }

        return designs;
      }
      catch (KeyNotFoundException)
      {
        /* This is OK, the element is not present in the cache yet */
      }
      catch (Exception e)
      {
        throw new TRexException("Exception reading designs cache element from Ignite", e);
      }

      return null;
    }

    /// <summary>
    /// Stores the list of designs for a site model
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="designs"></param>
    private void Store(Guid siteModelID, IDesigns designs)
    {
      try
      {
        WriteStorageProxy.WriteStreamToPersistentStore(siteModelID, DESIGNS_STREAM_NAME, FileSystemStreamType.Designs, designs.ToStream(), designs);
        WriteStorageProxy.Commit();

        // Notify the mutable and immutable grid listeners that attributes of this site model have changed
        var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
        sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyImmutable, siteModelID, designsChanged: true);
      }
      catch (Exception e)
      {
        throw new TRexException("Exception writing updated designs cache element to Ignite", e);
      }
    }

    /// <summary>
    /// Add a new design to a site model
    /// </summary>
    /// <param name="SiteModelID"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    public IDesign Add(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents)
    {
      IDesigns designs = Load(SiteModelID);
      IDesign result = designs.AddDesignDetails(designDescriptor.DesignID, designDescriptor, extents);
      Store(SiteModelID, designs);

      return result;
    }

    /// <summary>
    /// Returns the list of all designs known for the site model
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <returns></returns>
    public IDesigns List(Guid siteModelID)
    {
      Log.LogInformation($"Listing designs from {siteModelID}");

      return Load(siteModelID);
    }

    /// <summary>
    /// Remove a given design from a site model
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="designID"></param>
    /// <returns></returns>
    public bool Remove(Guid siteModelID, Guid designID)
    {
      IDesigns designs = Load(siteModelID);
      bool result = designs.RemoveDesign(designID);
      Store(siteModelID, designs);

      return result;
    }
  }
}

