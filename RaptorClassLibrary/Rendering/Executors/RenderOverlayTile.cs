﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Rendering;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Designs;

namespace VSS.VisionLink.Raptor.Rendering.Executors
{
    /// <summary>
    /// Renders a tile of themmatic imagery for a location in the project
    /// </summary>
    public class RenderOverlayTile
    {
        /// <summary>
        /// The Raptor application service node performing the request
        /// </summary>
        string RequestingRaptorNodeID { get; set; } = String.Empty;

        long DataModelID = -1;
        // long MachineID = -1;
        // FExternalDescriptor :TASNodeRequestDescriptor;

        DisplayMode Mode = DisplayMode.Height;

        bool CoordsAreGrid = true;

        XYZ BLPoint; // : TWGS84Point;
        XYZ TRPoint; // : TWGS84Point;

        XYZ[] NEECoords = null;
        XYZ[] LLHCoords = null;

        ushort NPixelsX = 0;
        ushort NPixelsY = 0;

        double TileRotation;
        double WorldTileWidth, WorldTileHeight;

        CombinedFilter Filter1 = null;
        CombinedFilter Filter2 = null;

        /// <summary>
        /// The identifier for the design held in the designs list ofr the project to be used to calculate cut/fill values
        /// </summary>
        public long CutFillDesignID { get; set; } = long.MinValue;
        // DesignDescriptor CutFillDesign; // DesignDescriptor

            
        // ComputeICVolumesType ReferenceVolumeType = ComputeICVolumesType.None;
        // FColourPalettes: TColourPalettes;
        // ICOptions ICOptions = new ICOptions();
        Color RepresentColor = Color.Black;

        PlanViewTileRenderer Renderer = null;

        /// <summary>
        /// Constructor for the renderer
        /// </summary>
        /// <param name="ADataModelID"></param>
        /// <param name="AMode"></param>
        /// <param name="ABLPoint"></param>
        /// <param name="ATRPoint"></param>
        /// <param name="ACoordsAreGrid"></param>
        /// <param name="ANPixelsX"></param>
        /// <param name="ANPixelsY"></param>
        /// <param name="AFilter1"></param>
        /// <param name="AFilter2"></param>
        /// <param name="ACutFillDesign"></param>
        /// <param name="ARepresentColor"></param>
        /// <param name="requestingRaptorNodeID"></param>
        public RenderOverlayTile(long ADataModelID,
                                 //AExternalDescriptor :TASNodeRequestDescriptor;
                                 DisplayMode AMode,
                                 XYZ ABLPoint, // : TWGS84Point;
                                 XYZ ATRPoint, // : TWGS84Point;
                                 bool ACoordsAreGrid,
                                 ushort ANPixelsX,
                                 ushort ANPixelsY,
                                 CombinedFilter AFilter1,
                                 CombinedFilter AFilter2,
                                 long ACutFillDesignID, //DesignDescriptor ACutFillDesign,
                                 //AReferenceVolumeType : TComputeICVolumesType;
                                 //AColourPalettes: TColourPalettes;
                                 //AICOptions: TSVOICOptions;
                                 Color ARepresentColor,
                                 string requestingRaptorNodeID
                                 )
        {
            DataModelID = ADataModelID;
            // ExternalDescriptor = AExternalDescriptor
            Mode = AMode;
            BLPoint = ABLPoint;
            TRPoint = ATRPoint;
            CoordsAreGrid = ACoordsAreGrid;
            NPixelsX = ANPixelsX;
            NPixelsY = ANPixelsY;
            Filter1 = AFilter1;
            Filter2 = AFilter2;
            CutFillDesignID = ACutFillDesignID; // CutFillDesign = ACutFillDesign;
            //ReferenceVolumeType = AReferenceVolumeType;
            //ColourPalettes = AColourPalettes;
            //ICOptions = AICOptions;
            RepresentColor = ARepresentColor;
            RequestingRaptorNodeID = requestingRaptorNodeID;
        }

        SubGridTreeSubGridExistenceBitMask OverallExistenceMap = null;

        RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

        BoundingWorldExtent3D RotatedTileBoundingExtents;

        bool SurveyedSurfacesExludedViaTimeFiltering = true;

        /* TODO
         private TICDisplayPaletteBaseClass ColourPaletteClassType()
            {
            case FMode of
              icdmHeight                  : Result  = TICDisplayPalette_Height;
              icdmCCV                     : Result  = TICDisplayPalette_CCV;
              icdmCCVPercent              : Result  = TICDisplayPalette_CCVPercent;
              icdmLatency                 : Result  = TICDisplayPalette_RadioLatency;
              icdmPassCount               : Result  = TICDisplayPalette_PassCount;
              icdmPassCountSummary        : Result  = TICDisplayPalette_PassCountSummary;  // Palettes are fixed three colour palettes - displayer will use direct transitions
              icdmRMV                     : Result  = TICDisplayPalette_RMV;
              icdmFrequency               : Result  = TICDisplayPalette_Frequency;
              icdmAmplitude               : Result  = TICDisplayPalette_Amplitude;
              icdmCutFill                 : Result  = TICDisplayPalette_CutFill;
              icdmMoisture                : Result  = TICDisplayPalette_Moisture;
              icdmTemperatureSummary      : Result  = TICDisplayPaletteBase; //TICDisplayPalette_Temperature;
              icdmGPSMode                 : Result  = TICDisplayPaletteBase; //TICDisplayPalette_GPSMode;
              icdmCCVSummary              : Result  = TICDisplayPaletteBase; //TICDisplayPalette_CCVSummary;
              icdmCCVPercentSummary       : Result  = TICDisplayPalette_CCVPercent;
              icdmCompactionCoverage      : Result  = TICDisplayPalette_CoverageOverlay;
              icdmVolumeCoverage          : Result  = TICDisplayPalette_VolumeOverlay;
              icdmMDP                     : Result  = TICDisplayPalette_MDP; // ajr15167
              icdmMDPSummary              : Result  = TICDisplayPaletteBase;
              icdmMDPPercent              : Result  = TICDisplayPalette_MDPPercent;
              icdmMDPPercentSummary       : Result  = TICDisplayPalette_MDPPercent;
              icdmMachineSpeed            : Result  = TICDisplayPalette_MachineSpeed;
              icdmCCVPercentChange        : Result  = TICDisplayPalette_CCVPercent;
              icdmTargetThicknessSummary  : Result  = TICDisplayPalette_VolumeOverlay;
              icdmTargetSpeedSummary      : Result  = TICDisplayPalette_SpeedSummary;
              icdmCCVChange               : Result  = TICDisplayPalette_CCVChange;
              icdmCCA                     : Result  = TICDisplayPalette_CCA;
              icdmCCASummary              : Result  = TICDisplayPalette_CCASummary;

            else
              SIGLogMessage.PublishNoODS(Self, Format('ColourPaletteClassType: Unknown display type: %d', [Ord(FMode)]), slmcAssert);
              Result  = TICDisplayPaletteBase;
            end;
          end;
        */

        /* TODO
          function ComputeCCAPalette :Boolean;
          var
            I, J, K               :Integer;
            ResponseVerb        :TRPCVerbBase;
            ServerResult        :TICServerRequestResult;
            ResponseDataStream  :TStream;
            CCAMinimumPasses    :TICCCAMinPassesValue;
            CCAColorScale       :TCCAColorScale;
            CCAPalette          :TColourPalettes;

          begin
            Result  = False;

            ResponseVerb  = nil;
            try
              if Length(FFilter1.Machines) > 0 then
                FMachineID  = FFilter1.Machines[0].ID // Must be set by caller
              else
                FMachineID  = -1; // will fail call
              if not Assigned(ASNodeImplInstance) or ASNodeImplInstance.ServiceStopped then
                begin
                  SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), slmcWarning);
                  Exit;
                end;

              ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetMachineCCAMinimumPassesValue(FDataModelID, FMachineID, FFilter1.StartTime, FFilter1.EndTime, FFilter1.LayerID, ResponseVerb);
              if Assigned(ResponseVerb) then
                with ResponseVerb as TVLPDRPCVerb_SendResponse do
                  begin
                    ServerResult  = TICServerRequestResult(ResponseCode);
                ResponseDataStream  = ResponseData;
                    if (ServerResult = icsrrNoError) and assigned(ResponseData) then
                      begin
                        CCAMinimumPasses  = ReadSmallIntFromStream(ResponseDataStream);

                Result  = CCAMinimumPasses > 0;

                        if not Result then
                          Exit;

                CCAColorScale  = TCCAColorScaleManager.CreateCoverageScale(CCAMinimumPasses);
                        try
                          SetLength(CCAPalette.Transitions, CCAColorScale.TotalColors);

                J  = Low(CCAPalette.Transitions);
                k  = High(CCAPalette.Transitions);
                          for I  = J to K do
                            begin
                              CCAPalette.Transitions[I].Colour  = CCAColorScale.ColorSegments[K - I].Color;
                CCAPalette.Transitions[I].Value   = I+1;
                            end;
                          CCAPalette.ConvertRGBToBGR; // gets done again but needed to match Anatoli palette test :)
                          WorkingColourPalette.PopulateFromPaletteColours(CCAPalette);
                          WorkingColourPalette.TransitionColours.ValuesCount  = Length(CCAPalette.Transitions);
                        finally
                          if Assigned(CCAColorScale) then
                            FreeAndNil(CCAColorScale);
                end;
                      end
                    else
                      SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: GetMachineCCAMinimumPassesValue Failed for MachineID: %d. ReturnCode:%d', [Self.ClassName, FMachineID, Ord(ServerResult)]), slmcWarning);
                  end;
            finally
              if Assigned(ResponseVerb) then
                FreeAndNil(ResponseVerb);
                end;
          end;
        */

        /* TODO
         function CreateAndInitialiseWorkingColourPalette :Boolean;
         begin
           Result  = True;

           // Create a scaled palette to use when rendering the data
           try
             if ColourPaletteClassType<> Nil then
              begin

                 WorkingColourPalette  = ColourPaletteClassType.Create;
                 WorkingColourPalette.SmoothPalette  = FMode = icdmCutFill;

                 // CCASummary is done per machineid
                 if FMode in [icdmCCA, icdmCCASummary]
               then
                   Result  = ComputeCCAPalette
                 else
                   begin
                     if Length(FColourPalettes.Transitions) = 0 then
                       WorkingColourPalette.SetToDefaults
                     else
                       WorkingColourPalette.PopulateFromPaletteColours(FColourPalettes);
               end;

                 if Result then
                   WorkingColourPalette.ComputePalette;
               end
             else
               WorkingColourPalette  = Nil;

           Except
             On e:Exception do
               SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Error: %s ', [Self.ClassName, e.Message]), slmcException);
           end;
         end;
       */

        /// <summary>
        /// Renders all subgrids in a representational style that indicates where there is data, but nothing else. This is used for large scale displays
        /// (zoomed out a lot) where meaningful detail cannot be drawn on the tile
        /// </summary>
        /// <returns></returns>
        private Bitmap RenderTileAsRepresentationalDueToScale()
        {
            PVMDisplayerBase RepresentationalDisplayer = PVMDisplayerFactory.GetDisplayer(Mode /*, FICOptions*/);

            RepresentationalDisplayer.MapView = new MapSurface
            {
                SquareAspect = false,
                Rotation = (-TileRotation + (Math.PI / 2)),
            };

            RepresentationalDisplayer.MapView.SetBounds(NPixelsX, NPixelsY);
            RepresentationalDisplayer.MapView.SetWorldBounds(NEECoords[0].X, NEECoords[0].Y,
                                                             NEECoords[0].X + WorldTileWidth, NEECoords[0].Y + WorldTileHeight,
                                                             0);

            // Iterate over all the bits in the subgrids drawing a rectangle for each one on the tile being rendered
            if (OverallExistenceMap.ScanSubGrids(RotatedTileBoundingExtents,
                                                 leaf =>
                                                 {
                                                     leaf.CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);

                                                     (leaf as SubGridTreeLeafBitmapSubGrid).Bits.ForEachSetBit((x, y) =>
                                                     {
                                                         RepresentationalDisplayer.MapView.DrawRect(WorldOriginX + (x * OverallExistenceMap.CellSize),
                                                                                                    WorldOriginY + (y * OverallExistenceMap.CellSize),
                                                                                                    OverallExistenceMap.CellSize, OverallExistenceMap.CellSize,
                                                                                                    true, RepresentColor);
                                                     });

                                                     return true;
                                                 }))
            {
                // PackageRenderedTileIntoPNG(RepresentationalDisplayer);
                return RepresentationalDisplayer.MapView.BitmapCanvas;
            }

            return null; // It did not work out...
        }

        /// <summary>
        /// Executor that implements requesting and rendering subgrid information to create the rendered tile
        /// </summary>
        /// <returns></returns>
        public Bitmap Execute()
        {
            BoundingWorldExtent3D SpatialExtents;
            // WorkingColourPalette  : TICDisplayPaletteBase;
            // CoordConversionResult : TCoordServiceErrorStatus;
            long RequestDescriptor;
            // bool ScheduledWithGovernor = false;
            SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap = null;
            SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap = null;
            long[] SurveyedSurfaceExclusionList = new long[0];

            double dx, dy;

            /*
               if not Assigned(ASNodeImplInstance) or ASNodeImplInstance.ServiceStopped then
                begin
                  SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), slmcWarning);
                  Exit;
                end;

            // TODO readd when logging available
            //SIGLogMessage.PublishNoODS(Self, Format('Performing %s.Execute for DataModel:%d, Mode=%d', [Self.ClassName, FDataModelID, Integer(FMode)]), slmcMessage);

            // TODO InterlockedIncrement64(ASNodeRequestStats.NumMapTileRequests);

           if Assigned(ASNodeImplInstance.RequestCancellations) and
              ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
             begin
               if VLPDSvcLocations.Debug_LogDebugRequestCancellationToFile then
                 SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, slmcDebug);

               ResultStatus  = asneRequestHasBeenCancelled;
               InterlockedIncrement64(ASNodeRequestStats.NumMapTileRequestsCancelled);
             Exit;
             end;

            // The governor is intended to restrict the numbers of heavy weight processes
            // such as pipelines that interact with the PC layer to request subgrids
            ScheduledWithGovernor  = ASNodeImplInstance.Governor.Schedule(FExternalDescriptor, Self, gqWMS, ResultStatus);
            if not ScheduledWithGovernor then
              Exit;
            */

            RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

            /* TODO Readd wen logging available
          if VLPDSvcLocations.Debug_EmitTileRenderRequestParametersToLog then
            begin
              if FCoordsAreGrid then
                begin
                  if Assigned(FFilter2) then
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d, Filter2=%d, Design=''%s'', VolType=%d, Bound[BL/TR:X/Y]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID, FFilter2.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon, FBLPoint.Lat, FTRPoint.Lon, FTRPoint.Lat,
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage)
                  else
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d Design=''%s'', VolType=%d, Bound[BL/TR:X/Y]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon, FBLPoint.Lat, FTRPoint.Lon, FTRPoint.Lat,
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage);
                end
              else
                begin
                  if Assigned(FFilter2) then
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d, Filter2=%d, Design=''%s'', VolType=%d, Bound[BL/TR:Lon/Lat]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID, FFilter2.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon * (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage)
                  else
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d, Design=''%s'', VolType=%d, Bound[BL/TR:Lon/Lat]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon * (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage);
                end;

              // Include the details of the filter with the logged tile parameters
              SIGLogMessage.PublishNoODS(Self, Format('Filter1: %s', [IfThen(FFilter1.IsNull, 'Null', FFilter1.ToString)]), slmcMessage);
              if Assigned(FFilter2) then
                SIGLogMessage.PublishNoODS(Self, Format('Filter2: %s', [IfThen(FFilter2.IsNull, 'Null', FFilter2.ToString)]), slmcMessage);
            end;
            */

            // Determine the grid (NEE) coordinates of the bottom/left, top/right WGS-84 positions
            // given the coordinate system assigned to the project. If there is no coordinate system
            // then this is where we exit

            LLHCoords = new XYZ[4]
            {
              new XYZ(BLPoint.X, BLPoint.Y),    // BL
              new XYZ(TRPoint.X, TRPoint.Y),    // TR
              new XYZ(BLPoint.X, TRPoint.Y),    // TL
              new XYZ(TRPoint.X, BLPoint.Y)     // BR
            };

            if (CoordsAreGrid)
            {
                NEECoords = LLHCoords;
            }
            else
            {
                /* TODO
                CoordConversionResult = ASNodeImplInstance.CoordService.RequestCoordinateConversion(RequestDescriptor, DataModelID, cctLLHtoNEE, LLHCoords, EmptyStr, NEECoords);
                if (CoordConversionResult != csOK)
                {
                    ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
                    return;
                }
                */
            }

            WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
            WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

            dx = NEECoords[2].X - NEECoords[0].X;
            dy = NEECoords[2].Y - NEECoords[0].Y;
            TileRotation = (Math.PI / 2) - Math.Atan2(dy, dx);

            RotatedTileBoundingExtents.SetInverted();
            foreach (XYZ xyz in NEECoords)
            {
                RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);
            }

            if (Filter1 != null && SurveyedSurfaceExclusionList.Length > 0)
            {
                SurveyedSurfaceExclusionList = new long[Filter1.AttributeFilter.SurveyedSurfaceExclusionList.Length];
                Array.Copy(Filter1.AttributeFilter.SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList.Length);
            }

            // Get the SiteModel for the request
            SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(DataModelID);
            if (SiteModel == null)
            {
                throw new ArgumentException(String.Format("Unable to acquire site model instance for ID:{0}", DataModelID));
            }

            SpatialExtents = SiteModel.GetAdjustedDataModelSpatialExtents(SurveyedSurfaceExclusionList);

            if (!SpatialExtents.IsValidPlanExtent)
            {
                ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // TODO: Or there was no date in the model
                return null;
            }

            // Get the current production data existance map from the sitemodel
            ProdDataExistenceMap = SiteModel.GetProductionDataExistanceMap();

            if (ProdDataExistenceMap == null)
            {
                ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
                return null;
            }

            // Temporarily remove tile pruning on intersection test
            // Intersect the site model extents with the extents requested by the caller
            /*if (!SpatialExtents.Intersect(RotatedTileBoundingExtents).IsValidPlanExtent)
            {
                ResultStatus = RequestErrorStatus.InvalidCoordinateRange;

                // RenderTransparentTile();
                return new Bitmap(NPixelsX, NPixelsY);
            }*/

            // Obtain the subgrid existence map for the project
            // Retrieve the existence map for the datamodel
            OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask()
            {
                CellSize = SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize
            };

            if (Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode))
            {
                // Obtain local reference to surveyed surfaces (lock free access)
                SurveyedSurfaces LocalSurveyedSurfaces = SiteModel.SurveyedSurfaces;
                SurveyedSurfaces Filter1SurveyedSurfaces = new SurveyedSurfaces();
                SurveyedSurfaces Filter2SurveyedSurfaces = new SurveyedSurfaces();

                if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(DataModelID, LocalSurveyedSurfaces, Filter1, Filter2SurveyedSurfaces, Filter1SurveyedSurfaces, OverallExistenceMap))
                {
                    ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
                    return null;
                };

                if (Filter2 != null)
                {
                    if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(DataModelID, LocalSurveyedSurfaces, Filter2, Filter1SurveyedSurfaces, Filter2SurveyedSurfaces, OverallExistenceMap))
                    {
                        ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
                        return null;
                    }
                }

                SurveyedSurfacesExludedViaTimeFiltering = !(Filter1SurveyedSurfaces?.Count > 0) || (Filter2SurveyedSurfaces?.Count > 0);
            }

            OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

            if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel.ID, Filter1, OverallExistenceMap))
            {
                ResultStatus = RequestErrorStatus.NoDesignProvided;
                return null;
            }


            // Test to see if the tile can be satisfied with a representational render indicating where
            // data is but not what it is (this is useful when the zoom level is far enough away that we
            // cannot meaningfully render the data). If the size of s subgrid is smaller than
            // the size of a pixel in the requested tile then do this. Just check the X dimension
            // as the data display is isotropic.
            if (Rendering.Utilities.SubgridShouldBeRenderedAsRepresentationalDueToScale(WorldTileWidth, WorldTileHeight, NPixelsX, NPixelsY, OverallExistenceMap.CellSize))
            {
                return RenderTileAsRepresentationalDueToScale(); // There is no need to do anything else
            }

            if (Filter1?.AttributeFilter.AnyFilterSelections == true)
            {
                if (Filter1.AttributeFilter.OverrideTimeBoundary && Filter1.AttributeFilter.EndTime == DateTime.MinValue)
                {
                    if (Filter2?.AttributeFilter.AnyFilterSelections == true)
                    {
                        // fix SV bug. Setup Filter 1 to look for early cell pass
                        Filter1.AttributeFilter.StartTime = DateTime.MinValue;
                        Filter1.AttributeFilter.EndTime = Filter2.AttributeFilter.StartTime;
                    }
                }

                ResultStatus = FilterUtilities.PrepareFilterForUse(Filter1, DataModelID);
                if (ResultStatus != RequestErrorStatus.OK)
                {
                    return null;
                }
            }

            if (Filter2 != null && !Filter2.AttributeFilter.AnyFilterSelections)
            {
                ResultStatus = FilterUtilities.PrepareFilterForUse(Filter2, DataModelID);
                if (ResultStatus != RequestErrorStatus.OK)
                {
                    return null;
                }
            }

            /* TODO Readd when logging available
               {$IFDEF DEBUG}
                SIGLogMessage.PublishNoODS(Self,
                                          Format('(%d) Tile render executing across tile: [Rotation:%.3fdeg] [BL:%.3f, %.3f, TL:%.3f, %.3f, TR:%.3f, %.3f, BR:%.3f, %.3f](%.3f x %.3f), Lat/Lon = [%.8f, %.8f, %.8f, %.8f](%.8f x %.8f)',
                                                 [FDataModelID,
                                                  TileRotation * (180/PI),
                                                  NEECoords[0].X, NEECoords[0].Y,
                                                  NEECoords[2].X, NEECoords[2].Y,
                                                  NEECoords[1].X, NEECoords[1].Y,
                                                  NEECoords[3].X, NEECoords[3].Y,
                                                  NEECoords[1].X - NEECoords[0].X, NEECoords[1].Y - NEECoords[0].Y,
                                                  FBLPoint.Lon* (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                  (FTRPoint.Lon - FBLPoint.Lon) * (180/PI), (FTRPoint.Lat - FBLPoint.Lat) * (180/PI)]),
                                          slmcDebug);
               {$ENDIF}
            */

            // If this render involves a relationship with a design then ensure the existance map
            // for the design is loaded in to memory to allow the request pipeline to confine
            // subgrid requests that overlay the actual design
            if (Rendering.Utilities.RequestRequiresAccessToDesignFileExistanceMap(Mode /*, ReferenceVolumeType*/))
            {
                /*if (CutFillDesign.IsNull)
                {
                    // TODO readd when logging available
                    //SIGLogMessage.PublishNoODS(Self, Format('No design provided to cut fill, summary volume or thickness overlay render request for datamodel %d', [FDataModelID]), slmcError);
                    ResultStatus = RequestErrorStatus.NoDesignProvided;
                    return null;
                }*/

                DesignSubgridOverlayMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(DataModelID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, CutFillDesignID);

                if (DesignSubgridOverlayMap == null)
                {
                    // SIGLogMessage.PublishNoODS(Self, Format('Failed to request subgrid overlay index for design %s in datamodel %d", [FDesignDescriptor.ToString, FDataModelID]), slmcError);
                    ResultStatus = RequestErrorStatus.NoDesignProvided;
                    return null;
                }

                DesignSubgridOverlayMap.CellSize = SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize;
            }

            // Construct the renderer, configure it, and set it on its way
            //  WorkingColourPalette = Nil;
            Renderer = new PlanViewTileRenderer(RequestingRaptorNodeID);
            try
            {
                /* TODO 
                // Create a scaled palette to use when rendering the data
                if not CreateAndInitialiseWorkingColourPalette then
                  begin
                  SIGLogMessage.PublishNoODS(Self, Format('Failed to create and initialise working colour palette for data: %s in datamodel %d', [TypInfo.GetEnumName(TypeInfo(TICDisplayMode), Ord(FMode)), FDataModelID]), slmcWarning);
                  Exit;
                  end;
                */

                ////////////////////// CONFIGURE THE RENDERER - START ///////////////////////////////

                Renderer.DataModelID = DataModelID;
                Renderer.RequestDescriptor = RequestDescriptor;
                // Renderer.ExternalDescriptor  = FExternalDescriptor;
                Renderer.SpatialExtents = SpatialExtents;
                Renderer.CellSize = SiteModel.Grid.CellSize;

                Renderer.Mode = Mode;
                // Renderer.ICOptions  = FICOptions;

                /* TODO Lift build settings not supported
                if (Filter1 != null && Filter1.AttributeFilter.HasLayerStateFilter || Filter1.AttributeFilter.LayerMethod != TFilterLayerMethod.flmNone)
                {
                    Renderer.LiftBuildSettings = ICOptions.GetLiftBuildSettings(Filter1.LayerMethod);
                }
                else
                {
                    Renderer.LiftBuildSettings = ICOptions.LiftBuildSettings;
                }
                */

                Renderer.OverallExistenceMap = OverallExistenceMap;
                Renderer.ProdDataExistenceMap = ProdDataExistenceMap;
                Renderer.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

                // Renderer.WorkingPalette = WorkingColourPalette;

                Renderer.Filter1 = Filter1;
                Renderer.Filter2 = Filter2;

                Renderer.IsWhollyInTermsOfGridProjection = true; // Ensure the renderer knows we are using grid projection coordinates
                Renderer.SetBounds(NEECoords[0].X, NEECoords[0].Y,
                                   WorldTileWidth, WorldTileHeight,
                                   NPixelsX, NPixelsY);

                Renderer.CutFillDesignID = CutFillDesignID;
                //Renderer.CutFillDesign = CutFillDesign;

                // Renderer.ReferenceVolumeType = FReferenceVolumeType;

                Renderer.TileRotation = TileRotation;
                Renderer.RotatedTileBoundingExtents = RotatedTileBoundingExtents;

                Renderer.WorldTileWidth = WorldTileWidth;
                Renderer.WorldTileHeight = WorldTileHeight;

                Renderer.SurveyedSurfacesExludedViaTimeFiltering = SurveyedSurfacesExludedViaTimeFiltering;

                ////////////////////// CONFIGURE THE RENDERER - END  ///////////////////////////////////////

                ResultStatus = Renderer.PerformRender();

                if (ResultStatus == RequestErrorStatus.OK)
                {
                    /*
                    // Draw diagonal cross and top left corner indicators
                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, BLPoint.Y, TRPoint.X, TRPoint.Y, Color.Red);
                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, TRPoint.Y, TRPoint.X, BLPoint.Y, Color.Red);

                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, TRPoint.Y, BLPoint.X, TRPoint.Y - 50, Color.Red);
                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, TRPoint.Y, BLPoint.X + 50, TRPoint.Y - 0.1, Color.Red);
                    */

                    return Renderer.Displayer.MapView.BitmapCanvas;

                    // PackageRenderedTileIntoPNG(Renderer.Displayer);

                    // TODO - report the final rendered result back
                }
            }
            catch
            {
                // TODO add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Exception "%s" occurred', [Self.ClassName, E.Message]), slmcException);
                ResultStatus = RequestErrorStatus.Exception;
            }

            return null;
        }
    }
}
