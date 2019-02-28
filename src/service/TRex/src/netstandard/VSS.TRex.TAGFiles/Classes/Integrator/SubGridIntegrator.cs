﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
    /// <summary>
    /// Responsible for orchestrating integration of mini site models processed from one or
    /// more TAG files into another site model, either a temporary/transient artifact of the ingest
    /// pipeline, or the persistent data store.
    /// </summary>
    public class SubGridIntegrator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridIntegrator>();
     
        /// <summary>
        /// The sub grid tree from which information is being integrated
        /// </summary>
        private readonly IServerSubGridTree Source;

        /// <summary>
        /// Site model representing the target sub grid tree
        /// </summary>
        private readonly ISiteModel SiteModel;

        /// <summary>
        /// The sub grid tree the receives the sub grid information from the source sub grid tree
        /// </summary>
        private readonly IServerSubGridTree Target;

        private IServerLeafSubGrid SourceSubGrid;
        private IServerLeafSubGrid TargetSubGrid;

        private Action<uint, uint> SubGridChangeNotifier;

        private readonly IStorageProxy StorageProxy;

        public readonly List<ISubGridSpatialAffinityKey> InvalidatedSpatialStreams = new List<ISubGridSpatialAffinityKey>(100);

        /// <summary>
        /// Constructor the initializes state ready for integration
        /// </summary>
        /// <param name="source">The sub grid tree from which information is being integrated</param>
        /// <param name="siteModel">The site model representing the target sub grid tree</param>
        /// <param name="target">The sub grid tree into which the data from the source sub grid tree is integrated</param>
        /// <param name="storageProxy">The storage proxy providing storage semantics for persisting integration results</param>
        public SubGridIntegrator(IServerSubGridTree source, ISiteModel siteModel, IServerSubGridTree target, IStorageProxy storageProxy)
        {
            Source = source;
            SiteModel = siteModel;
            Target = target;
            StorageProxy = storageProxy;
        }

        private void IntegrateIntoIntermediaryGrid(ISubGridSegmentIterator SegmentIterator)
        {
            TargetSubGrid = Target.ConstructPathToCell(SourceSubGrid.OriginX,
                                                       SourceSubGrid.OriginY,
                                                       SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
            TargetSubGrid.AllocateLeafFullPassStacks();

            // If the node is brand new (ie: it does not have any cell passes committed to it yet)
            // then create and select the default segment
            if (TargetSubGrid.Directory.SegmentDirectory.Count == 0)
            {
                TargetSubGrid.Cells.SelectSegment(DateTime.MinValue);
                TargetSubGrid.Cells.PassesData[0].AllocateFullPassStacks();
            }

            if (TargetSubGrid.Cells.PassesData[0].PassesData == null)
            {
                Log.LogCritical("No segment passes data in new segment");
                return;
            }

            // As the integration is into the intermediary grid, these segments do not
            // need to be involved with the cache, so instruct the iterator to not 'touch' them
            SegmentIterator.MarkReturnedSegmentsAsTouched = false;

            TargetSubGrid.Integrate(SourceSubGrid, SegmentIterator, true);
        }

        private bool IntegrateIntoLiveDatabase(SubGridSegmentIterator SegmentIterator)
        {
            // Note the fact that this sub grid will be changed and become dirty as a result
            // of the cell pass integration
            TargetSubGrid.SetDirty();

            // As the integration is into the live database these segments do
            // need to be involved with the cache, so instruct the iterator to
            // 'touch' them
            SegmentIterator.MarkReturnedSegmentsAsTouched = true;

            TargetSubGrid.Integrate(SourceSubGrid, SegmentIterator, false);

            SubGridChangeNotifier?.Invoke(TargetSubGrid.OriginX, TargetSubGrid.OriginY);

            // Save the integrated state of the sub grid segments to allow Ignite to store & socialize the update
            // within the cluster. 

            // Failure to save a piece of data aborts the entire integration
            bool result = false;
            if (Target.SaveLeafSubGrid(TargetSubGrid, SegmentIterator.StorageProxy, InvalidatedSpatialStreams))
            {
              // Successfully saving the sub grid directory information is the point at which this sub grid may be recognized to exist
              // in the site model. Note this by including it within the SiteModel existence map
        
              SiteModel.ExistenceMap.SetCell(TargetSubGrid.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                             TargetSubGrid.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                             true);
              result = true;
            }
        
            // Finally, mark the source sub grid as not being dirty. We need to do this to allow
            // the sub grid to permit its destruction as all changes have been merged into the target.
            if (result)
              SourceSubGrid.AllChangesMigrated();
        
            return result;
        }

        private void IntegrateIntoLiveGrid(SubGridSegmentIterator SegmentIterator)
        {
            TargetSubGrid = LocateOrCreateSubGrid(Target, SourceSubGrid.OriginX, SourceSubGrid.OriginY);
            if (TargetSubGrid == null)
            {
                Log.LogError("Failed to locate or create sub grid in IntegrateIntoLiveGrid");
                return;
            }

            if (!IntegrateIntoLiveDatabase(SegmentIterator))
            {
                Log.LogError("Integration into live database failed");
            }
        }

        public bool IntegrateSubGridTree(SubGridTreeIntegrationMode integrationMode,
                                         Action<uint, uint> subGridChangeNotifier)
        {
            // Iterate over the sub grids in source and merge the cell passes from source
            // into the sub grids in this sub grid tree;

            SubGridTreeIterator Iterator = new SubGridTreeIterator(StorageProxy, false)
            {
                Grid = Source
            };

            SubGridSegmentIterator SegmentIterator = new SubGridSegmentIterator(null, StorageProxy)
            {
                IterationDirection = IterationDirection.Forwards
            };

            bool IntegratingIntoIntermediaryGrid = integrationMode == SubGridTreeIntegrationMode.UsingInMemoryTarget;
            SubGridChangeNotifier = subGridChangeNotifier;

            while (Iterator.MoveToNextSubGrid())
            {
                SourceSubGrid = Iterator.CurrentSubGrid as IServerLeafSubGrid;

                /*
                 // TODO: Terminated check for integration processing
                if (Terminated)
                {
                    // Service has been shutdown. Abort integration of changes and flag the
                    // operation as failed. The TAG file will be reprocessed when the service restarts
                    return false;
                }
                */

                // Locate a matching sub grid in this tree. If there is none, then create it
                // and assign the sub grid from the iterator to it. If there is one, process
                // the cell pass stacks merging the two together
                if (IntegratingIntoIntermediaryGrid)
                    IntegrateIntoIntermediaryGrid(SegmentIterator);
                else
                    IntegrateIntoLiveGrid(SegmentIterator);
            }

            return true;
        }

        /// <summary>
        /// Locates a sub grid in within this site model. If the sub grid cannot be found it will be created.
        /// If requested from an immutable grid context, the result of this call should be considered as an immutable
        /// copy of the requested data that is valid for the duration the request holds a reference to it. Updates
        /// to sub grids in this data model from ingest processing and other operations performed in mutable contexts
        /// can occur while this request is in process, but will not affected the immutable copy initially requested.
        /// If requested from a mutable grid context the calling context is responsible for ensuring serialized write access
        /// to the data elements being requested. 
        /// </summary>
        /// <param name="Grid"></param>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public IServerLeafSubGrid LocateOrCreateSubGrid(IServerSubGridTree Grid, uint CellX, uint CellY)
        {
            IServerLeafSubGrid Result = SubGridUtilities.LocateSubGridContaining(
                                    StorageProxy,
                                    Grid,
                                    // DataStoreInstance.GridDataCache,
                                    CellX, CellY,
                                    Grid.NumLevels,
                                    false, true) as IServerLeafSubGrid;

            // Ensure the cells and segment directory are initialized if this is a new sub grid
            if (Result != null)
            {
                // By definition, any new sub grid we create here is dirty, even if we
                // ultimately do not add any cell passes to it. This is necessary to
                // encourage even otherwise empty sub grids to be persisted to disk if
                // they have been created, but never populated with cell passes.
                // The sub grid persistent layer may implement a rule that no empty
                // sub grids are saved to disk if this becomes an issue...
                Result.SetDirty();

                Result.AllocateLeafFullPassStacks();
                if (Result.Directory.SegmentDirectory.Count == 0)
                {
                    Result.Cells.SelectSegment(DateTime.MinValue);
                }

                if (Result.Cells == null)
                {
                    Log.LogCritical($"LocateSubGridContaining returned a sub grid {Result.Moniker()} with no allocated cells");
                }
                else if (Result.Directory.SegmentDirectory.Count == 0)
                {
                    Log.LogCritical($"LocateSubGridContaining returned a sub grid {Result.Moniker()} with no segments in its directory");
                }
            }
            else
            {
                Log.LogCritical($"LocateSubGridContaining failed to return a sub grid (CellX/Y={CellX}/{CellY})");
            }

            return Result;
        }
    }
}
