﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Executors
{
  /// <summary>
  /// Executes business logic that calculates the profile between two points in space
  /// </summary>
  public class ComputeProfileExecutor_ClusterCompute
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ComputeProfileExecutor_ClusterCompute>();

    private Guid ProjectID;
    private GridDataType ProfileTypeRequired;
    private XYZ[] NEECoords;
    private FilterSet Filters;

    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;
    private DesignDescriptor DesignDescriptor;
    private bool ReturnAllPassesAndLayers;

    private ISubGridSegmentCellPassIterator CellPassIterator;
    private ISubGridSegmentIterator SegmentIterator;

    /// <summary>
    /// Constructs the profile analysis executor
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="profileTypeRequired"></param>
    /// <param name="nEECoords"></param>
    /// <param name="filters"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    public ComputeProfileExecutor_ClusterCompute(Guid projectID, GridDataType profileTypeRequired, XYZ[] nEECoords, FilterSet filters,
      // todo liftBuildSettings: TICLiftBuildSettings;
      // externalRequestDescriptor: TASNodeRequestDescriptor;
      DesignDescriptor designDescriptor, bool returnAllPassesAndLayers)
    {
      ProjectID = projectID;
      ProfileTypeRequired = profileTypeRequired;
      NEECoords = nEECoords;
      Filters = filters;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }

    /// <summary>
    /// Create and configure the segment iterator to be used
    /// </summary>
    /// <param name="passFilter"></param>
    private void SetupForCellPassStackExamination(CellPassAttributeFilter passFilter)
    {
      SegmentIterator = new SubGridSegmentIterator(null, null, SiteModels.SiteModels.Instance().ImmutableStorageProxy); 
      if (passFilter.ReturnEarliestFilteredCellPass ||
          (passFilter.HasElevationTypeFilter && passFilter.ElevationType == ElevationType.First))
        SegmentIterator.IterationDirection = IterationDirection.Forwards;
      else
        SegmentIterator.IterationDirection = IterationDirection.Backwards;

      if (passFilter.HasMachineFilter)
        SegmentIterator.SetMachineRestriction(passFilter.MachineIDSet);

      // Create and configure the cell pass iterator to be used
      CellPassIterator = new SubGridSegmentCellPassIterator_NonStatic
      {
        SegmentIterator = SegmentIterator
      }; 

      CellPassIterator.SetTimeRange(passFilter.HasTimeFilter, passFilter.StartTime, passFilter.EndTime);
    }

    /// <summary>
    /// Executes the profiler logic in the cluster compute context where each cluster node processes its fraction of the work and returns the
    /// results to the application service context
    /// </summary>
    public ProfileRequestResponse Execute()
    {
//        ResponseDataStream    : TMemoryStream;
//        Packager              : TICProfileCellListPackager;
//      SubGridTreeSubGridExistenceBitMask OverallExistenceMap;

      // todo Args.LiftBuildSettings.CCVSummaryTypes := Args.LiftBuildSettings.CCVSummaryTypes + [iccstCompaction];
      // todo Args.LiftBuildSettings.MDPSummaryTypes := Args.LiftBuildSettings.MDPSummaryTypes + [icmdpCompaction];

      try
      {
        ProfileRequestResponse Response = new ProfileRequestResponse();
        List<ProfileCell> ProfileCells = new List<ProfileCell>(1000);

        try
        {
          // Note: Start/end point lat/lon fields have been converted into grid local coordinate system by this point
          if (NEECoords.Length > 0)
            Log.LogInformation($"#In#: DataModel {ProjectID}, Vertices:{NEECoords[0]} -> {NEECoords[0]}");
          else
            Log.LogInformation($"#In#: DataModel {ProjectID}, Note! verticies list is empty");

          ISiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(ProjectID);

          if (SiteModel == null)
          {
            Log.LogWarning($"Failed to locate sitemodel {ProjectID}");
            return Response = new ProfileRequestResponse {ResultStatus = RequestErrorStatus.NoSuchDataModel};
          }

          // Obtain the subgrid existence map for the project
          SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap = SiteModel.ExistanceMap;

          if (ProdDataExistenceMap == null)
          {
            Log.LogWarning($"Failed to locate production data existence map from sitemodel {ProjectID}");
            return Response = new ProfileRequestResponse {ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap};
          }

          CellSpatialFilter CellFilter = Filters.Filters[0].SpatialFilter;
          CellPassAttributeFilter PassFilter = Filters.Filters[0].AttributeFilter;

          FilteredValuePopulationControl PopulationControl = new FilteredValuePopulationControl();
          PopulationControl.PreparePopulationControl(ProfileTypeRequired, PassFilter);

          // Raptor profile implementation did not use the overall existence map, so this commented out code
          // has no effect in Raptor and has been exluced for this reason in TRex.
          //if (DesignProfilerService.RequestCombinedDesignSubgridIndexMap(ProjectID, SiteModel.Grid.CellSize, SiteModel.SurveyedSurfaces, OverallExistenceMap) = dppiOK)
          //  OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);
          //else
          //  return Response = new ProfileRequestResponse {ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap};

          Log.LogInformation("Creating IProfileBuilder");

          IProfilerBuilder Profiler = new ProfilerBuilder(SiteModel, ProdDataExistenceMap, ProfileTypeRequired, PassFilter, CellFilter, 
            /* todo design: */null, /* todo elevation range design: */null,
            PopulationControl, new CellPassFastEventLookerUpper(SiteModel));

          // todo {$IFDEF DEBUG}
          //SIGLogMessage.PublishNoODS(Self, Format('RequestProfile: BuildCellPassProfile for sitemodel %d', [Args.DataModelID]), slmcMessage);
          //{$ENDIF}

          Log.LogInformation("Building cell profile");
          if (Profiler.CellProfileBuilder.Build(NEECoords, ProfileCells))
          {
            // todo {$IFDEF DEBUG}
            // SIGLogMessage.PublishNoODS(Self, Format('RequestProfile: BuildLiftProfileFromInitialLayer for sitemodel %d', [Args.DataModelID]), slmcMessage);
            // {$ENDIF}

            SetupForCellPassStackExamination(PassFilter);

            Log.LogInformation("Building lift profile");
            if (Profiler.ProfileLiftBuilder.Build(ProfileCells, CellPassIterator))
            {
              Log.LogInformation("Lift profile building succeeded");

              return new ProfileRequestResponse
              {
                ProfileCells = ProfileCells,
                ResultStatus = RequestErrorStatus.OK
              };
            }

            Log.LogInformation("Lift profile building failed");
          }

          // todo... {$IFDEF DEBUG}
          // SIGLogMessage.PublishNoODS(Self, Format('RequestProfile: completed construction for sitemodel %d', [Args.DataModelID]), slmcMessage);
          // {$ENDIF}

          /*       if ServerResult = icsrrNoError then
                   begin
                     Packager := TICProfileCellListPackager.Create;
                     Packager.CellList := Profiler.Profile;
                     Packager.GridDistanceBetweenProfilePoints := Profiler.GridDistanceBetweenProfilePoints;
                     Packager.WriteCellPassesAndLayers := Args.ReturnAllPassesAndLayers;

                     ResponseDataStream := TMemoryStream.Create;
                     Packager.WriteToStream(ResponseDataStream);
                   end;
          */
        }
        finally
        {
          Log.LogInformation($"#Out# Execute: DataModel {ProjectID} complete for profile line. #Result#:{Response.ResultStatus} with {Response.ProfileCells?.Count ?? 0} vertices");
        }
      }
      catch (Exception E)
      {
        Log.LogError($"Execute: Exception {E}");
      }

      return new ProfileRequestResponse();
    }
  }
}
