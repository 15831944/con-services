﻿using Draw = System.Drawing;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Palettes
{
    // A basic palette class that defines a set of transitions covering a value range being rendered
    public class PaletteBase : IPlanViewPalette
    {
        public PaletteBase(Transition[] transitions)
        {
            PaletteTransitions = transitions;
        }

        // The set of transition value/colour pairs defining a render-able value range
        public Transition[] PaletteTransitions { get; set;}

        /// <summary>
        /// Logic to choose a colour from the set of transitions depending on the value. Slow but simple for the POC...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Draw.Color ChooseColour(double value)
        {
            for (int i = PaletteTransitions.Length - 1; i >= 0; i--)
            {
                if (value >= PaletteTransitions[i].Value)
                {
                    return PaletteTransitions[i].Color;
                }
            }

            return Draw.Color.Empty;
        }
    }
}
