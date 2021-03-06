﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
    /// <summary>
    /// Provides business logic driving integration of event lists derived from TAG file processing
    /// into event lists in a site model into the event lists of another site model
    /// </summary>
    public class EventIntegrator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The array of enumeration values represented by ProductionEventType
        /// </summary>
        private static readonly int[] ProductionEventTypeValues = Enum.GetValues(typeof(ProductionEventType)).Cast<int>().ToArray();

        private IProductionEventLists _sourceLists;
        private IProductionEventLists _targetLists;
        private bool _integratingIntoPersistentDataModel;
        private ISiteModel _sourceSiteModel;
        private ISiteModel _targetSiteModel;
     
        public EventIntegrator()
        {
        }

      public EventIntegrator(IProductionEventLists sourceLists,
        IProductionEventLists targetLists,
        bool integratingIntoPersistentDataModel) : this()
      {
        _sourceLists = sourceLists;
        _targetLists = targetLists;
        _integratingIntoPersistentDataModel = integratingIntoPersistentDataModel;
      }

      private void IntegrateMachineDesignEventNames()
      {
        // ensure that 1 copy of the machineDesignName exists in the targetSiteModels List,
        // and we reflect THAT Id in the source list

        for (var I = 0; I < _sourceLists.MachineDesignNameIDStateEvents.Count(); I++)
        {
          _sourceLists.MachineDesignNameIDStateEvents.GetStateAtIndex(I, out var dateTime, out var machineDesignId);
          if (machineDesignId > -1)
          {
            var sourceMachineDesign = _sourceSiteModel.SiteModelMachineDesigns.Locate(machineDesignId);
            if (sourceMachineDesign != null)
            {
              var targetMachineDesign = _targetSiteModel.SiteModelMachineDesigns.CreateNew(sourceMachineDesign.Name);
              _sourceLists.MachineDesignNameIDStateEvents.SetStateAtIndex(I, targetMachineDesign.Id);
            }
          }
          else
          {
            Log.LogError($"Failed to locate machine design name at dateTime: {dateTime} in the design change events list");
          }
        }
      }

      // IntegrateList takes a list of machine events and merges them into the machine event list.
        // Note: This method assumes that the methods being merged into the new list
        // are machine events only, and do not include custom events.
        private void IntegrateList(IProductionEvents source, IProductionEvents target)
        {
            if (source.Count() == 0)
                return;

            if (source.Count() > 1)
                source.Sort();

            target.CopyEventsFrom(source);
            target.Collate(_targetLists);
        }

        private void PerformListIntegration(IProductionEvents source, IProductionEvents target)
        {
            IntegrateList(source, target);
        }

      public void IntegrateMachineEvents(IProductionEventLists sourceLists,
        IProductionEventLists targetLists,
        bool integratingIntoPersistentDataModel,
        ISiteModel sourceSiteModel,
        ISiteModel targetSiteModel)
      {
        _sourceLists = sourceLists;
        _targetLists = targetLists;
        _integratingIntoPersistentDataModel = integratingIntoPersistentDataModel;
        _sourceSiteModel = sourceSiteModel;
        _targetSiteModel = targetSiteModel;
        IntegrateMachineEvents();
      }

         /// <summary>
        /// Integrate together all the events lists for a machine between the source and target lists of machine events
        /// </summary>
        private void IntegrateMachineEvents()
        {
            if (_sourceLists == null)
              return;

            IntegrateMachineDesignEventNames();

            IProductionEvents sourceStartEndRecordedDataList = _sourceLists.StartEndRecordedDataEvents;

            // Always integrate the machine recorded data start/stop events first, as collation
            // of the other events depends on collation of these events
            PerformListIntegration(sourceStartEndRecordedDataList, _targetLists.StartEndRecordedDataEvents); 

            var sourceEventLists = _sourceLists.GetEventLists();
            var targetEventLists = _targetLists.GetEventLists();

            // Integrate all remaining event lists and collate them wrt the machine start/stop recording events
            foreach (var evt in ProductionEventTypeValues)
            {
                var sourceList = sourceEventLists[evt];

                if (sourceList != null && sourceList != sourceStartEndRecordedDataList && sourceList.Count() > 0)
                {
                    // The source event list is always an in-memory list. The target event list
                    // will be an in-memory list unless IntegratingIntoPersistentDataModel is true,
                    // in which case the source events are being integrated into the data model events
                    // list present in the persistent store.

                    var targetList = targetEventLists[evt] ?? _targetLists.GetEventList(sourceList.EventListType);

                    if (_integratingIntoPersistentDataModel && targetList == null)
                        Log.LogError($"Event list {evt} not available in IntegrateMachineEvents");

                    if (targetList != null)
                        PerformListIntegration(sourceList, targetList);
                }
            }
        }
    }
}
