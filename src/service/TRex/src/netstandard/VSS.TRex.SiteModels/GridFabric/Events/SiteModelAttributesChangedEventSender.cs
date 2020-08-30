﻿using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  /// <summary>
  /// Responsible for sending a notification that the attributes of a site model have changed
  /// By definition, all server and client nodes should react to this message
  /// </summary>
  public class SiteModelAttributesChangedEventSender : ISiteModelAttributesChangedEventSender
  {
    //private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelAttributesChangedEventSender>();

    private const string MessageTopicName = "SiteModelAttributesChangedEvents";

    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    public void ModelAttributesChanged(SiteModelNotificationEventGridMutability targetGrids,
      Guid siteModelID,
      bool existenceMapChanged = false,
      ISubGridTreeBitMask existenceMapChangeMask = null,
      bool designsChanged = false,
      bool surveyedSurfacesChanged = false,
      bool csibChanged = false,
      bool machinesChanged = false,
      bool machineTargetValuesChanged = false,
      bool machineDesignsModified = false,
      bool proofingRunsModified = false,
      bool alignmentsChanged = false,
      bool siteModelMarkedForDeletion = false)
    {
      var gridFactory = DIContext.Obtain<ITRexGridFactory>();
      var evt = new SiteModelAttributesChangedEvent
      {
        SiteModelID = siteModelID,
        ExistenceMapModified = existenceMapChanged,
        ExistenceMapChangeMask = existenceMapChangeMask?.ToBytes(),
        CsibModified = csibChanged,
        DesignsModified = designsChanged,
        SurveyedSurfacesModified = surveyedSurfacesChanged,
        MachinesModified = machinesChanged,
        MachineTargetValuesModified = machineTargetValuesChanged,
        MachineDesignsModified = machineDesignsModified,
        ProofingRunsModified = proofingRunsModified,
        AlignmentsModified = alignmentsChanged,
        SiteModelMarkedForDeletion = siteModelMarkedForDeletion,
        ChangeEventUid = Guid.NewGuid(),
        TimeSentUtc = DateTime.UtcNow
      };

      if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyImmutable) != 0)
        gridFactory.Grid(StorageMutability.Immutable).GetMessaging().SendOrdered(evt, MessageTopicName);

      if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyMutable) != 0)
        gridFactory.Grid(StorageMutability.Mutable).GetMessaging().SendOrdered(evt, MessageTopicName);
    }
  }
}
