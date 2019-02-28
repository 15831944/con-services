﻿using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.Executors;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs
{
  public class SurfaceElevationPatchComputeFunc : BaseComputeFunc, IComputeFunc<ISurfaceElevationPatchArgument, byte[]>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SurfaceElevationPatchComputeFunc>();

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private static IClientLeafSubGridFactory clientLeafSubGridFactory;

    private IClientLeafSubGridFactory ClientLeafSubGridFactory
      => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>());

    /// <summary>
    /// Invokes the surface elevation patch computation function on the server nodes the request has been sent to
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public byte[] Invoke(ISurfaceElevationPatchArgument arg)
    {
      try
      {
        Log.LogDebug($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

        CalculateSurfaceElevationPatch Executor = new CalculateSurfaceElevationPatch(arg);

        IClientLeafSubGrid result = Executor.Execute();

        if (result == null)
          return null;

        try
        {
          return result.ToBytes();
        }
        finally
        {
          ClientLeafSubGridFactory.ReturnClientSubGrid(ref result);
        }
      }
      catch (Exception E)
      {
        Log.LogError(E, "Exception:");
      }

      return null;
    }
  }
}
