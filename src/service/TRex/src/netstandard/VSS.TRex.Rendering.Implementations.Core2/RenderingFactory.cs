﻿using VSS.Productivity3D.Models.Extensions;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class RenderingFactory : IRenderingFactory
  {
    public IBitmap CreateBitmap(int x, int y)
    {
      lock (RenderingLock.Lock)
      {
        return new Bitmap(x, y);
      }
    }

    public IGraphics CreateGraphics(IBitmap bitmap)
    {
      lock (RenderingLock.Lock)
      {
        return new Graphics(Draw.Graphics.FromImage(((Bitmap)bitmap).UnderlyingBitmap));
      }
    }

    public IPen CreatePen(Draw.Color color)
    {
      lock (RenderingLock.Lock)
      {
        return new Pen(color);
      }
    }

    public IBrush CreateBrush(Draw.Color color)
    {
      lock (RenderingLock.Lock)
      {
        return new Brush(color);
      }
    }

    public ITileRenderResponse CreateTileRenderResponse(object bmp)
    {
      lock (RenderingLock.Lock)
      {
        return new TileRenderResponse_Core2
        {
          TileBitmapData = ((Draw.Bitmap)bmp)?.BitmapToByteArray()
        };
      }
    }
  }
}
