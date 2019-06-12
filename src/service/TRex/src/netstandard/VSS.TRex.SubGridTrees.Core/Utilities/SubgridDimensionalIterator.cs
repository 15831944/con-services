﻿using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core.Utilities
{
  public static partial class SubGridUtilities
  {
    /// <summary>
    /// Iterates across the dimensional extent of a sub grid (ie: the [0..Dimension, 0..Dimension] indices) calling
    /// the supplied action functor.
    /// </summary>
    /// <param name="functor"></param>
    public static void SubGridDimensionalIterator(Action<int, int> functor)
    {
      for (int I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
      {
        for (int J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
        {
          functor(I, J);
        }
      }
    }

    /// <summary>
    /// Iterates across the dimensional extent of a sub grid (ie: the [0..Dimension, 0..Dimension] indices) calling
    /// the supplied action functor.
    /// </summary>
    /// <param name="functor"></param>
    public static bool SubGridDimensionalIterator(Func<int, int, bool> functor)
    {
      for (int I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
      {
        for (int J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
        {
          if (!functor(I, J))
            return false;
        }
      }

      return true;
    }
  }
}
