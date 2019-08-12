﻿using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The factory used to create the client sub grid creation factory. This abstracts the factory creation aspect away
  /// from the dependency injection aspect.
  /// </summary>
  public static class ClientLeafSubGridFactoryFactory
  {
    /// <summary>
    /// Gets the sub grid client factory to use. Replace this with an implementation that 
    /// returns an appropriate element from the Dependency Injection container when this is implemented
    /// </summary>
    /// <returns></returns>
    public static IClientLeafSubGridFactory CreateClientSubGridFactory()
    {
      var clientSubGridFactory = new ClientLeafSubGridFactory();

      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.Height, () => new ClientHeightLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.HeightAndTime, () => new ClientHeightAndTimeLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CompositeHeights, () => new ClientCompositeHeightsLeafSubgrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.MachineSpeed, () => new ClientMachineSpeedLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.MachineSpeedTarget, () => new ClientMachineTargetSpeedLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CCV, () => new ClientCMVLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CCVPercentChange, () => new ClientCMVLeafSubGrid(true, false));
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CCVPercentChangeIgnoredTopNullValue, () => new ClientCMVLeafSubGrid(true, true));
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.MDP, () => new ClientMDPLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.PassCount, () => new ClientPassCountLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.Temperature, () => new ClientTemperatureLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.TemperatureDetail, () => new ClientTemperatureLeafSubGrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CellProfile, () => new ClientCellProfileLeafSubgrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CellPasses, () => new ClientCellProfileAllPassesLeafSubgrid());
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CCA, () => new ClientCCALeafSubGrid());

      // Cut fill client sub grids piggy back on the standard height leaf sub grid
      clientSubGridFactory.RegisterClientLeafSubGridType(GridDataType.CutFill, () => new ClientHeightLeafSubGrid());

      return clientSubGridFactory;
    }
  }
}
