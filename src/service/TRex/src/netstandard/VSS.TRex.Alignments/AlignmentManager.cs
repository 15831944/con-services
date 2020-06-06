﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Alignments
{
  /// <summary>
  /// The Alignment manager responsible for orchestrating access and mutations against the Alignments held for a project.
  /// </summary>
  public class AlignmentManager : IAlignmentManager
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AlignmentManager>();

    private readonly IStorageProxy _writeStorageProxy;
    private readonly IStorageProxy _readStorageProxy;

    public const string ALIGNMENTS_STREAM_NAME = "Alignments";

    /// <summary>
    /// Constructs an instance using the supplied storage proxy
    /// </summary>
    public AlignmentManager(StorageMutability mutability)
    {
      _writeStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryMutableStorageProxy; 
      _readStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryStorageProxy(mutability);
    }

    /// <summary>
    /// Loads the set of Alignments for a site model. If none exist and empty list is returned.
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <returns></returns>
    private IAlignments Load(Guid siteModelUid)
    {
      try
      {
        var alignments = DIContext.Obtain<IAlignments>();

        if (alignments == null)
        {
          _log.LogError("Unable to access IAlignments factory from DI");
          return null;
        }

        _readStorageProxy.ReadStreamFromPersistentStore(siteModelUid, ALIGNMENTS_STREAM_NAME, FileSystemStreamType.Alignments, out var ms);

        if (ms != null)
        {
          using (ms)
          {
            alignments.FromStream(ms);
          }
        }

        return alignments;
      }
      catch (KeyNotFoundException)
      {
        /* This is OK, the element is not present in the cache yet */
      }
      catch (Exception e)
      {
        throw new TRexException("Exception reading Alignment cache element from Ignite", e);
      }

      return null;
    }

    /// <summary>
    /// Stores the list of Alignments for a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="alignments"></param>
    private void Store(Guid siteModelUid, IAlignments alignments)
    {
      try
      {
        using var stream = alignments.ToStream();
        _writeStorageProxy.WriteStreamToPersistentStore(siteModelUid, ALIGNMENTS_STREAM_NAME, FileSystemStreamType.Alignments, stream, this);

        _writeStorageProxy.Commit();

        // Notify the  grid listeners that attributes of this site model have changed.
        var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
        sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyAll, siteModelUid, alignmentsChanged: true);
      }
      catch (Exception e)
      {
        throw new TRexException("Exception writing updated Alignments cache element to Ignite", e);
      }
    }

    /// <summary>
    /// Add a new Alignment to a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    public IAlignment Add(Guid siteModelUid, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents)
    {
      var alignments = Load(siteModelUid);
      var newAlignment = alignments.AddAlignmentDetails(designDescriptor.DesignID, designDescriptor, extents);
      Store(siteModelUid, alignments);

      return newAlignment;
    }

    /// <summary>
    /// List the Alignments for a site model
    /// </summary>
    public IAlignments List(Guid siteModelUid)
    {
      _log.LogInformation($"Listing Alignments from site model {siteModelUid}");

      return Load(siteModelUid);
    }

    /// <summary>
    /// Remove a given Alignment from a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="alignmentUid"></param>
    /// <returns></returns>
    public bool Remove(Guid siteModelUid, Guid alignmentUid)
    {
      var alignments = Load(siteModelUid);
      var result = alignments.RemoveAlignment(alignmentUid);
      Store(siteModelUid, alignments);

      return result;
    }

    /// <summary>
    /// Remove the alignments list for a site model from the persistent store
    /// </summary>
    /// <param name="siteModelId"></param>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public bool Remove(Guid siteModelId, IStorageProxy storageProxy)
    {
      var result = storageProxy.RemoveStreamFromPersistentStore(siteModelId, FileSystemStreamType.Designs, ALIGNMENTS_STREAM_NAME);

      if (result != FileSystemErrorStatus.OK)
      {
        _log.LogInformation($"Removing alignments list from project {siteModelId} failed with error {result}");
      }

      return result == FileSystemErrorStatus.OK;
    }
  }
}
