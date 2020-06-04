﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Common.Models;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SubGrids.Executors
{
  /// <summary>
  /// The closure/function that implements sub grid request processing on compute nodes
  /// </summary>
  public abstract class SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    public const string SUB_GRIDS_REQUEST_ADDRESS_BUCKET_SIZE = "SUB_GRIDS_REQUEST_ADDRESS_BUCKET_SIZE";

    private readonly int _addressBucketSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(SUB_GRIDS_REQUEST_ADDRESS_BUCKET_SIZE, 50);

    private readonly IRequestorUtilities _requestorUtilities = DIContext.Obtain<IRequestorUtilities>();

    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory _clientLeafSubGridFactory;

    private IClientLeafSubGridFactory ClientLeafSubGridFactory
      => _clientLeafSubGridFactory ?? (_clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>());

    /// <summary>
    /// Mask is the internal sub grid bit mask tree created from the serialized mask contained in the 
    /// ProdDataMaskBytes member of the argument. It is only used during processing of the request.
    /// It is marked as non serialized so the Ignite GridCompute Broadcast method does not attempt 
    /// to serialize this member as an aspect of the compute func.
    /// </summary>
    private ISubGridTreeBitMask ProdDataMask;

    /// <summary>
    /// Mask is the internal sub grid bit mask tree created from the serialized mask contained in the 
    /// SurveyedSurfaceOnlyMaskBytes member of the argument. It is only used during processing of the request.
    /// It is marked as non serialized so the Ignite GridCompute Broadcast method does not attempt 
    /// to serialize this member as an aspect of the compute func.
    /// </summary>
    private ISubGridTreeBitMask SurveyedSurfaceOnlyMask;

    protected TSubGridsRequestArgument localArg;

    private ISiteModel siteModel;

    private ISiteModels siteModels;

    /// <summary>
    /// The list of address being constructed prior to submission to the processing engine
    /// </summary>
    private SubGridCellAddress[] addresses;

    /// <summary>
    /// The number of sub grids currently present in the process pending list
    /// </summary>
    private int listCount;

    /// <summary>
    /// The Design to be used for querying elevation information from in the process of calculating cut-fill values
    /// together with its offset for a reference surface
    /// </summary>
    private IDesignWrapper ReferenceDesignWrapper;

    /// <summary>
    /// Any overriding targets to be used instead of machine targets
    /// </summary>
    private IOverrideParameters Overrides;

    /// <summary>
    /// Parameters for lift analysis
    /// </summary>
    private ILiftParameters LiftParams;

    /// <summary>
    /// Cleans an array of client leaf sub grids by repatriating them to the client leaf sub grid factory
    /// </summary>
    /// <param name="SubGridResultArray"></param>
    private void CleanSubGridResultArray(IClientLeafSubGrid[] SubGridResultArray)
    {
      if (SubGridResultArray != null)
        ClientLeafSubGridFactory.ReturnClientSubGrids(SubGridResultArray, SubGridResultArray.Length);
    }

    /// <summary>
    /// Performs conversions from the internal sub grid client leaf type to the requested client leaf type
    /// </summary>
    /// <param name="RequestGridDataType"></param>
    /// <param name="SubGridResultArray"></param>
    private void ConvertIntermediarySubGridsToResult(GridDataType RequestGridDataType,
      ref IClientLeafSubGrid[] SubGridResultArray)
    {
      var NewClientGrids = new IClientLeafSubGrid[SubGridResultArray.Length];

      try
      {
        // If performing simple volume calculations, there may be an intermediary filter in play. If this is
        // the case then the first two sub grid results will be HeightAndTime elevation sub grids and will
        // need to be merged into a single height and time sub grid before any secondary conversion of intermediary
        //  results in the logic below.

        if (SubGridResultArray.Length == 3 // Three filters in play
            && SubGridResultArray[0].GridDataType == GridDataType.HeightAndTime // Height and time sub grids
            && SubGridResultArray[1].GridDataType == GridDataType.HeightAndTime
            && SubGridResultArray[2].GridDataType == GridDataType.HeightAndTime
        )
        {
          var SubGrid1 = (ClientHeightAndTimeLeafSubGrid) SubGridResultArray[0];
          var SubGrid2 = (ClientHeightAndTimeLeafSubGrid) SubGridResultArray[1];

          // Merge the first two results then swap the second and third items so later processing
          // uses the correct two result, and the the third is correctly recycled
          // Subgrid1 is 'latest @ first filter', sub grid 2 is earliest @ second filter
          SubGridUtilities.SubGridDimensionalIterator((I, J) =>
          {
            // Check if there is a non null candidate in the earlier @ second filter
            if (SubGrid1.Cells[I, J] == Consts.NullHeight && SubGrid2.Cells[I, J] != Consts.NullHeight)
              SubGrid1.Cells[I, J] = SubGrid2.Cells[I, J];
          });

          // Swap the lst two elements...
          MinMax.Swap(ref SubGridResultArray[1], ref SubGridResultArray[2]);
        }

        if (SubGridResultArray.Length == 0)
          return;

        try
        {
          for (var I = 0; I < SubGridResultArray.Length; I++)
          {
            if (SubGridResultArray[I] == null)
              continue;

            var subGridResult = SubGridResultArray[I];

            if (subGridResult.GridDataType != RequestGridDataType)
            {
              switch (RequestGridDataType)
              {
                case GridDataType.SimpleVolumeOverlay:
                  throw new TRexSubGridProcessingException("SimpleVolumeOverlay not implemented");

                case GridDataType.Height:
                  NewClientGrids[I] = ClientLeafSubGridFactory.GetSubGridEx(GridDataType.Height, siteModel.CellSize, siteModel.Grid.NumLevels, subGridResult.OriginX, subGridResult.OriginY);

                  /*
                  Debug.Assert(NewClientGrids[I] is ClientHeightLeafSubGrid, $"NewClientGrids[I] is ClientHeightLeafSubGrid failed, is actually {NewClientGrids[I].GetType().Name}/{NewClientGrids[I]}");
                  if (!(SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid))
                      Debug.Assert(SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid, $"SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid failed, is actually {SubGridResultArray[I].GetType().Name}/{SubGridResultArray[I]}");
                  */

                  (NewClientGrids[I] as ClientHeightLeafSubGrid).Assign(subGridResult as ClientHeightAndTimeLeafSubGrid);
                  break;

                case GridDataType.CutFill:
                  // Just copy the height sub grid to new sub grid list
                  NewClientGrids[I] = subGridResult;
                  SubGridResultArray[I] = null;
                  break;
              }
            }
            else
            {
              NewClientGrids[I] = subGridResult;
              SubGridResultArray[I] = null;
            }
          }

        }
        finally
        {
          CleanSubGridResultArray(SubGridResultArray);
        }

        SubGridResultArray = NewClientGrids;
      }
      catch
      {
        CleanSubGridResultArray(NewClientGrids);
        throw;
      }
    }

    /// <summary>
    /// Take the supplied argument to the compute func and perform any necessary unpacking of the
    /// contents of it into a form ready to use. Also make a location reference to the arg parameter
    /// to allow other methods to access it as local state.
    /// </summary>
    /// <param name="arg"></param>
    public virtual void UnpackArgument(TSubGridsRequestArgument arg)
    {
      localArg = arg;

      siteModels = DIContext.Obtain<ISiteModels>();
      siteModel = siteModels.GetSiteModel(localArg.ProjectID);

      // Unpack the mask from the argument.
      if (arg.ProdDataMaskBytes != null)
      {
        ProdDataMask = new SubGridTreeSubGridExistenceBitMask();
        ProdDataMask.FromBytes(arg.ProdDataMaskBytes);
      }

      if (arg.SurveyedSurfaceOnlyMaskBytes != null)
      {
        SurveyedSurfaceOnlyMask = new SubGridTreeSubGridExistenceBitMask();
        SurveyedSurfaceOnlyMask.FromBytes(arg.SurveyedSurfaceOnlyMaskBytes);
      }

      // Set up any required cut fill design
      if ((arg.ReferenceDesign?.DesignID ?? Guid.Empty) != Guid.Empty)
      {
        ReferenceDesignWrapper = new DesignWrapper(arg.ReferenceDesign, siteModel.Designs.Locate(arg.ReferenceDesign.DesignID));
      }

      Overrides = arg.Overrides;
      LiftParams = arg.LiftParams;
    }

    /// <summary>
    /// Take a sub grid address and a set of requesters and request the required client sub grid depending on GridDataType
    /// </summary>
    private async Task<(ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[]> PerformSubGridRequest(ISubGridRequestor[] requesters, SubGridCellAddress address)
    {
      //################################################
      // Special case for DesignHeight sub grid requests
      // Todo: This should be refactored out into another method
      //################################################

      if (localArg.GridDataType == GridDataType.DesignHeight)
      {
        var designHeightResult = new (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[] { (ServerRequestResult.UnknownError, null) };
        var getGetDesignHeights = await ReferenceDesignWrapper.Design.GetDesignHeights(localArg.ProjectID, ReferenceDesignWrapper.Offset, address, siteModel.CellSize);

        designHeightResult[0].clientGrid = getGetDesignHeights.designHeights;
        if (getGetDesignHeights.errorCode == DesignProfilerRequestResult.OK || getGetDesignHeights.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          designHeightResult[0].requestResult = ServerRequestResult.NoError;
          return designHeightResult;
        }

        Log.LogError($"Design profiler sub grid elevation request for {address} failed with error {getGetDesignHeights.errorCode}");

        designHeightResult[0].requestResult = ServerRequestResult.FailedToComputeDesignElevationPatch;
        return designHeightResult;
      }

      // ##################################
      // General case for sub grid requests
      // ##################################

      var result = new (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[requesters.Length];

      var requestCount = 0;
      // Reach into the sub grid request layer and retrieve an appropriate sub grid
      foreach (var requester in requesters)
      {
        var requestSubGridInternalResult = await requester.RequestSubGridInternal(address, address.ProdDataRequested, address.SurveyedSurfaceDataRequested);
        var subGridResult = (requestSubGridInternalResult.requestResult, requestSubGridInternalResult.clientGrid);

        if (subGridResult.requestResult != ServerRequestResult.NoError)
          Log.LogError($"Request for sub grid {address} request failed with code {result}");

        result[requestCount++] = subGridResult;
      }

      // Some request types require additional processing of the sub grid results prior to repatriating the answers back to the caller
      // Convert the computed intermediary grids into the client grid form expected by the caller
      if (result[0].clientGrid?.GridDataType != localArg.GridDataType)
      {
        // Convert to an array to preserve the multiple filter semantic giving a list of sub grids to be converted (eg: volumes)
        var clientArray = result.Select(x => x.clientGrid).ToArray();

        ConvertIntermediarySubGridsToResult(localArg.GridDataType, ref clientArray);

        // If the requested data is cut fill derived from elevation data previously calculated, 
        // then perform the conversion here
        if (localArg.GridDataType == GridDataType.CutFill)
        {
          if (clientArray.Length == 1)
          {
            // The cut fill is defined between one production data derived height sub grid and a
            // height sub grid to be calculated from a designated design
            var computeCutFillSubGridResult = await CutFillUtilities.ComputeCutFillSubGrid(
              clientArray[0], // base
              ReferenceDesignWrapper, // 'top'
              localArg.ProjectID);

            if (!computeCutFillSubGridResult.executionResult)
            {
              ClientLeafSubGridFactory.ReturnClientSubGrid(ref result[0].clientGrid);
              result[0].requestResult = ServerRequestResult.FailedToComputeDesignElevationPatch;
            }
            else
            {
              result[0].clientGrid = clientArray[0];
            }
          }

          // If the requested data is cut fill derived from two elevation data sub grids previously calculated, 
          // then perform the conversion here
          if (clientArray.Length == 2)
          {
            // The cut fill is defined between two production data derived height sub grids
            // depending on volume type work out height difference
            CutFillUtilities.ComputeCutFillSubGrid((IClientHeightLeafSubGrid)clientArray[0], // 'base'
              (IClientHeightLeafSubGrid)clientArray[1]); // 'top'

            // ComputeCutFillSubGrid has placed the result of the cut fill computation into clientGrids[0],
            // so clientGrids[1] can be discarded
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref clientArray[1]);

            result = new [] {(ServerRequestResult.NoError, clientArray[0])};
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Method responsible for accepting sub grids from the query engine and processing them in the next step of
    /// the request
    /// </summary>
    /// <param name="results"></param>
    /// <param name="resultCount"></param>
    protected abstract void ProcessSubGridRequestResult(IClientLeafSubGrid[][] results, int resultCount);

    /// <summary>
    /// Transforms the internal aggregation state into the desired response for the request
    /// </summary>
    /// <returns></returns>
    protected abstract TSubGridRequestsResponse AcquireComputationResult();

    /// <summary>
    /// Performs any necessary setup and configuration of Ignite infrastructure to support the processing of this request
    /// </summary>
    protected abstract bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse);

    /// <summary>
    /// Process a subset of the full set of sub grids in the request
    /// </summary>
    private async Task PerformSubGridRequestList(SubGridCellAddress[] addressList, int addressCount)
    {
      if (addressCount == 0)
        return;

      // Construct the set of requester objects to be used for the filters present in the request
      var requestors = _requestorUtilities.ConstructRequestors(localArg,
        siteModel, localArg.Overrides, localArg.LiftParams, RequestorIntermediaries, localArg.AreaControlSet, ProdDataMask);

      //Log.LogInformation("Sending {0} sub grids to caller for processing", count);
      //Log.LogInformation($"Requester list contains {Requestors.Length} items");

      var clientGridTasks = new Task<(ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[]>[addressCount];

      // Execute a client grid request for each requester and create an array of the results
      addressList.ForEach((x, i) => clientGridTasks[i] = PerformSubGridRequest(requestors, x));
      await Task.WhenAll(clientGridTasks);

      var clientGrids = clientGridTasks.Select(c => c.Result.Select(x => x.requestResult == ServerRequestResult.NoError ? x.clientGrid : null).ToArray()).ToArray();

      try
      {
        ProcessSubGridRequestResult(clientGrids, addressCount);
      }
      finally
      {
        // Return the client grid to the factory for recycling now its role is complete here...
        ClientLeafSubGridFactory.ReturnClientSubGrids(clientGrids, addressCount);
      }
    }

    private readonly List<Task> _tasks = new List<Task>();

    /// <summary>
    /// Processes a bucket of sub grids by creating a task for it and adding it to the tasks list for the request
    /// </summary>
    /// <param name="addressList"></param>
    /// <param name="addressCount"></param>
    private void ProcessSubGridAddressGroup(SubGridCellAddress[] addressList, int addressCount)
    {
      var addressListCopy = new SubGridCellAddress[addressCount];
      Array.Copy(addressList, addressListCopy, addressCount);

      _tasks.Add(PerformSubGridRequestList(addressListCopy, addressCount));
    }

    /// <summary>
    /// Adds a new address to the list of addresses being built and triggers processing of the list if it hits the critical size
    /// </summary>
    /// <param name="address"></param>
    private void AddSubGridToAddressList(SubGridCellAddress address)
    {
      addresses[listCount++] = address;

      if (listCount == _addressBucketSize)
      {
        // Process the sub grids...
        ProcessSubGridAddressGroup(addresses, listCount);
        listCount = 0;
      }
    }

    /// <summary>
    /// The collection of requestor intermediaries that are derived from to create requestor delegates
    /// </summary>
    private (GridDataType GridDataType,
      ICombinedFilter Filter, 
      ISurveyedSurfaces FilteredSurveyedSurfaces, 
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
      ITRexSpatialMemoryCacheContext CacheContext)[] RequestorIntermediaries;

    /// <summary>
    /// Process the set of sub grids in the request that have partition mappings that match their affinity with this node
    /// </summary>
    private TSubGridRequestsResponse PerformSubGridRequests()
    {
      // Scan through all the bitmap leaf sub grids, and for each, scan through all the sub grids as 
      // noted with the 'set' bits in the bitmask, processing only those that matter for this server

      Log.LogInformation("Scanning sub grids in request");

      addresses = new SubGridCellAddress[_addressBucketSize];

      // Obtain the primary partition map to allow this request to determine the elements it needs to process
      var primaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions();

      // Request production data only, or hybrid production data and surveyed surface data sub grids
      ProdDataMask?.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        // Is this sub grid the responsibility of this server?
        if (!primaryPartitionMap[address.ToSpatialPartitionDescriptor()])
          return;

        // Decorate the address with the production data and surveyed surface flags
        address.ProdDataRequested = true;
        address.SurveyedSurfaceDataRequested = localArg.IncludeSurveyedSurfaceInformation;

        AddSubGridToAddressList(address); // Assign the address into the group to be processed
      });

      if (localArg.IncludeSurveyedSurfaceInformation)
      {
        // Request surveyed surface only sub grids
        SurveyedSurfaceOnlyMask?.ScanAllSetBitsAsSubGridAddresses(address =>
        {
          // Is this sub grid the responsibility of this server?
          if (!primaryPartitionMap[address.ToSpatialPartitionDescriptor()])
            return;

          // Decorate the address with the production data and surveyed surface flags
          address.ProdDataRequested = false;
          address.SurveyedSurfaceDataRequested = true;

          AddSubGridToAddressList(address); // Assign the address into the group to be processed
        });
      }

      ProcessSubGridAddressGroup(addresses, listCount); // Process the remaining sub grids...

      // Wait for all the sub-tasks to complete

      Log.LogInformation($"Waiting for {_tasks.Count} sub tasks to complete for sub grids request");

      var summaryTask = Task.WhenAll(_tasks);
      summaryTask.Wait();

      if (summaryTask.Status == TaskStatus.RanToCompletion)
      {
        Log.LogInformation($"{_tasks.Count} sub grid tasks completed, executing AcquireComputationResult()");
        return AcquireComputationResult();
      }

      Log.LogError("Failed to process all sub grids");
      return null;
    }

    /// <summary>
    /// Executes the request for sub grids
    /// </summary>
    /// <returns></returns>
    public TSubGridRequestsResponse Execute()
    {
      var numProdDataSubGrids = ProdDataMask?.CountBits() ?? 0;
      var numSurveyedSurfaceSubGrids = SurveyedSurfaceOnlyMask?.CountBits() ?? 0;
      var numSubGridsToBeExamined = numProdDataSubGrids + numSurveyedSurfaceSubGrids;

      Log.LogInformation($"Num sub grids present in request = {numSubGridsToBeExamined} [All divisions], {numProdDataSubGrids} prod data (plus surveyed surface), {numSurveyedSurfaceSubGrids} surveyed surface only");

      if (!EstablishRequiredIgniteContext(out var contextEstablishmentResponse))
        return new TSubGridRequestsResponse {ResponseCode = contextEstablishmentResponse};

      RequestorIntermediaries = _requestorUtilities.ConstructRequestorIntermediaries
        (siteModel, localArg.Filters, localArg.IncludeSurveyedSurfaceInformation, localArg.GridDataType);

      var result = PerformSubGridRequests();
      result.NumSubgridsExamined = numSubGridsToBeExamined;

      //TODO: Map the actual response code into this
      result.ResponseCode = SubGridRequestsResponseResult.OK;

      return result;
    }
  }
}
