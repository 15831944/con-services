﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering raw CMV data
    /// </summary>
    public class CMVPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(0, Color.Green),
            new Transition(20, Color.Yellow),
            new Transition(40, Color.Olive),
            new Transition(60, Color.Blue),
            new Transition(100, Color.SkyBlue)
        };

        public CMVPalette() : base(Transitions)
        {
        }
    }
}
