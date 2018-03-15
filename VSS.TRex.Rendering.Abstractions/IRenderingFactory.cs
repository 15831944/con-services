using System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IRenderingFactory
  {
    IBitmap CreateBitmap(int x, int y);
    IGraphics CreateGraphics(IBitmap bitmap);
    IPen CreatePen(Color color);
    IBrush CreateBrush(Color color);
  }
}