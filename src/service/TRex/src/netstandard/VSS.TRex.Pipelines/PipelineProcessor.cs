﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Supports construction and configuration of a sub grid request pipeline that mediates and orchestrates
  /// sub grid based queries
  /// </summary>
  public class PipelineProcessor<TSubGridsRequestArgument> : IPipelineProcessor<TSubGridsRequestArgument>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<PipelineProcessor<TSubGridsRequestArgument>>();

    private IExistenceMaps _existenceMaps;
    private IExistenceMaps GetExistenceMaps() => _existenceMaps ??= DIContext.Obtain<IExistenceMaps>();

    public Guid RequestDescriptor;

    /// <summary>
    /// The identifier of the project the request is operating on.
    /// </summary>
    public Guid DataModelID;

    /// <summary>
    /// The set of surveyed surfaces that are to be excluded from the computed results for the request
    /// </summary>
    public Guid[] SurveyedSurfaceExclusionList = new Guid[0];

    /// <summary>
    /// The set of filters to be executed for each sub grid examined in the request. Each filters will result in a computed
    /// sub grid variation for subsequent business logic in the pipeline task to operate on.
    /// </summary>
    public IFilterSet Filters;

    /// <summary>
    /// The spatial extents derived from the parameters when building the pipeline
    /// </summary>
    public BoundingWorldExtent3D SpatialExtents { get; set; } = BoundingWorldExtent3D.Full();

    /// <summary>
    /// Any override world coordinate spatial extent imposed by the client context.
    /// For example, this might be the rectangular border of a tile being requested
    /// </summary>
    public BoundingWorldExtent3D OverrideSpatialExtents { get; set; } = BoundingWorldExtent3D.Full();

    /// <summary>
    /// The response used as the return from the pipeline request
    /// </summary>
    public ISubGridsPipelinedReponseBase Response { get; set; }

    /// <summary>
    /// Grid data type to be processed and/or returned by the query (eg: Height, CutFill etc)
    /// </summary>
    public GridDataType GridDataType { get; set; }

    public ISubGridTreeBitMask ProdDataExistenceMap { get; set; }
    public ISubGridTreeBitMask OverallExistenceMap { get; set; }
    public ISubGridTreeBitMask DesignSubGridOverlayMap { get; set; }

    /// <summary>
    /// Flag indicating if all surveyed surface have been excluded from the request due to time filtering constraints
    /// </summary>
    public bool SurveyedSurfacesExcludedViaTimeFiltering;

    /// <summary>
    /// The identifier for any cut/fill design reference being supplied to the request together with its offset for a reference surface
    /// </summary>
    public DesignOffset CutFillDesign;

    /// <summary>
    /// Records if the pipeline was aborted before completing operations
    /// </summary>
    public bool PipelineAborted { get; set; }

    /// <summary>
    /// The task to be fitted to the pipeline to mediate sub grid retrieval and processing
    /// </summary>
    public ITRexTask Task { get; set; }

    /// <summary>
    /// The pipe line used to retrieve sub grids from the cluster compute layer
    /// </summary>
    public ISubGridPipelineBase Pipeline { get; set; }

    /// <summary>
    /// The request analyzer used to determine the sub grids to be sent to the cluster compute layer
    /// </summary>
    public IRequestAnalyser RequestAnalyser { get; set; }

    /// <summary>
    /// Reference to the site model involved in the request
    /// </summary>
    public ISiteModel SiteModel { get; private set; }

    /// <summary>
    /// Indicates if the pipeline was aborted due to a TTL timeout
    /// </summary>
    public bool AbortedDueToTimeout { get; set; }

    /// <summary>
    /// Indicates if the pipeline requests should include surveyed surface information
    /// </summary>
    public bool RequireSurveyedSurfaceInformation { get; set; }

    /// <summary>
    /// If this request involves a relationship with a design then ensure the existence map
    /// for the design is loaded in to memory to allow the request pipeline to confine
    /// sub grid requests that overlay the actual design
    /// </summary>
    public bool RequestRequiresAccessToDesignFileExistenceMap { get; set; }

    /// <summary>
    /// Parameters used for lift analysis
    /// </summary>
    public ILiftParameters LiftParams { get; set; }

    /// <summary>
    /// A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria
    /// </summary>
    public BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

    public Action<TSubGridsRequestArgument> CustomArgumentInitializer { get; set; }

    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="dataModelID"></param>
    /// <param name="gridDataType"></param>
    /// <param name="response"></param>
    /// <param name="filters"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="task"></param>
    /// <param name="pipeline"></param>
    /// <param name="requestAnalyser"></param>
    /// <param name="requireSurveyedSurfaceInformation"></param>
    /// <param name="requestRequiresAccessToDesignFileExistenceMap"></param>
    /// <param name="overrideSpatialCellRestriction">A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria</param>
    /// <param name="liftParams"></param>
    public PipelineProcessor(Guid requestDescriptor,
                             Guid dataModelID,
                             GridDataType gridDataType,
                             ISubGridsPipelinedReponseBase response,
                             IFilterSet filters,
                             DesignOffset cutFillDesign,
                             ITRexTask task,
                             ISubGridPipelineBase pipeline,
                             IRequestAnalyser requestAnalyser,
                             bool requireSurveyedSurfaceInformation,
                             bool requestRequiresAccessToDesignFileExistenceMap,
                             BoundingIntegerExtent2D overrideSpatialCellRestriction,
                             ILiftParameters liftParams)
    {
      RequestDescriptor = requestDescriptor;
      DataModelID = dataModelID;
      GridDataType = gridDataType;
      Response = response;
      Filters = filters;
      CutFillDesign = cutFillDesign;
      Task = task;

      Pipeline = pipeline;

      RequestAnalyser = requestAnalyser;

      RequireSurveyedSurfaceInformation = requireSurveyedSurfaceInformation;
      RequestRequiresAccessToDesignFileExistenceMap = requestRequiresAccessToDesignFileExistenceMap;

      OverrideSpatialCellRestriction = overrideSpatialCellRestriction;
      LiftParams = liftParams;
    }

    /// <summary>
    /// Builds the pipeline configured per the supplied state ready to execute the request
    /// </summary>
    public async Task<bool> BuildAsync()
    {
      // Ensure the task is initialised with the request descriptor
      Task.RequestDescriptor = RequestDescriptor;

      // Ensure the Task grid data type matches the pipeline processor
      Task.GridDataType = GridDataType;

      // Introduce the task and the pipeline to each other
      Pipeline.PipelineTask = Task;
      Task.PipeLine = Pipeline;

      (Pipeline as ISubGridPipelineBase<TSubGridsRequestArgument>).CustomArgumentInitializer = CustomArgumentInitializer;

      // Construct an aggregated set of excluded surveyed surfaces for the filters used in the query
      foreach (var filter in Filters.Filters)
      {
        if (filter != null && SurveyedSurfaceExclusionList.Length > 0)
        {
          SurveyedSurfaceExclusionList = new Guid[filter.AttributeFilter.SurveyedSurfaceExclusionList.Length];
          Array.Copy(filter.AttributeFilter.SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList,
            SurveyedSurfaceExclusionList.Length);
        }
      }

      // Get the SiteModel for the request
      SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
      if (SiteModel == null)
      {
        Response.ResultStatus = RequestErrorStatus.NoSuchDataModel;
        return false;
      }

      SpatialExtents = SiteModel.GetAdjustedDataModelSpatialExtents(SurveyedSurfaceExclusionList);

      if (!SpatialExtents.IsValidPlanExtent)
      {
        Response.ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // Or there was no data in the model
        return false;
      }

      // Get the current production data existence map from the site model
      ProdDataExistenceMap = SiteModel.ExistenceMap;

      // Obtain the sub grid existence map for the project
      // Retrieve the existence map for the datamodel
      OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask
      {
        CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModel.CellSize
      };

      if (RequireSurveyedSurfaceInformation)
      {
        // Obtain local reference to surveyed surfaces (lock free access)
        var localSurveyedSurfaces = SiteModel.SurveyedSurfaces;

        if (localSurveyedSurfaces != null)
        {
          // Construct two filtered surveyed surface lists to act as a rolling pair used as arguments
          // to the ProcessSurveyedSurfacesForFilter method
          var filterSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
          var filteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

          SurveyedSurfacesExcludedViaTimeFiltering = Filters.Filters.Length > 0;

          foreach (var filter in Filters.Filters)
          {
            if (!localSurveyedSurfaces.ProcessSurveyedSurfacesForFilter(DataModelID, filter,
              filteredSurveyedSurfaces, filterSurveyedSurfaces, OverallExistenceMap))
            {
              Response.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
              return false;
            }

            SurveyedSurfacesExcludedViaTimeFiltering &= filterSurveyedSurfaces.Count == 0;
          }
        }
      }

      OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

      foreach (var filter in Filters.Filters)
      {
        if (filter != null)
        {
          if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, filter, OverallExistenceMap))
          {
            Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
            return false;
          }

          if (filter.AttributeFilter.AnyFilterSelections)
          {
            Response.ResultStatus = FilterUtilities.PrepareFilterForUse(filter, DataModelID);
            if (Response.ResultStatus != RequestErrorStatus.OK)
            {
              _log.LogInformation($"PrepareFilterForUse failed: Datamodel={DataModelID}");
              return false;
            }
          }
        }
      }

      // Adjust the extents we have been given to encompass the spatial extent of the supplied filters (if any)
      Filters.ApplyFilterAndSubsetBoundariesToExtents(SpatialExtents);

      // If this request involves a relationship with a design then ensure the existence map
      // for the design is loaded in to memory to allow the request pipeline to confine
      // sub grid requests that overlay the actual design
      if (RequestRequiresAccessToDesignFileExistenceMap)
      {
        if (CutFillDesign == null || CutFillDesign.DesignID == Guid.Empty)
        {
            _log.LogError($"No design provided to cut fill, summary volume or thickness overlay render request for datamodel {DataModelID}");
            Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
            return false;
        }

        DesignSubGridOverlayMap = GetExistenceMaps().GetSingleExistenceMap(DataModelID, ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, CutFillDesign.DesignID);

        if (DesignSubGridOverlayMap == null)
        {
          _log.LogError($"Failed to request sub grid overlay index for design {CutFillDesign.DesignID} in datamodel {DataModelID}");
          Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        DesignSubGridOverlayMap.CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModel.CellSize;
      }

      // Impose the final restriction on the spatial extents from the client context
      SpatialExtents.Intersect(OverrideSpatialExtents);

      // Introduce the Request analyzer to the pipeline and spatial extents it requires
      RequestAnalyser.Pipeline = Pipeline;
      RequestAnalyser.WorldExtents = SpatialExtents;

      ConfigurePipeline();

      return true;
    }

    /// <summary>
    /// Configures pipeline specific settings into the pipeline aspect of the processor
    /// </summary>
    protected void ConfigurePipeline()
    {
      Pipeline.GridDataType = GridDataType;
      Pipeline.PipelineTask.GridDataType = GridDataType;

      Pipeline.RequestDescriptor = RequestDescriptor;

      // PipeLine.ExternalDescriptor  = ExternalDescriptor;
      // PipeLine.SubmissionNode.DescriptorType  = cdtWMSTile;
      // PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;

      Pipeline.DataModelID = DataModelID;
      Pipeline.ReferenceDesign = CutFillDesign;

      Pipeline.LiftParams = LiftParams;

      // If summaries of compaction information (both CMV and MDP) are being displayed,
      // and the lift build settings requires all layers to be examined (so the
      // appropriate summarize top layer only flag is false), then instruct the layer
      // analysis engine to apply to restriction to the number of cell passes to use
      // to perform layer analysis (ie: all cell passes will be used).

      //Todo: Delegate this kind of specialised configuration to the client of the pipeline processor
      /*
      if (Mode == DisplayMode.CCVSummary || Mode == DisplayMode.CCVPercentSummary)
      {
        if (!Pipeline.LiftParams.CCVSummarizeTopLayerOnly)
           Pipeline.MaxNumberOfPassesToReturn = CellPassConsts.MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
      }

      if (Mode == DisplayMode.MDPSummary || Mode == DisplayMode.MDPPercentSummary)
      {
        if (!Pipeline.LiftParams.MDPSummarizeTopLayerOnly)
          Pipeline.MaxNumberOfPassesToReturn = CellPassConsts.MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
      } 
      */
     
      Pipeline.OverallExistenceMap = OverallExistenceMap;
      Pipeline.ProdDataExistenceMap = ProdDataExistenceMap;
      Pipeline.DesignSubGridOverlayMap = DesignSubGridOverlayMap;

      // Assign the filter set into the pipeline
      Pipeline.FilterSet = Filters;

      _log.LogDebug($"Extents for query against DM={DataModelID}: {SpatialExtents}");

      Pipeline.IncludeSurveyedSurfaceInformation = RequireSurveyedSurfaceInformation && !SurveyedSurfacesExcludedViaTimeFiltering;

//      Pipeline.OverrideSpatialCellRestriction = OverrideSpatialCellRestriction;

      //PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;

      Pipeline.RequestAnalyser = RequestAnalyser;
    }

    /// <summary>
    /// Performing all processing activities to retrieve sub grids
    /// </summary>
    public void Process()
    {
      try
      {
        if (Pipeline.Initiate())
        {
          Pipeline.WaitForCompletion()
            .ContinueWith(x =>
            {
              _log.LogInformation(x.Result ? "WaitForCompletion successful" : $"WaitForCompletion timed out with {Pipeline.SubGridsRemainingToProcess} sub grids remaining to be processed");
            }).Wait();
        }

        PipelineAborted = Pipeline.Aborted;

        if (!Pipeline.Terminated && !Pipeline.Aborted)
          Response.ResultStatus = RequestErrorStatus.OK;
      }
      finally
      {
        if (AbortedDueToTimeout)
          Response.ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
        else
        {
          if (Task.IsCancelled || PipelineAborted)
            Response.ResultStatus = RequestErrorStatus.RequestHasBeenCancelled;
        }
      }
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          Task?.Dispose();
        }

        Task = null;
        Pipeline = null;
        RequestAnalyser = null;
        SiteModel = null;

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
