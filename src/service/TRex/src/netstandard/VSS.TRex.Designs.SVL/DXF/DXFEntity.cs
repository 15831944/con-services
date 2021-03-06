﻿using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.DXF
{
  public abstract class DXFEntity
  {
    public string Layer { get; set; }
    public int Colour { get; set; }

    public DXFEntity(string layer, int colour)
    {
      Layer = layer;
      Colour = colour;
    }

    public abstract DXFEntityTypes EntityType();

    public virtual void SaveToFile(StreamWriter writer, DistanceUnitsType OutputUnits)
    {
      DXFUtils.WriteDXFRecord(writer, DXFConsts.LayerNameId, DXFUtils.DXFiseLayerName(Layer));
      DXFUtils.WriteDXFRecord(writer, DXFConsts.ColourId, Colour.ToString());
    }
//    procedure CalculateExtents(var EMinX, EMinY, EMinZ, EMaxX, EMaxY, EMaxZ : Double); Virtual; Abstract;
//    Procedure ConvertTo2D; Virtual; Abstract;

    public abstract bool Is3D();

    public virtual double GetInitialHeight() => Consts.NullDouble;
  }
}
