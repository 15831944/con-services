﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs
{
  public abstract class DesignBase : IDesignBase
  {
    private int _lockCount;

    public Guid DesignUid { get; set; }

    public int LockCount => _lockCount;

    public string FileName { get; set; } = "";

    public Guid ProjectUid { get; set; }

    protected DesignBase()
    {
    }

    /// <summary>
    /// Indicates if the design exists but is in the process of being loaded. This is used to control
    /// multiple concurrent requests to a design that is not yet loaded.
    /// </summary>
    public bool IsLoading { get; set; } = false;

    public abstract DesignLoadResult LoadFromFile(string fileName, bool saveIndexFiles = true);

    public abstract Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath,
      bool loadIndices = false);

    public abstract void GetExtents(out double x1, out double y1, out double x2, out double y2);

    public abstract BoundingWorldExtent3D GetExtents();

    public abstract void GetHeightRange(out double z1, out double z2);

    public abstract bool InterpolateHeight(ref int hint,
      double x, double y,
      double offset,
      out double z);

    public abstract bool InterpolateHeights(float[,] patch, // The receiver of the patch of elevations
      double originX, double originY,
      double cellSize,
      double offset);

    // ComputeFilterPatch computes a bit set representing which cells in the
    // sub grid will be selected within the filter (i.e. the design forms a mask
    // over the production data where the cells 'under' the design are considered
    // to be in the filtered set. The mask parameter allows the caller to restrict
    // the set of cells in the sub grid to be filtered, allowing additional spatial
    // filtering operations to be applied prior to this filtering step.
    public abstract bool ComputeFilterPatch(double startStn, double endStn, double leftOffset, double rightOffset,
      SubGridTreeBitmapSubGridBits mask,
      SubGridTreeBitmapSubGridBits patch,
      double originX, double originY,
      double cellSize,
      double offset);

    public void WindLock() => Interlocked.Increment(ref _lockCount);

    public void UnWindLock() => Interlocked.Decrement(ref _lockCount);

    public bool IsStale { get; set; }

    public bool Locked => _lockCount > 0;

    public abstract bool HasElevationDataForSubGridPatch(double x, double y);

    public abstract bool HasElevationDataForSubGridPatch(int subGridX, int subGridY);

    public abstract bool HasFiltrationDataForSubGridPatch(double x, double y);

    public abstract bool HasFiltrationDataForSubGridPatch(int subGridX, int subGridY);

    public virtual ISubGridTreeBitMask SubGridOverlayIndex() => null;

    public void AcquireExclusiveInterlock() => Monitor.Enter(this);
    public void ReleaseExclusiveInterlock() => Monitor.Exit(this);

    public abstract List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize);

    public abstract List<Fence> GetBoundary();

    public abstract bool RemoveFromStorage(Guid siteModelUid, string fileName);


    public abstract long SizeInCache();

    public abstract void Dispose();
  }
}
