﻿using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids.GridFabric.Requests
{
    /// <summary>
    /// The SubGridRequests GridFabric class sends a request to the grid for a collection of sub grids to be processed according 
    /// to relevant filters other parameters. The grid fabric responds with responses as the servers in the fabric compute them, sending
    /// them to the TRex node identified by the TRexNodeId property
    /// </summary>
    public abstract class SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse> : CacheComputePoolRequest<TSubGridsRequestArgument, TSubGridRequestsResponse> 
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

        /// <summary>
        /// Task is the business logic that will handle the response to the sub grids request
        /// </summary>
        public ITRexTask TRexTask;

        /// <summary>
        /// The request argument to be passed to target of the request
        /// </summary>
        public TSubGridsRequestArgument arg;

        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public Guid SiteModelID { get; set; } = Guid.Empty;

        /// <summary>
        /// The request ID assigned to the activity requiring these sub grids to be requested. This ID is used to funnel 
        /// traffic from the processing cluster into the correct processing context
        /// </summary>
        public Guid RequestID { get; set; } = Guid.Empty;

        /// <summary>
        /// The identifier of the TRex Node that is issuing the request for sub grids and which wants to receive the processed
        /// sub grid responses
        /// </summary>
        public Guid TRexNodeId { get; set; } = Guid.Empty;

        /// <summary>
        /// The type of grid data to be retrieved from the sub grid requests
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// A sub grid bit mask tree identifying all the production data sub grids that require processing
        /// </summary>
        public ISubGridTreeBitMask ProdDataMask { get; set; }

        /// <summary>
        /// A sub grid bit mask tree identifying all the surveyed surface sub grids that require processing
        /// </summary>
        public ISubGridTreeBitMask SurveyedSurfaceOnlyMask { get; set; }

        /// <summary>
        /// The set of filters to be applied to the sub grids being processed
        /// </summary>
        public IFilterSet Filters { get; set; }

        /// <summary>
        /// Denotes whether results of these requests should include any surveyed surfaces in the site model
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; }

        /// <summary>
        /// The design to be used in cases of cut/fill sub grid requests together with its offset for a reference surface
        /// </summary>
        public DesignOffset ReferenceDesign { get; set; } = new DesignOffset();

        public AreaControlSet AreaControlSet { get; set; } = AreaControlSet.CreateAreaControlSet();

        /// <summary>
        /// A lambda to provide custom initialization of specialist sub grids arguments used for different purposes
        /// </summary>
        public Action<TSubGridsRequestArgument> CustomArgumentInitializer { get; set; }

        public SubGridsRequestComputeStyle SubGridsRequestComputeStyle { get; set; } = SubGridsRequestComputeStyle.Normal;

        /// <summary>
        /// No arg constructor that establishes this request as a cache compute request. 
        /// of sub grid processing is returned as a set of partitioned results from the Broadcast() invocation.
        /// </summary>
        public SubGridRequestsBase()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SubGridRequestsBase(ITRexTask tRexTask,
                                   Guid siteModelID, 
                                   Guid requestID, 
                                   Guid trexNodeId, 
                                   GridDataType requestedGridDataType, 
                                   bool includeSurveyedSurfaceInformation,
                                   ISubGridTreeBitMask prodDataMask,
                                   ISubGridTreeBitMask surveyedSurfaceOnlyMask,
                                   IFilterSet filters,
                                   DesignOffset referenceDesign,
                                   AreaControlSet areaControlSet,
                                   Action<TSubGridsRequestArgument> customArgumentInitializer,
                                   SubGridsRequestComputeStyle subGridsRequestComputeStyle) : this()
        {
            TRexTask = tRexTask;
            SiteModelID = siteModelID;
            RequestID = requestID;
            TRexNodeId = trexNodeId;
            RequestedGridDataType = requestedGridDataType;
            IncludeSurveyedSurfaceInformation = includeSurveyedSurfaceInformation;
            ProdDataMask = prodDataMask;
            SurveyedSurfaceOnlyMask = surveyedSurfaceOnlyMask;
            Filters = filters;
            ReferenceDesign = referenceDesign;
            AreaControlSet = areaControlSet;
            CustomArgumentInitializer = customArgumentInitializer;
            SubGridsRequestComputeStyle = SubGridsRequestComputeStyle;
        }

        /// <summary>
        /// Unpacks elements of the request argument that are represented as byte arrays in the Ignite request
        /// </summary>
        protected void PrepareArgument()
        {
            var originatingIgniteNodeId = Ignite.GetCluster().GetLocalNode().Id;
            Log.LogInformation($"Preparing argument with OriginatingIgniteNodeId = {originatingIgniteNodeId}, TRexNodeId = {TRexNodeId}");

            arg = new TSubGridsRequestArgument
            {
                ProjectID = SiteModelID,
                RequestID = RequestID,
                GridDataType = RequestedGridDataType,
                IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaceInformation,
                ProdDataMaskBytes = ProdDataMask.ToBytes(),
                SurveyedSurfaceOnlyMaskBytes = SurveyedSurfaceOnlyMask.ToBytes(),
                Filters = Filters,
                OriginatingIgniteNodeId = originatingIgniteNodeId,
                TRexNodeID = TRexNodeId,
                ReferenceDesign = ReferenceDesign,
                AreaControlSet = AreaControlSet,
                SubGridsRequestComputeStyle = SubGridsRequestComputeStyle
            };

            CustomArgumentInitializer?.Invoke(arg);
        }

        protected void CheckArguments()
        {
            // Make sure things look kosher
            if (ProdDataMask == null || SurveyedSurfaceOnlyMask == null || Filters == null || RequestID == Guid.Empty)
            {
                if (ProdDataMask == null)
                    throw new ArgumentException("ProdDataMask not initialized");
                if (SurveyedSurfaceOnlyMask == null)
                    throw new ArgumentException("SurveyedSurfaceOnlyMask not initialized");
                if (Filters == null)
                    throw new ArgumentException("Filters not initialized");
                if (RequestID == Guid.Empty)
                    throw new ArgumentException("RequestID not initialized");
            }
        }

      /// <summary>
      /// Executes a request for a number of sub grids to be processed according to filters and other
      /// parameters
      /// </summary>
      public abstract TSubGridRequestsResponse Execute();

      /// <summary>
      /// Executes a request for a number of sub grids to be processed according to filters and other
      /// parameters
      /// </summary>
      public abstract Task<TSubGridRequestsResponse> ExecuteAsync();
  }
}
