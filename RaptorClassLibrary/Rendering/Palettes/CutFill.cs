﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering speed data
    /// </summary>
    public class CutFill : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(-2.0, Color.Red),
            new Transition(-1.0, Color.Yellow),
            new Transition(-0.1, Color.Green),
            new Transition(0.1, Color.Blue),
            new Transition(1.0, Color.SkyBlue),
            new Transition(1.0, Color.DarkBlue)
        };

        public CutFill() : base(Transitions)
        {
        }
    }
}
