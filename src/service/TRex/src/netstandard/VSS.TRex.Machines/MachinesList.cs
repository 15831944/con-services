﻿using System;
using System.Collections.Generic;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Common.Extensions;

namespace VSS.TRex.Machines
{
  /// <summary>
  /// Implements a container for all the machines that have had activity within a Site Model
  /// </summary>
  public class MachinesList : List<IMachine>, IMachinesList, IBinaryReaderWriter
  {
    private const byte VERSION_NUMBER = 1;
    public const string MACHINES_LIST_STREAM_NAME = "Machines";

    /// <summary>
    /// Maps machine IDs (currently as 64 bit integers) to the instance containing all the event lists for all the machines
    /// that have contributed to the owner SiteModel
    /// </summary>
    private readonly Dictionary<Guid, IMachine> MachineIDMap = new Dictionary<Guid, IMachine>();

    /// <summary>
    /// The identifier of the site model owning this list of machines
    /// </summary>
    public Guid DataModelID { get; set; }

    public MachinesList()
    {
    }

    /// <summary>
    /// Determine the next unique JohnDoe machine ID to use for a new John Doe machine
    /// </summary>
    /// <returns></returns>
    private Guid UniqueJohnDoeID() => Guid.NewGuid();

    /// <summary>
    /// Creates a new machine and adds it to the internal list with a new internal machine index
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machineHardwareID"></param>
    /// <param name="machineType"></param>
    /// <param name="deviceType"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <param name="machineID"></param>
    /// <returns></returns>
    public IMachine CreateNew(string name, 
      string machineHardwareID,
      MachineType machineType,
      DeviceTypeEnum deviceType,
      bool isJohnDoeMachine,
      Guid machineID)
    {
      var ExistingMachine = isJohnDoeMachine ? Locate(name, true) : Locate(machineID, false);

      if (ExistingMachine != null)
        return ExistingMachine;

      // Create the new machine
      if (isJohnDoeMachine)
        machineID = UniqueJohnDoeID();

      // Determine the internal ID for the new machine.
      // Note: This assumes machines are never removed from a project

      var internalMachineID = (short) Count;

      var Result = new Machine(name, machineHardwareID, machineType, deviceType, machineID, internalMachineID, isJohnDoeMachine);

      // Add it to the list
      Add(Result);

      return Result;
    }

    /// <summary>
    /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
    /// </summary>
    /// <param name="machine"></param>
    public new void Add(IMachine machine)
    {
      if (machine == null)
        throw new ArgumentException($"Machine reference cannot be null in {nameof(Add)}", nameof(machine));

      base.Add(machine);

      MachineIDMap.Add(machine.ID, machine);
    }

    /// <summary>
    /// Finds the machine in the list whose name matches the given name
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    public IMachine Locate(string name, bool isJohnDoeMachine) => Find(x => x.IsJohnDoeMachine == isJohnDoeMachine && name.Equals(x.Name));

    /// <summary>
    /// Locate finds the machine in the list whose name matches the given ID
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    public IMachine Locate(Guid id, bool isJohnDoeMachine) => Find(x => x.IsJohnDoeMachine == isJohnDoeMachine && id == x.ID);

    /// <summary>
    /// Finds a machine given a machine ID, the machine name and whether the machine is John Doe machine
    /// If a machine matching the given ID is located it is returned, else if the machine is expected to be a John Doe a
    /// machine is attempted to be located using a specific John Doe machine name approach
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    public IMachine Locate(Guid id, string name, bool isJohnDoeMachine) => Locate(id, isJohnDoeMachine) ?? (isJohnDoeMachine ? Locate(name, true) : null);

    /// <summary>
    /// Locate finds a machine given the ID of a machine in the list.
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IMachine Locate(Guid id) => MachineIDMap.TryGetValue(id, out var result) ? result : null;

    // LocateByMachineHardwareID locates the (first) machine in the machines
    // list that has a matching machine hardware ID to the <AID> parameter.
    public IMachine LocateByMachineHardwareID(string hardwareID) => Find(x => x.MachineHardwareID.Equals(hardwareID));

    /// <summary>
    /// Serialize the list of machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(Count);
      for (int i = 0; i < Count; i++)
        this[i].Write(writer);
    }

    /// <summary>
    /// Deserializes the list of machines using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      int count = reader.ReadInt32();
      Capacity = count;

      for (int i = 0; i < count; i++)
      {
        Machine Machine = new Machine();
        Machine.Read(reader);
        Add(Machine);
      }
    }

    /// <summary>
    /// Saves the content of the machines list into the persistent store
    /// Note: It uses a storage proxy delegate to support the TAG file ingest pipeline that creates transactional storage
    /// proxies to manage graceful rollback of changes if needed
    /// </summary>
    public void SaveToPersistentStore(IStorageProxy storageProxy)
    {
      using var stream = this.ToStream();
      storageProxy.WriteStreamToPersistentStore(DataModelID, MACHINES_LIST_STREAM_NAME, FileSystemStreamType.Machines, stream, this);
    }

    /// <summary>
    /// Removes the content of the machines list from the persistent store
    /// </summary>
    public void RemoveFromPersistentStore(IStorageProxy storageProxy)
    {
        RemoveFromPersistentStore(DataModelID, storageProxy);
    }

    /// <summary>
    /// Removes the content of the machines list from the persistent store
    /// </summary>
    public static void RemoveFromPersistentStore(Guid dataModelId, IStorageProxy storageProxy)
    {
      storageProxy.RemoveStreamFromPersistentStore(dataModelId, FileSystemStreamType.Machines, MACHINES_LIST_STREAM_NAME);
    }

    /// <summary>
    /// Removes all state in the machine obtained from TAG file processing apart from identity information 
    /// </summary>
    public void ClearTAGFileStateData(IStorageProxy storageProxy)
    {
      ForEach(x => x.ClearTAGFileStateData());

      SaveToPersistentStore(storageProxy);
    }

    /// <summary>
    /// Loads the content of the machines list from the persistent store. If there is no item in the persistent store containing
    /// machines for this site model them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore(IStorageProxy storageProxy)
    {
      storageProxy.ReadStreamFromPersistentStore(DataModelID, MACHINES_LIST_STREAM_NAME, FileSystemStreamType.Machines, out MemoryStream MS);
      if (MS == null)
        return;

      using (MS)
      {
        this.FromStream(MS);
      }
    }
  }
}
