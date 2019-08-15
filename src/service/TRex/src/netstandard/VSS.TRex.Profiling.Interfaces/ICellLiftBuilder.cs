﻿using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellLiftBuilder
  {
    /// <summary>
    /// The count of filtered call passes used to construct the top most (latest) layer
    /// </summary>
    int FilteredPassCountOfTopMostLayer { get; set; }

    // FilteredHalfCellPassCountOfTopMostLayer tracks 'half cell passes'.
    // A half cell pass is recorded when a Quattro four drum compactor drives over the ground.
    // Other machines, like single drum compactors, record two half cell pass counts to form a single cell pass.
    int FilteredHalfCellPassCountOfTopMostLayer { get; set; }

    bool Build(IProfileCell cell,
      ILiftParameters liftParameters,
      IClientLeafSubGrid ClientGrid,
      FilteredValueAssignmentContext AssignmentContext,
      ISubGridSegmentCellPassIterator cellPassIterator,
      bool returnIndividualFilteredValueSelection);
  }
}
