using System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IPen
  {
    Color Color { get; set; }
    IBrush Brush { get; set; }
    int Width { get; set; }
  }
}