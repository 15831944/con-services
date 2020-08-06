﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Contains the business logic to construct a vector of cells containing the information calculated for a profile along a vector of points
  /// </summary>
  public class CellProfileBuilder<T> : ICellProfileBuilder<T> where T : class, IProfileCellBase, new()
  {
    private static ILogger Log = Logging.Logger.CreateLogger<CellProfileBuilder<T>>();

    public const int kMaxHzVtGridInterceptsToCalculate = 8000;

    private ISiteModel SiteModel;
    private double CellSize;
    public bool Aborted { get; set; }

    private InterceptList VtIntercepts = new InterceptList();
    private InterceptList HzIntercepts = new InterceptList();
    private InterceptList VtHzIntercepts = new InterceptList();

    private bool SlicerToolUsed;
    private bool ReturnDesignElevation;
    private int OTGCellX, OTGCellY;

    private SubGridCellAddress CurrentSubgridOrigin;
    private SubGridCellAddress ThisSubgridOrigin;

    private SubGridTreeBitmapSubGridBits FilterMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    private double StartX, StartY, StartStation;
    private double EndX, EndY, EndStation;
    private double CurrStationPos;
    private double Distance;

    private IClientHeightLeafSubGrid DesignElevations;
    private DesignProfilerRequestResult DesignResult;

    private List<T> ProfileCells;
    private XYZ[] NEECoords;
    private ICellSpatialFilter CellFilter;
    private IDesignWrapper CutFillDesignWrapper;

    /// <summary>
    /// The design to be used as a TIN surface design based 'cookie cutter' selection mask for production data
    /// </summary>
    protected IDesign SurfaceDesignMaskDesign;

    /// <summary>
    /// Creates a CellProfile builder given a list of coordinates defining the path to profile and a container to place the resulting cells into
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="filterSet"></param>
    /// <param name="cutFillDesignWrapper"></param>
    /// <param name="slicerToolUsed"></param>
    public CellProfileBuilder(ISiteModel siteModel,
      IFilterSet filterSet,
      IDesignWrapper cutFillDesignWrapper,
      bool slicerToolUsed)
    {
      SiteModel = siteModel;
      CellFilter = filterSet?.Filters[0].SpatialFilter;
      CutFillDesignWrapper = cutFillDesignWrapper;
      SlicerToolUsed = slicerToolUsed;

      Initialise();
    }

    private void Initialise()
    {
      if (CellFilter != null && CellFilter.SurfaceDesignMaskDesignUid != Guid.Empty)
      {
        SurfaceDesignMaskDesign = SiteModel.Designs.Locate(CellFilter.SurfaceDesignMaskDesignUid);
        if (SurfaceDesignMaskDesign == null)
          throw new ArgumentException($"Design {CellFilter.SurfaceDesignMaskDesignUid} not found in project {SiteModel.ID}");
      }
    }

    /// <summary>
    /// Calculates all intercepts between the profile line and horizontal lines separating rows of cells
    /// </summary>
    /// <param name="startStation"></param>
    private void CalculateHorizontalIntercepts(double startStation)
    {
      // Find intersections of all horizontal grid rows
      int VGridLineStartIndex = (int) Math.Truncate(StartY / CellSize);
      int VGridLineEndIndex = (int) Math.Truncate(EndY / CellSize);
      int Increment = Math.Sign(VGridLineEndIndex - VGridLineStartIndex);

      // To find a match, tell the intersection matching method that each
      // horizontal grid line starts 'before' the StartX parameter and 'ends'
      // after the EndX parameter - use the CellSize as an arbitrary value for this.
      // This gets around an issue where with a perfectly vertical profile line,
      // no intersections were being determined.
      double HGridStartX = StartX;
      double HGridEndX = EndX;
      if (HGridStartX > HGridEndX)
        MinMax.Swap(ref HGridEndX, ref HGridStartX);

      HGridStartX = HGridStartX - CellSize;
      HGridEndX = HGridEndX + CellSize;

      int IntersectionCount = Math.Abs(VGridLineEndIndex - VGridLineStartIndex) + 1;

      while (IntersectionCount > 0 && HzIntercepts.Count < kMaxHzVtGridInterceptsToCalculate && !Aborted)
      {
        if (LineIntersection.LinesIntersect(StartX, StartY, EndX, EndY,
          HGridStartX, VGridLineStartIndex * CellSize,
          HGridEndX, VGridLineStartIndex * CellSize,
          out double IntersectX, out double IntersectY, true, out bool _))
        {
          HzIntercepts.AddPoint(IntersectX, IntersectY,
            startStation + MathUtilities.Hypot(StartX - IntersectX, StartY - IntersectY));
        }

        VGridLineStartIndex += Increment;
        IntersectionCount--;
      }
    }

    /// <summary>
    /// Calculates all intercepts between the profile line and vertical lines separating rows of cells
    /// </summary>
    /// <param name="startStation"></param>
    private void CalculateVerticalIntercepts(double startStation)
    {
      // Find intersections of all vertical grid columns
      int HGridLineStartIndex = (int) Math.Truncate(StartX / CellSize);
      int HGridLineEndIndex = (int) Math.Truncate(EndX / CellSize);
      int Increment = Math.Sign(HGridLineEndIndex - HGridLineStartIndex);

      // To find a match, tell the intersection matching method that each
      // vertical grid line starts 'before' the StartX parameter and 'ends'
      // after the EndX parameter - use the CellSize as an arbitrary value for this.
      // This gets around an issue where with a perfectly horizontal profile line,
      // no intersections were being determined.
      double VGridStartX = StartY;
      double VGridEndX = EndY;

      if (VGridStartX > VGridEndX)
        MinMax.Swap(ref VGridEndX, ref VGridStartX);

      VGridStartX -= CellSize;
      VGridEndX += CellSize;

      int IntersectionCount = Math.Abs(HGridLineEndIndex - HGridLineStartIndex) + 1;
      while (IntersectionCount > 0 && VtIntercepts.Count < kMaxHzVtGridInterceptsToCalculate && !Aborted)
      {
        if (LineIntersection.LinesIntersect(StartX, StartY, EndX, EndY,
          HGridLineStartIndex * CellSize, VGridStartX,
          HGridLineStartIndex * CellSize, VGridEndX,
          out double IntersectX, out double IntersectY, true, out bool _))
        {
          VtIntercepts.AddPoint(IntersectX, IntersectY,
            startStation + MathUtilities.Hypot(StartX - IntersectX, StartY - IntersectY));
        }

        HGridLineStartIndex += Increment;
        IntersectionCount--;
      }
    }

    /// <summary>
    /// Adds the data for a single cell into the list containing the result fof the profile cells
    /// </summary>
    /// <param name="OTGX"></param>
    /// <param name="OTGY"></param>
    /// <param name="Station"></param>
    /// <param name="InterceptLength"></param>
    private void AddCellPassesDataToList(int OTGX, int OTGY, double Station, double InterceptLength)
    {
      IProfileCellBase ProfileCell = new T
      {
        OTGCellX = OTGX,

       OTGCellY = OTGY,
        Station = Station,
        InterceptLength = InterceptLength
      };

      if (DesignElevations != null)
        ProfileCell.DesignElev = DesignElevations.Cells[OTGX & SubGridTreeConsts.SubGridLocalKeyMask, OTGY & SubGridTreeConsts.SubGridLocalKeyMask];
      else
        ProfileCell.DesignElev = Consts.NullHeight;

      ProfileCells.Add((T)ProfileCell);
    }

    /// <summary>
    /// Constructs a vector of cells in profileCells along the path of the profile geometry containing in nEECoords
    /// </summary>
    /// <param name="nEECoords"></param>
    /// <param name="profileCells"></param>
    /// <returns></returns>
    public async Task<bool> Build(XYZ[] nEECoords, List<T> profileCells)
    {
      NEECoords = nEECoords;
      ProfileCells = profileCells;

      CellSize = SiteModel.CellSize;

      CurrStationPos = 0;

      CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ReturnDesignElevation = CutFillDesignWrapper?.Design != null;
      DesignElevations = null;

      // Obtain the primary partition map to allow this request to determine the elements it needs to process
      bool[] primaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions();

      for (int loopBound = NEECoords.Length - 1, I = 0; I < loopBound; I++)
      {
        StartX = NEECoords[I].X;
        StartY = NEECoords[I].Y;
        StartStation = NEECoords[I].Z;

        EndX = NEECoords[I + 1].X;
        EndY = NEECoords[I + 1].Y;
        EndStation = NEECoords[I + 1].Z;

        if (I == 0) // Add start point of profile line to intercept list
        {
          CurrStationPos = SlicerToolUsed ? 0 : NEECoords[I].Z; // alignment profiles pass in station for more accuracy
          VtHzIntercepts.AddPoint(StartX, StartY, CurrStationPos);
        }

        Distance = SlicerToolUsed
          ? MathUtilities.Hypot(EndX - StartX, EndY - StartY) // station is not passed so compute
          : EndStation - StartStation; // use precise station passed

        if (Distance == 0) // if we have two points the same
          continue;

        // Get all intercepts between profile line and cell boundaries for this segment
        CalculateHorizontalIntercepts(CurrStationPos); // pass the distance down alignment this segment starts
        CalculateVerticalIntercepts(CurrStationPos);

        CurrStationPos += Distance; // add distance to current station
      }

      // Merge vertical and horizontal cell boundary/profile line intercepts
      VtHzIntercepts.MergeInterceptLists(VtIntercepts, HzIntercepts);

      // Add end point of profile line to intercept list
      VtHzIntercepts.AddPoint(EndX, EndY, CurrStationPos);

      // Update each intercept with it's midpoint and intercept length
      // i.e. the midpoint on the line between one intercept and the next one
      // and the length between those intercepts
      VtHzIntercepts.UpdateMergedListInterceptMidPoints();

      if (VtHzIntercepts.Count > ProfileCells.Capacity)
        ProfileCells.Capacity = VtHzIntercepts.Count;

      // Iterate over all intercepts calculating the results for each cell that lies in
      // a subgrid handled by this node
      for (int i = 0; i < VtHzIntercepts.Count; i++)
      {
        if (Aborted)
          return false;

        // Determine the on-the-ground cell underneath the midpoint of each intercept line
        SiteModel.Grid.CalculateIndexOfCellContainingPosition(VtHzIntercepts.Items[i].MidPointX,
          VtHzIntercepts.Items[i].MidPointY, out OTGCellX, out OTGCellY);

        ThisSubgridOrigin = new SubGridCellAddress(OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(ThisSubgridOrigin))
        {
          if (!primaryPartitionMap[ThisSubgridOrigin.ToSpatialPartitionDescriptor()])
            continue;

          CurrentSubgridOrigin = ThisSubgridOrigin;

          if (!await ProfileFilterMask.ConstructSubgridCellFilterMask(CurrentSubgridOrigin, VtHzIntercepts, i, FilterMask, CellFilter, SiteModel.Grid, 
            SurfaceDesignMaskDesign))
            continue;

          if (ReturnDesignElevation && CutFillDesignWrapper?.Design != null) // cut fill profile request then get elevation at same spot along design
          {
            var getDesignHeightsResult = await CutFillDesignWrapper.Design.GetDesignHeights(SiteModel.ID, CutFillDesignWrapper.Offset, new SubGridCellAddress(OTGCellX, OTGCellY), CellSize);
            
            DesignElevations = getDesignHeightsResult.designHeights;
            DesignResult = getDesignHeightsResult.errorCode;

            if (DesignResult != DesignProfilerRequestResult.OK &&
                DesignResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              continue;

            if (DesignResult == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              DesignElevations = null; // force a null height to be written
          }
        }

        if (FilterMask.BitSet(OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask, OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask))
          AddCellPassesDataToList(OTGCellX, OTGCellY, VtHzIntercepts.Items[i].ProfileItemIndex, VtHzIntercepts.Items[i].InterceptLength);
      }

      Log.LogInformation($"CellProfileBuilder constructed a vector of {VtHzIntercepts.Count} vertices");

      return true;
    }
  }
}
