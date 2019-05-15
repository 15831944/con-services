﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Tile.Service.Common.Helpers;
using Xunit;

namespace VSS.Tile.Service.UnitTests.Service.WebApi.Helpers
{
  public class TileOverlayTests
  {
    [Fact]
    public void OverlayTiles_should_not_throw_When_array_is_zero_length()
    {
      var logger = new Mock<ILogger>().Object;
      var imageList = new List<byte[]>
      {
        Convert.FromBase64String("") 
      };

      _ = new TileOverlay(logger).OverlayTiles(imageList);
    }
  }
}
