﻿using System;
using System.Threading.Tasks;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Analytics.Foundation.Aggregators
{
  /// <summary>
  /// Base class used by all analytics aggregators supporting functions such as pass count summary/details, cut/fill summary, speed summary/details etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public abstract class AggregatorBase : ISubGridRequestsAggregator
  {
    /// <summary>
    /// The project the aggregation is operating on
    /// </summary>
    public Guid SiteModelID { get; set; }

    /// <summary>
    /// The cell size of the site model the aggregation is being performed over
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// Provides any state initialization logic for the aggregator
    /// </summary>
    /// <param name="state"></param>
    public abstract void Initialise(AggregatorBase state);

    /// <summary>
    /// Performs base aggregator handling of subgrid results
    /// </summary>
    /// <param name="subGrids"></param>
    public abstract void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids);

    /// <summary>
    /// Performs base aggregator finalisation activities
    /// </summary>
    public abstract void Finalise();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public AggregatorBase()
    {
    }
  }
}
