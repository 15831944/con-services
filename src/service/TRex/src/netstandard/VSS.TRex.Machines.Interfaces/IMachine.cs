﻿using System;
using System.IO;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Machines.Interfaces
{
  public interface IMachine
  {
    Guid ID { get; set; }
    short InternalSiteModelMachineIndex { get; set; }
    string Name { get; set; }
    MachineType MachineType { get; set; }
    DeviceType DeviceType { get; set; }
    string MachineHardwareID { get; set; }
    bool IsJohnDoeMachine { get; set; }
    double LastKnownX { get; set; }
    double LastKnownY { get; set; }
    DateTime LastKnownPositionTimeStamp { get; set; }
    string LastKnownDesignName { get; set; }
    ushort LastKnownLayerId { get; set; }

    /// <summary>
    /// Indicates if the machine has ever reported any compaction related data, such as CCV, MDP or CCA measurements
    /// </summary>
    bool CompactionDataReported { get; set; }

    CompactionSensorType CompactionSensorType { get; set; }

    /// <summary>
    /// Determines if the type of this machine is one of the machine types that supports compaction operations
    /// </summary>
    /// <returns></returns>
    bool MachineIsCompactorType();

    void Assign(IMachine source);

    /// <summary>
    /// Serializes machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Deserializes the machine using the given reader
    /// </summary>
    /// <param name="reader"></param>
    void Read(BinaryReader reader);
  }
}
