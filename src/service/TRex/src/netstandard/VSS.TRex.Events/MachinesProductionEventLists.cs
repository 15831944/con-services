﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Events
{
  /// <summary>
  /// Stores the list of sets of machine events. Each machine has a set of event lists. MachinesTargetValues 
  /// contains a list of these sets of event lists for multiple machines
  /// </summary>
  public class MachinesProductionEventLists : IMachinesProductionEventLists
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<MachinesProductionEventLists>();

    /// <summary>
    /// The site model instance that owns this set of machines target values
    /// </summary>
    private readonly ISiteModel Owner;

    /// <summary>
    /// Maps machine IDs (currently as 16 bit integers) to the instance containing all the event lists for all the machines
    /// that have contributed to the owner SiteModel
    /// </summary>
    private IProductionEventLists[] internalSiteModelMachineIndexMap;

    /// <summary>
    /// Constructor for the machines events within the site model supplier as owner
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="machineCount"></param>
    public MachinesProductionEventLists(ISiteModel owner, int machineCount)
    {
      Owner = owner;

      Log.LogInformation($"Creating machine ID map containing {machineCount} entries");
      internalSiteModelMachineIndexMap = new IProductionEventLists[machineCount]; 
    }

    /// <summary>
    /// </summary>
    /// <param name="internalSiteModelMachineIndex"></param>
    /// <returns></returns>
    private IProductionEventLists GetMachineEventLists(short internalSiteModelMachineIndex)
    {
      if (internalSiteModelMachineIndex < 0 || internalSiteModelMachineIndex >= internalSiteModelMachineIndexMap.Length)
        return null;

      // Return (creating if necessary) the machine event lists for this machine and allow required events to lazy load.
      if (internalSiteModelMachineIndexMap[internalSiteModelMachineIndex] == null)
      {
        lock (this)
        {
          if (internalSiteModelMachineIndexMap[internalSiteModelMachineIndex] == null) // This thread 'won'
            internalSiteModelMachineIndexMap[internalSiteModelMachineIndex] = new ProductionEventLists(Owner, internalSiteModelMachineIndex);
        }
      }

      return internalSiteModelMachineIndexMap[internalSiteModelMachineIndex];
    }

    /// <summary>
    /// Default (short) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
    /// events lists for that machine.
    /// </summary>
    /// <param name="internalSiteModelMachineIndex"></param>
    /// <returns></returns>
    public IProductionEventLists this[short internalSiteModelMachineIndex] => GetMachineEventLists(internalSiteModelMachineIndex);

    /// <summary>
    /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
    /// </summary>
    /// <param name="events"></param>
    public void Add(IProductionEventLists events)
    {
      if (events.InternalSiteModelMachineIndex >= internalSiteModelMachineIndexMap.Length)
        Array.Resize(ref internalSiteModelMachineIndexMap, events.InternalSiteModelMachineIndex + 1);

      internalSiteModelMachineIndexMap[events.InternalSiteModelMachineIndex] = events;
    }
  }
}
