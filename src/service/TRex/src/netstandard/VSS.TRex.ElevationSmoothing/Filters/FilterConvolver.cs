﻿using System;

namespace VSS.TRex.DataSmoothing
{
  /// <summary>
  /// Implements a classical base convolver that accepts a filter matrix and applies it to the data set
  /// to be convoled.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class FilterConvolver<T> : ConvolverBase<T>
  {
    public readonly double[,] FilterMatrix;

    public FilterConvolver(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix, NullInfillMode nullInfillMode) : base(accumulator)
    {
      _infillNullValuesOnly = nullInfillMode == NullInfillMode.InfillNullValuesOnly;
      _updateNullValues = nullInfillMode == NullInfillMode.InfillNullValuesOnly || _infillNullValuesOnly;

      FilterMatrix = filterMatrix;

      var majorDim = FilterMatrix.GetLength(0);
      var minorDim = FilterMatrix.GetLength(1);

      if (majorDim != minorDim)
      {
        throw new ArgumentException($"Major dimension ({majorDim}) and minor dimension ({minorDim}) of filterMatrix must be the same");
      }

      ContextSize = (ConvolutionMaskSize)majorDim;
    }

    /// <summary>
    /// Performs convolution on a single element in the data set
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    public override void ConvolveElement(int i, int j)
    {
      _accumulator.Clear();
      _accumulator.ConvolutionSourceValue = GetValue(i, j);

      var convolutionSourceValueIsNull = _accumulator.ConvolutionSourceValueIsNull();

      if (!_updateNullValues && convolutionSourceValueIsNull ||
          _infillNullValuesOnly && !convolutionSourceValueIsNull)
      {
        // There is no change to be done to this cell...
        SetValue(i, j, _accumulator.ConvolutionSourceValue);
        return;
      }

      for (int x = i - _contextOffset, limitx = i + _contextOffset, majorIndex = 0; x <= limitx; x++, majorIndex++)
      {
        for (int y = j - _contextOffset, limity = j + _contextOffset, minorIndex = 0; y <= limity; y++, minorIndex++)
        {
          _accumulator.Accumulate(GetValue(x, y), FilterMatrix[majorIndex, minorIndex]);
        }
      }

      if (_updateNullValues && convolutionSourceValueIsNull)
      {
        SetValue(i, j, _accumulator.NullInfillResult());
      }
      else
      {
        SetValue(i, j, _accumulator.Result());
      }
    }
  }
}
