﻿using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;

namespace VSS.TRex.Profiling.Executors
{
  /// <summary>
  /// Executes business logic that calculates the profile between two points in space
  /// </summary>
  public class ComputeProfileExecutor_ApplicatonService
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ComputeProfileExecutor_ApplicatonService>();

    public ComputeProfileExecutor_ApplicatonService()
    {
    }

    /// <summary>
    /// Executes the profiler
    /// </summary>
    public ProfileRequestResponse Execute(ProfileRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("Start execution");
      try
      {
        ProfileRequestArgument_ClusterCompute arg2 = new ProfileRequestArgument_ClusterCompute()
        {
          ProfileTypeRequired = arg.ProfileTypeRequired,
          ProjectID = arg.ProjectID,
          Filters = arg.Filters,
          CutFillDesignID = arg.CutFillDesignID,
          ReturnAllPassesAndLayers = arg.ReturnAllPassesAndLayers,
          DesignDescriptor = arg.DesignDescriptor,
          TRexNodeID = arg.TRexNodeID
        };

        // Perform coordinate conversion on the argument before broadcasting it:
        if (arg.PositionsAreGrid)
          arg2.NEECoords = CoordinateSystems.Convert.NullWGSLLToXY(new[] {arg.StartPoint, arg.EndPoint});
        else
          arg2.NEECoords =
            CoordinateSystems.Convert.WGS84ToCalibration(arg.ProjectID, new[] {arg.StartPoint, arg.EndPoint});

        ProfileRequest_ClusterCompute request = new ProfileRequest_ClusterCompute();
        //ProfileRequestComputeFunc_ClusterCompute func = new ProfileRequestComputeFunc_ClusterCompute();

        ProfileRequestResponse ProfileResponse = request.Execute(arg2);

        //... and then sort them to get the final result
        ProfileResponse?.ProfileCells?.OrderBy(x => x.Station);

        // Return the care package to the caller
        return ProfileResponse;
      }
      finally
      {
        Log.LogInformation("End execution");
      }
    }
  }
}
