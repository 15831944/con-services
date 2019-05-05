﻿using System.Drawing;

namespace VSS.TRex.Rendering.Palettes.Interfaces
{
    /// <summary>
    /// IPlanViewPalette defines the responsibility of deriving a colour value from a datum supplied to it
    /// </summary>
    public interface IPlanViewPalette
    {
    /// <summary>
    /// Returns a Color derived from the given value datum
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Color ChooseColour(double value);
    }
}
