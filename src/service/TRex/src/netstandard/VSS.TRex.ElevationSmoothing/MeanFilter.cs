﻿using System;

namespace VSS.TRex.ElevationSmoothing
{
  public class MeanFilter<T> : FilterConvolver<T>
  {
    protected static double[,] CreateFilter(int contextSize, int centerWeight)
    {
      if (contextSize < 3 || contextSize > 11)
      {
        throw new ArgumentException($"Context size of {contextSize} is out of range: 3..11");
      }

      var totalWeight = (contextSize * contextSize) - 1 + centerWeight;

      var result = new double[contextSize, contextSize];
      for (var i = 0; i < contextSize; i++)
      {
        for (var j = 0; j < contextSize; j++)
        {
          result[i, j] = 1.0d / totalWeight;
        }
      }

      result[contextSize / 2, contextSize / 2] = centerWeight;

      return result;
    }

    public MeanFilter(IConvolutionAccumulator<T> accumulator, int contextSize) : base(accumulator, CreateFilter(contextSize, 1))
    {
    }

    public MeanFilter(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix) : base(accumulator, filterMatrix)
    {
    }
  }
}
