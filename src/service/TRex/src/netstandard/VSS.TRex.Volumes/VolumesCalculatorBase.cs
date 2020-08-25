﻿using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.Executors.Tasks;
using VSS.TRex.Volumes.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using Consts = VSS.TRex.Common.Consts;
using VSS.TRex.SubGrids.Interfaces;

namespace VSS.TRex.Volumes
{
    /// <summary>
    /// VolumesCalculatorBase provides a base class that may be extended/decorated
    /// to implement specific volume calculation engines that access production data and use
    /// it to derive volumes information.
    /// </summary>
    public abstract class VolumesCalculatorBase
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<VolumesCalculatorBase>();

        /// <summary>
        /// DI'ed context for access to ExistenceMaps functionality
        /// </summary>
        private IExistenceMaps _existenceMaps = null;
        private IExistenceMaps GetExistenceMaps() => _existenceMaps ??= DIContext.Obtain<IExistenceMaps>();

        /// <summary>
        /// The Aggregator to use for calculating volumes statistics
        /// </summary>
        public ISubGridRequestsAggregator Aggregator { get; set; }

        /// <summary>
        /// The site model from which the volume is being calculated
        /// </summary>
        public ISiteModel SiteModel { get; set; }

        /// <summary>
        /// The volume computation method to use when calculating volume information
        /// </summary>
        public VolumeComputationType VolumeType = VolumeComputationType.None;

        /// <summary>
        /// Parameters for lift analysis
        /// </summary>
        public ILiftParameters LiftParams { get; set; }

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public VolumesCalculatorBase()
        {
        }

        protected BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

        /// <summary>
        /// BaseFilter and TopFilter reference two sets of filter settings
        /// between which we may calculate volumes. At the current time, it is
        /// meaningful for a filter to have a spatial extent, and to denote an
        /// 'as-at' time only.
        /// </summary>
        public ICombinedFilter BaseFilter { get; set; }
        public ICombinedFilter TopFilter { get; set; }

        /// FIntermediary filter is used to assist calculation of the volumetric work
        /// done between two points in time. Conceptually, the from surface is defined
        /// as a combination of the surface data using the latest available information
        /// for an AsAt time filter, ['from', representing the start of the time period] and combining
        /// it with the surface data using the earliest available information for a
        /// time range filter defined as starting at the AsAt data of the from filter and
        /// [representing work started after the AsAt data of the 'from' filter]
        /// terminating at the end date of the 'To'. This combined surface is then
        /// compared to a surface constructed from the latest available information
        /// for the supplied To Filter.
        ///
        /// The intermediary filter is derived from the 'To' filter and is altered to be
        /// an 'earliest' filter with all other attributes remaining unchanged and is
        /// used to calculate the additional elevation information to be combined with
        /// the 'AsAt'/latest surface information from the 'From' filter.
        ///
        /// The conditions for using the intermediary filter are then:
        /// 1. A summary volume requested between two filters
        /// 2. The 'from' filter is defined as an 'As-At' filter, with latest data selected
        /// 3. The 'to' filter is defined either as an 'As-At' or a time range filter,
        ///    with latest data selected
        ///
        /// Note: No 'look forward' behaviour should be undertaken.
        ICombinedFilter IntermediaryFilter { get; set; }

        /// <summary>
        /// RefOriginal references a subset that may be used in the volumes calculations
        /// process. If set, it represents the original ground of the site
        /// </summary>
        public IDesign RefOriginal { get; set; }

        /// <summary>
        /// RefDesign references a subset that may be used in the volumes calculations
        /// process. If set, it takes the place of the 'top' filter.
        /// </summary>
        public IDesign RefDesign { get; set; }

        /// <summary>
        /// ActiveDesign is the design surface being used as the comparison surface in the
        /// surface to production data volume calculations. It is assigned from the FRefOriginal
        /// and FRefDesign surfaces depending on the volumes reporting type and configuration.
        /// It also contains the offset for a reference surface.
        /// </summary>
        public IDesignWrapper ActiveDesign { get; set; }

        /// <summary>
        /// FromSelectionType and ToSelectionType describe how we mix the two filters
        /// (BaseFilter and TopFilter) and two reference designs (RefOriginal and
        /// RefDesign) together to derive the upper and lower 'surfaces' between which
        /// we compute summary or details production volumes
        /// </summary>
        public ProdReportSelectionType FromSelectionType { get; set; } = ProdReportSelectionType.None;

        /// <summary>
        /// FromSelectionType and ToSelectionType describe how we mix the two filters
        /// (BaseFilter and TopFilter) and two reference designs (RefOriginal and
        /// RefDesign) together to derive the upper and lower 'surfaces' between which
        /// we compute summary or details production volumes
        /// </summary>
        public ProdReportSelectionType ToSelectionType { get; set; }  = ProdReportSelectionType.None;

        /*
        // FAborted keeps track of whether we've been bushwhacked or not!
        protected FAborted : Boolean;

        // FNoChangeMap maps the area of cells that we have considered and found to have
        // had no height change between to two surfaces considered
        protected FNoChangeMap : TSubGridTreeBitMask;                      
        */

        /// <summary>
        /// UseEarliestData governs whether we want the earliest or latest data from filtered
        /// ranges of cell passes in the base filtered surface.
        /// </summary>
        public bool UseEarliestData { get; set; }

        // FLiftBuildSettings : TICLiftBuildSettings;

        private ISubGridTreeBitMask ProdDataExistenceMap; 
        private ISubGridTreeBitMask OverallExistenceMap;

        private ISubGridTreeBitMask DesignSubgridOverlayMap;

        public bool AbortedDueToTimeout { get; set; } = false;

        //        FEpochCount           : Integer;

        ISurveyedSurfaces FilteredBaseSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
        ISurveyedSurfaces FilteredTopSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

        public Guid RequestDescriptor { get; set; } = Guid.Empty;

        public abstract bool ComputeVolumeInformation();

        private void ConfigurePipeline(SubGridPipelineAggregative<SubGridsRequestArgument, SimpleVolumesResponse> PipeLine)
        {
          //PipeLine.TimeToLiveSeconds := VLPDSvcLocations.VLPDPSNode_VolumePipelineTTLSeconds;
          PipeLine.RequestDescriptor = RequestDescriptor;
          //PipeLine.ExternalDescriptor := FExternalDescriptor;

          PipeLine.DataModelID = SiteModel.ID;

          Log.LogDebug($"Volume calculation extents for DM={SiteModel.ID}, Request={RequestDescriptor}: {Extents}");

          PipeLine.OverallExistenceMap = OverallExistenceMap;
          PipeLine.ProdDataExistenceMap = ProdDataExistenceMap;
          PipeLine.DesignSubGridOverlayMap = DesignSubgridOverlayMap;

          // Initialise a request analyzer to provide to the pipeline
          PipeLine.RequestAnalyser = DIContext.Obtain<IRequestAnalyser>();
          PipeLine.RequestAnalyser.Pipeline = PipeLine;
          PipeLine.RequestAnalyser.WorldExtents.Assign(Extents);

          PipeLine.LiftParams = LiftParams;

          // Construct and assign the filter set into the pipeline
          IFilterSet FilterSet = FilterUtilities.ConstructFilters(new FilterSet(BaseFilter, TopFilter), VolumeType);
          if (FilterSet.Filters.Length == 3)
          {
            PipeLine.SubGridsRequestComputeStyle = SubGridsRequestComputeStyle.SimpleVolumeThreeWayCoalescing;
          }
          PipeLine.FilterSet = FilterSet; 
          PipeLine.GridDataType = GridDataType.Height;

          if (FilteredTopSurveyedSurfaces.Count > 0 || FilteredBaseSurveyedSurfaces.Count > 0)
            PipeLine.IncludeSurveyedSurfaceInformation = true;
        }

        public RequestErrorStatus ExecutePipeline()
        {
            SubGridPipelineAggregative<SubGridsRequestArgument, SimpleVolumesResponse> PipeLine;

            var Result = RequestErrorStatus.Unknown;

            var PipelineAborted = false;
            // bool ShouldAbortDueToCompletedEventSet  = false;

            try
            {
                ProdDataExistenceMap = SiteModel.ExistenceMap;

                try
                {
                    if (ActiveDesign != null && (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.BetweenDesignAndFilter))
                    {
                        if (ActiveDesign == null || ActiveDesign.Design.DesignDescriptor.IsNull)
                        {
                            Log.LogError($"No design provided to prod data/design volumes calc for datamodel {SiteModel.ID}");
                            return RequestErrorStatus.NoDesignProvided;
                        }

                        DesignSubgridOverlayMap = GetExistenceMaps().GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, ActiveDesign.Design.ID);

                        if (DesignSubgridOverlayMap == null)
                            return RequestErrorStatus.NoDesignProvided;
                    }

                    OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask();

                    // Work out the surveyed surfaces and coverage areas that need to be taken into account

                    var SurveyedSurfaces = SiteModel.SurveyedSurfaces;

                    if (SurveyedSurfaces != null)
                    {
                        // See if we need to handle surveyed surface data for 'base'
                        // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                        if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenFilterAndDesign)
                        {
                          if (!SurveyedSurfaces.ProcessSurveyedSurfacesForFilter(SiteModel.ID, BaseFilter, FilteredTopSurveyedSurfaces, FilteredBaseSurveyedSurfaces, OverallExistenceMap))
                            return RequestErrorStatus.Unknown;
                        }
                    
                        // See if we need to handle surveyed surface data for 'top'
                        // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                        if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                        {
                          if (!SurveyedSurfaces.ProcessSurveyedSurfacesForFilter(SiteModel.ID, TopFilter, FilteredBaseSurveyedSurfaces, FilteredTopSurveyedSurfaces, OverallExistenceMap))
                            return RequestErrorStatus.Unknown;
                        }
                    }

                    // Add in the production data existence map to the computed surveyed surfaces existence maps
                    OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

                    // If necessary, impose spatial constraints from filter design(s)
                    if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenFilterAndDesign)
                    {
                        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, BaseFilter, OverallExistenceMap))
                            return RequestErrorStatus.Unknown;
                    }

                    if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                    {
                        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, TopFilter, OverallExistenceMap))
                            return RequestErrorStatus.Unknown;
                    }

                    var PipelinedTask = new VolumesComputationTask
                    {
                        Aggregator = Aggregator
                    };
                 
                    try
                    {
                        PipeLine = new SubGridPipelineAggregative<SubGridsRequestArgument, SimpleVolumesResponse>(/*0, */ PipelinedTask);
                        PipelinedTask.PipeLine = PipeLine;

                        ConfigurePipeline(PipeLine);

                        if (PipeLine.Initiate())
                        {
                          PipeLine.WaitForCompletion().ContinueWith(x =>
                          {
                            if (x.Result)
                              Log.LogInformation("WaitForCompletion successful");
                            else // No signal was received, the wait timed out...            
                              Log.LogInformation($"WaitForCompletion timed out with {PipeLine.SubGridsRemainingToProcess} subgrids remaining to be processed");
                          }).Wait();
                        }

                        /*
                        while not FPipeLine.AllFinished and not FPipeLine.PipelineAborted do
                          begin
                            WaitResult := FPipeLine.CompleteEvent.WaitFor(5000);

                            if VLPDSvcLocations.Debug_EmitSubgridPipelineProgressLogging then
                              begin
                                if ((FEpochCount > 0) or (FPipeLine.SubmissionNode.TotalNumberOfSubgridsScanned > 0)) and
                                   ((FPipeLine.OperationNode.NumPendingResultsReceived > 0) or (FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0)) then
                                  SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d): #Progress# - Scanned = %d, Submitted = %d, Processed = %d (with %d pending and %d results outstanding)',
                                                                          [Self.ClassName,
                                                                           FRequestDescriptor, FPipeline.ProjectUid,
                                                                           FPipeLine.SubmissionNode.TotalNumberOfSubgridsScanned,
                                                                           FPipeLine.SubmissionNode.TotalSumbittedSubgridRequests,
                                                                           FPipeLine.OperationNode.TotalOperatedOnSubgrids,
                                                                           FPipeLine.OperationNode.NumPendingResultsReceived,
                                                                           FPipeLine.OperationNode.OustandingSubgridsToOperateOn]), slmcDebug);
                              end;

                            if (WaitResult = wrSignaled) and not FPipeLine.AllFinished and not FPipeLine.PipelineAborted and not FPipeLine.Terminated then
                              begin
                                if ShouldAbortDueToCompletedEventSet then
                                  begin
                                    if (FPipeLine.OperationNode.NumPendingResultsReceived > 0) or (FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0) then
                                      SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d) being aborted as it''s completed event has remained set but still has work to do (%d outstanding subgrids, %d pending results to process) over a sleep epoch',
                                                                            [Self.ClassName,
                                                                             FRequestDescriptor, FPipeline.ProjectUid,
                                                                             FPipeLine.OperationNode.OustandingSubgridsToOperateOn,
                                                                             FPipeLine.OperationNode.NumPendingResultsReceived]), slmcError);
                                    FPipeLine.Abort;
                                    ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.PerformTaskCancellation(FPipelinedTask);
                                    Exit;
                                  end
                                else
                                  begin
                                    if (FPipeLine.OperationNode.NumPendingResultsReceived > 0) or (FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0) then
                                      SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d) has it''s completed event set but still has work to do (%d outstanding subgrids, %d pending results to process)',
                                                                            [Self.ClassName,
                                                                             FRequestDescriptor, FPipeline.ProjectUid,
                                                                             FPipeLine.OperationNode.OustandingSubgridsToOperateOn,
                                                                             FPipeLine.OperationNode.NumPendingResultsReceived]), slmcDebug);
                                    Sleep(500);
                                    ShouldAbortDueToCompletedEventSet := True;
                                  end;
                              end;

                            if FPipeLine.TimeToLiveExpired then
                              begin
                                FAbortedDueToTimeout := True;
                                FPipeLine.Abort;
                                ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.PerformTaskCancellation(FPipelinedTask);

                                // The pipeline has exceed its allotted time to complete. It will now
                                // be aborted and this request will be failed.
                                SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d) aborted due to time to live expiration (%d seconds)',
                                                                        [Self.ClassName, FRequestDescriptor, FPipeLine.TimeToLiveSeconds]), slmcError);
                                Exit;
                              end;
                         */

                        PipelineAborted = PipeLine.Aborted;

                        if (!PipeLine.Terminated && !PipeLine.Aborted)
                            Result = RequestErrorStatus.OK;
                    }
                    finally
                    {
                        if (AbortedDueToTimeout)
                            Result = RequestErrorStatus.AbortedDueToPipelineTimeout;
                        else
                            if (PipelinedTask.IsCancelled || PipelineAborted)
                                Result = RequestErrorStatus.RequestHasBeenCancelled;
                    }
                }
                catch (Exception e)
                {
                    Log.LogError(e, "ExecutePipeline raised exception");
                }

                return Result;
            }
            catch (Exception e)
            {
                Log.LogError(e, "Exception");
            }

            return RequestErrorStatus.Unknown;
        }

      /*
      public RequestErrorStatus ExecutePipelineEx()
      {
        using (var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild
        (requestDescriptor: RequestDescriptor,
          dataModelID: SiteModel.ID,
          siteModel: SiteModel,
          gridDataType: GridDataType.Height,
          response: new SimpleVolumesResponse(), // todo or any predefined response object
          filters: new FilterSet(BaseFilter, TopFilter),
          cutFillDesign: ReferenceDesign,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.SimpleVolumes),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultAggregative),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requestRequiresAccessToDesignFileExistenceMap: ReferenceDesignUID != Guid.Empty,
          requireSurveyedSurfaceInformation: true, // todo -> IncludeSurveyedSurfaces,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
        ))
        {
        }

        return RequestErrorStatus.OK;
      }
      */
  }
}
