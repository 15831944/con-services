﻿using System;
using System.Drawing;
using Microsoft.Extensions.Logging;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayer: IDisposable
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProductionPVMDisplayer>();

    public MapSurface MapView;

    // Various quantities useful when iterating across cells and drawing them
    protected int north_row, east_col;
    protected double currentNorth;
    protected double currentEast;

    protected double cellSizeX;
    protected double cellSizeY;

    // accumulatingScanLine is a flag indicating we are accumulating cells together
    // to for a scan line of cells that we will display in one hit
    private bool _accumulatingScanLine;

    // cellStripStartX and cellStripEndX record the start and end of the strip we are displaying
    private double _cellStripStartX;
    private double _cellStripEndX;

    // cellStripColour records the colour of the strip of cells we will draw
    private Color _cellStripColour;

    public abstract void SetPalette(IPlanViewPalette palette);

    public abstract IPlanViewPalette GetPalette();

    protected void DoRenderCell()
    {
      var colour = DoGetDisplayColour();

      if (colour != Color.Empty)
      {
        MapView.DrawRect(currentEast, currentNorth,
          cellSizeX, cellSizeY, true, colour);
      }
    }

    protected void DoStartRowScan() => _accumulatingScanLine = false;

    protected void DoEndRowScan()
    {
      if (_accumulatingScanLine)
        DoRenderStrip();
    }

    // DoGetDisplayColour queries the data at the current cell location and
    // determines the colour that should be displayed there. If there is no value
    // that should be displayed there (ie: it is <Null>, then the function returns
    // clNone as the colour).
    public abstract Color DoGetDisplayColour();

    private void DoRenderStrip()
    {
      if (_accumulatingScanLine && _cellStripColour != Color.Empty)
      {
        MapView.DrawRect(_cellStripStartX,
          currentNorth,
          (_cellStripEndX - _cellStripStartX) + cellSizeX,
          cellSizeY,
          true,
          _cellStripColour);

        _accumulatingScanLine = false;
      }
    }

    protected void DoAccumulateStrip()
    {
      var displayColour = DoGetDisplayColour();

      if (displayColour != Color.Empty) // There's something to draw
      {
        // Set the end of the strip to current east
        _cellStripEndX = currentEast;

        if (!_accumulatingScanLine) // We should start accumulating one
        {
          _accumulatingScanLine = true;
          _cellStripColour = displayColour;
          _cellStripStartX = currentEast;
        }
        else // ... We're already accumulating one, we might need to draw it and start again
        {
          if (_cellStripColour != displayColour)
          {
            DoRenderStrip();

            _accumulatingScanLine = true;
            _cellStripColour = displayColour;
            _cellStripStartX = currentEast;
          }
        }
      }
      else // The cell should not be drawn
      {
        if (_accumulatingScanLine) // We have accumulated something that should be drawn
          DoRenderStrip();
      }
    }

    /// <summary>
    /// Enables a display context to advertise is it capable of rendering cell information in strips.
    /// </summary>
    protected virtual bool SupportsCellStripRendering() => true;

    /// <summary>
    /// Performs iteration across a region of a single 2D array of values
    /// </summary>
    protected void DoIterate(double valueStoreCellSizeX, double valueStoreCellSizeY, double worldOriginX, double worldOriginY, double worldWidth, double worldHeight, int originX, int originY, int limitX, int limitY)
    {
      _log.LogDebug($"Performing render iteration: valueStoreCellSizeX:{valueStoreCellSizeX}, valueStoreCellSizeY:{valueStoreCellSizeY}, worldOriginX:{worldOriginX}, worldOriginY:{worldOriginY}, worldWidth:{worldWidth}, worldHeight:{worldHeight}, originX:{originX}, originY:{originY}, limitX:{limitX}, limitY:{limitY}");

      var drawCellStrips = SupportsCellStripRendering();

      cellSizeX = valueStoreCellSizeX;
      cellSizeY = valueStoreCellSizeY;

      var worldOriginXWithOffset = worldOriginX + originX * cellSizeX;
      var worldOriginYWithOffset = worldOriginY + originY * cellSizeY;

      north_row = originY;
      currentNorth = worldOriginYWithOffset;

      for (var y = originY; y <= limitY; y++)
      {
        currentEast = worldOriginXWithOffset;

        if (drawCellStrips)
          DoStartRowScan();

        east_col = originX;
        for (var x = originX; x <= limitX; x++)
        {
          if (drawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          currentEast += cellSizeX;
          east_col++;
        }

        if (drawCellStrips)
        {
          DoEndRowScan();
        }

        currentNorth += cellSizeY;
        north_row++;
      }
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          MapView?.Dispose();
          MapView = null;
        }

        _disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
