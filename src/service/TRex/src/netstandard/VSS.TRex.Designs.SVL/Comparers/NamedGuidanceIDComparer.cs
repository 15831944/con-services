﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Designs.SVL.Comparers
{
  public class NamedGuidanceIDComparer : IComparer<NFFNamedGuidanceID>
  {
    public int Compare(NFFNamedGuidanceID x, NFFNamedGuidanceID y)
    {
      double CalcNamedGuidanceIDCompareOffset(NFFNamedGuidanceID NamedGuidanceID)
      {
        const double BatterAlignmentOffset  = 1E10;
        const double DitchAlignmentOffset = 1E9;
        const double HingeAlignmentOffset = 1E8;

        // Return a modified Offset value for a NamedGuidanceID, fudged for NamedGuidanceIDs
        // that are flagged as being Hinge, Ditch or Batter.
        switch (NamedGuidanceID.GuidanceAlignmentType)
        {
          case NFFGuidanceAlignmentType.gtMasterAlignment:
         case NFFGuidanceAlignmentType.gtSubAlignment:
            return NamedGuidanceID.StartOffset;
         case NFFGuidanceAlignmentType.gtHinge:
            return HingeAlignmentOffset * Math.Sign(NamedGuidanceID.StartOffset);
         case NFFGuidanceAlignmentType.gtDitch:
            return DitchAlignmentOffset * Math.Sign(NamedGuidanceID.StartOffset);
         case NFFGuidanceAlignmentType.gtBatter:
            return BatterAlignmentOffset * Math.Sign(NamedGuidanceID.StartOffset);
          default:
            throw new TRexException("Unknown guidance alignment type");
        }
      }

      Debug.Assert(x.StartOffset != Consts.NullDouble && y.StartOffset != Consts.NullDouble);

      return CalcNamedGuidanceIDCompareOffset(x).CompareTo(CalcNamedGuidanceIDCompareOffset(y));
    }
  }
}
