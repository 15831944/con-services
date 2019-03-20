﻿using System;
using System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Rendering.Palettes
{
    public class HeightPalette : IPlanViewPalette
    {
        private double MinElevation;// = Consts.NullDouble;
        private double MaxElevation;// = Consts.NullDouble;
        private double ElevationPerBand;// = Consts.NullDouble;

        private static readonly Color[] ElevationPalette = 
        {
        Color.Aqua,
        Color.Yellow,
        Color.Fuchsia,
        Color.Lime,
        Color.FromArgb(0x80, 0x80, 0xFF),
        Color.LightGray,  
        Color.FromArgb(0xEB, 0xFD, 0xAC),
        Color.FromArgb(0xFF, 0x80, 0x00),
        Color.FromArgb(0xFF, 0xC0, 0xFF),
        Color.FromArgb(0x96, 0xCB, 0xFF),
        Color.FromArgb(0xB5, 0x8E, 0x6C),
        Color.FromArgb(0xFF, 0xFF, 0x80),
        Color.FromArgb(0xFF, 0x80, 0x80),
        Color.FromArgb(0x80, 0xFF, 0x00),
        Color.FromArgb(0x00, 0x80, 0xFF),
        Color.FromArgb(0xFF, 0x00, 0x80),
        Color.Teal,     
        Color.FromArgb(0xFF, 0xC0, 0xC0),
        Color.FromArgb(0xFF, 0x80, 0xFF),
        Color.FromArgb(0x00, 0xFF, 0x80)
        };
    
        public HeightPalette(double minElevation, double maxElevation)
        {
            MinElevation = minElevation;
            MaxElevation = maxElevation;
            ElevationPerBand = (MaxElevation - MinElevation) / ElevationPalette.Length;
        }

        public Color ChooseColour(double value)
        {
            var color = Color.Black;

            if (value != Consts.NullDouble)
            {
              int index = (int) Math.Floor((value - MinElevation) / ElevationPerBand);
              color = Range.InRange(index, 0, ElevationPalette.Length - 1) ? ElevationPalette[index] : Color.Black; // Color.Empty;
            }

            return color;
        }
    }
}
