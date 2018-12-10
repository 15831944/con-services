﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.GridFabric.Arguments
{
  /// <summary>
  /// Defines the parameters required for a production data profile request argument on the application service node
  /// </summary>
  public class ProfileRequestArgument_ApplicationService : BaseApplicationServiceRequestArgument
  {
    public GridDataType ProfileTypeRequired { get; set; }

    public WGS84Point StartPoint { get; set; } = new WGS84Point();
    public WGS84Point EndPoint { get; set; } = new WGS84Point();

    public bool PositionsAreGrid { get; set; }

    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

    public DesignDescriptor DesignDescriptor;

    public bool ReturnAllPassesAndLayers { get; set; }

    /// <summary>
    /// Constructs a default profile request argument
    /// </summary>
    public ProfileRequestArgument_ApplicationService()
    {
    }

    /// <summary>
    /// Creates a new profile request argument initialized with the supplied parameters
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="positionsAreGrid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    public ProfileRequestArgument_ApplicationService(GridDataType profileTypeRequired, WGS84Point startPoint, WGS84Point endPoint, bool positionsAreGrid, DesignDescriptor designDescriptor, bool returnAllPassesAndLayers)
    {
      ProfileTypeRequired = profileTypeRequired;
      StartPoint = startPoint;
      EndPoint = endPoint;
      PositionsAreGrid = positionsAreGrid;
      DesignDescriptor = designDescriptor;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteInt((int)ProfileTypeRequired);

      writer.WriteBoolean(StartPoint != null);
      StartPoint?.ToBinary(writer);

      writer.WriteBoolean(EndPoint != null);
      EndPoint?.ToBinary(writer);

      writer.WriteBoolean(PositionsAreGrid);

      DesignDescriptor.ToBinary(writer);

      writer.WriteBoolean(ReturnAllPassesAndLayers);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      ProfileTypeRequired = (GridDataType)reader.ReadInt();

      StartPoint = new WGS84Point();
      if (reader.ReadBoolean())
        StartPoint.FromBinary(reader);

      EndPoint = new WGS84Point();
      if (reader.ReadBoolean())
        EndPoint.FromBinary(reader);

      PositionsAreGrid = reader.ReadBoolean();

      DesignDescriptor.FromBinary(reader);

      ReturnAllPassesAndLayers = reader.ReadBoolean();
    }
  }
}
