﻿using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using System.Drawing;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayerBase : IDisposable
  {
    //private static readonly ILogger Log = Logging.Logger.CreateLogger<ProductionPVMDisplayerBase>();

    private const int MAX_STEP_SIZE = 10000;

    private ISubGrid _subGrid;

    protected virtual void SetSubGrid(ISubGrid value)
    {
      _subGrid = value;
    }

    /// <summary>
    /// Production data holder.
    /// </summary>
    protected ISubGrid SubGrid { get => _subGrid; set => SetSubGrid(value); }

    // Various quantities useful when displaying a sub grid full of grid data
    private int stepX;
    private int stepY;

    private double stepXIncrement;
    private double stepYIncrement;
    private double stepXIncrementOverTwo;
    private double stepYIncrementOverTwo;

    // Various quantities useful when iterating across cells in a sub grid and drawing them

    protected int north_row, east_col;
    private double currentNorth;
    private double currentEast;

    private double cellSize;

    // accumulatingScanLine is a flag indicating we are accumulating cells together
    // to for a scan line of cells that we will display in one hit
    private bool accumulatingScanLine;

    // cellStripStartX and cellStripEndX record the start and end of the strip we are displaying
    private double cellStripStartX;
    private double cellStripEndX;

    // cellStripColour records the colour of the strip of cells we will draw
    private Color cellStripColour;

    // OriginX/y and LimitX/Y denote the extents of the physical world area covered by
    // the display context being drawn into
    // protected double OriginX, OriginY, LimitX, LimitY;

    // ICOptions is a transient reference an IC options object to be used while rendering
    // ICOptions : TSVOICOptions;

    private bool displayParametersCalculated;

    private void CalculateDisplayParameters()
    {
      // Set the cell size for displaying the grid. If we will be processing
      // representative grids then set cellSize to be the size of a leaf
      // sub grid in the sub grid tree
      // oneThirdCellSize = cellSize * (1 / 3.0);
      // halfCellSize = cellSize / 2.0;
      // twoThirdsCellSize = cellSize * (2 / 3.0);

      var stepsPerPixelX = MapView.XPixelSize / cellSize;
      var stepsPerPixelY = MapView.YPixelSize / cellSize;

      stepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelX)));
      stepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelY)));

      stepXIncrement = stepX * cellSize;
      stepYIncrement = stepY * cellSize;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;
    }

    protected virtual bool DoRenderSubGrid(ISubGrid subGrid)
    {
      _subGrid = subGrid;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // See if this display supports cell strip rendering

      var drawCellStrips = SupportsCellStripRendering();

      // Calculate the world coordinate location of the origin (bottom left corner)
      // of this sub grid
      subGrid.CalculateWorldOrigin(out var subGridWorldOriginX, out var subGridWorldOriginY);

      // Draw the background of the sub grid if a pixel is less than 1 meter is width
      // if (MapView.XPixelSize < 1.0)
      //    MapView.DrawRect(SubGridWorldOriginX, SubGridWorldOriginY + cellSize * 32, cellSize * 32, cellSize * 32, true,
      //    ((SubGrid.OriginX >> 5) + (SubGrid.OriginY >> 5)) % 2 == 0 ? Color.Black : Color.Blue);

      // Skip-Iterate through the cells drawing them in strips

      var temp = subGridWorldOriginY / stepYIncrement;
      currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      north_row = (int)Math.Floor((currentNorth - subGridWorldOriginY) / cellSize);

      while (north_row < 0)
      {
        north_row += stepY;
        currentNorth += stepYIncrement;
      }

      while (north_row < SubGridTreeConsts.SubGridTreeDimension)
      {
        temp = subGridWorldOriginX / stepXIncrement;
        currentEast = (Math.Truncate(temp) * stepXIncrement) + stepXIncrementOverTwo;
        east_col = (int)Math.Floor((currentEast - subGridWorldOriginX) / cellSize);

        while (east_col < 0)
        {
          east_col += stepX;
          currentEast += stepXIncrement;
        }

        if (drawCellStrips)
          DoStartRowScan();

        while (east_col < SubGridTreeConsts.SubGridTreeDimension)
        {
          if (drawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          currentEast += stepXIncrement;
          east_col += stepX;
        }

        if (drawCellStrips)
          DoEndRowScan();

        currentNorth += stepYIncrement;
        north_row += stepY;
      }

      return true;
    }

    /// <summary>
    /// Performs a 'consistent' render across a 2D array of collated values from queried subgrids.
    /// Effectively this treats the passed array as if it were a subgrid of that size and renders it as
    /// such against the MapView.
    /// Essentially, this function should be called just once to render the entire set of data for a tile
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valueStore"></param>
    /// <param name="worldOriginX"></param>
    /// <param name="worldOriginY"></param>
    /// <param name="valueCellSizeX"></param>
    /// <param name="valueCellSizeY"></param>
    /// <returns></returns>
    public bool PerformConsistentRender<T>(T[,] valueStore,
      double worldOriginX, double worldOriginY, double valueCellSizeX, double valueCellSizeY)
    {
      var xDimension = valueStore.GetLength(0);
      var yDimension = valueStore.GetLength(1);

      var stepsPerPixelX = MapView.XPixelSize / valueCellSizeX;
      var stepsPerPixelY = MapView.YPixelSize / valueCellSizeY;

      stepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelX)));
      stepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelY)));

      stepXIncrement = stepX * valueCellSizeX;
      stepYIncrement = stepY * valueCellSizeY;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // See if this display supports cell strip rendering

      var drawCellStrips = SupportsCellStripRendering();

      // Skip-Iterate through the cells drawing them in strips

      var temp = worldOriginY / stepYIncrement;
      currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      north_row = (int)Math.Floor((currentNorth - worldOriginY) / valueCellSizeY);

      while (north_row < 0)
      {
        north_row += stepY;
        currentNorth += stepYIncrement;
      }

      while (north_row < yDimension)
      {
        temp = worldOriginX / stepXIncrement;
        currentEast = (Math.Truncate(temp) * stepXIncrement) + stepXIncrementOverTwo;
        east_col = (int)Math.Floor((currentEast - worldOriginX) / valueCellSizeX);

        while (east_col < 0)
        {
          east_col += stepX;
          currentEast += stepXIncrement;
        }

        if (drawCellStrips)
          DoStartRowScan();

        while (east_col < xDimension)
        {
          if (drawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          currentEast += stepXIncrement;
          east_col += stepX;
        }

        if (drawCellStrips)
          DoEndRowScan();

        currentNorth += stepYIncrement;
        north_row += stepY;
      }

      return true;
    }

    private void DoRenderCell()
    {
      var colour = DoGetDisplayColour();

      if (colour != Color.Empty)
      {
        MapView.DrawRect(currentEast, currentNorth,
                         cellSize, cellSize, true, colour);
      }
    }

    // SupportsCellStripRendering enables a displayer to advertise is it capable
    // of rendering cell information in strips
    protected abstract bool SupportsCellStripRendering();

    // DoGetDisplayColour queries the data at the current cell location and
    // determines the colour that should be displayed there. If there is no value
    // that should be displayed there (ie: it is <Null>, then the function returns
    // clNone as the colour).
    protected abstract Color DoGetDisplayColour();

    private void DoStartRowScan() => accumulatingScanLine = false;

    private void DoEndRowScan()
    {
      if (accumulatingScanLine)
        DoRenderStrip();
    }

    private void DoAccumulateStrip()
    {
      var displayColour = DoGetDisplayColour();

      if (displayColour != Color.Empty) // There's something to draw
      {
        // Set the end of the strip to current east
        cellStripEndX = currentEast;

        if (!accumulatingScanLine) // We should start accumulating one
        {
          accumulatingScanLine = true;
          cellStripColour = displayColour;
          cellStripStartX = currentEast;
        }
        else // ... We're already accumulating one, we might need to draw it and start again
        {
          if (cellStripColour != displayColour)
          {
            DoRenderStrip();

            accumulatingScanLine = true;
            cellStripColour = displayColour;
            cellStripStartX = currentEast;
          }
        }
      }
      else // The cell should not be drawn
      {
        if (accumulatingScanLine) // We have accumulated something that should be drawn
          DoRenderStrip();
      }
    }

    private void DoRenderStrip()
    {
      if (accumulatingScanLine && cellStripColour != Color.Empty)
      {
        MapView.DrawRect(cellStripStartX - stepXIncrementOverTwo,
          currentNorth - stepYIncrementOverTwo,
          (cellStripEndX - cellStripStartX) + stepXIncrement,
          stepYIncrement,
          true,
          cellStripColour);

        accumulatingScanLine = false;
      }
    }

    public MapSurface MapView { get; set; }

    public bool HasRenderedSubGrid { get; set; }

//    public ProductionPVMDisplayerBase()
//    {
//    }

    public bool RenderSubGrid(IClientLeafSubGrid clientSubGrid)
    {
      if (clientSubGrid != null)
      {
        if (!displayParametersCalculated)
        {
          cellSize = clientSubGrid.CellSize;
          CalculateDisplayParameters();
          displayParametersCalculated = true;
        }

        HasRenderedSubGrid = true;

        return DoRenderSubGrid(clientSubGrid);
      }

      return false;
    }

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          MapView?.Dispose();
          MapView = null;
        }

        disposedValue = true;
      }
    }

    // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ProductionPVMDisplayerBase()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // Uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
