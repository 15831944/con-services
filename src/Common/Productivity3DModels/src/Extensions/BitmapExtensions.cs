﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Logging;

namespace VSS.Productivity3D.Models.Extensions
{
  public static class BitmapExtensions
  {
    /// <summary>
    /// Converts a bitmap to an array of bytes representing the image
    /// </summary>
    /// <param name="bitmap">The bitmap to convert</param>
    /// <returns>An array of bytes</returns>
    public static byte[] BitmapToByteArray(this Bitmap bitmap)
    {
      using var bitmapStream = new MemoryStream();
      bitmap.Save(bitmapStream, ImageFormat.Png);
      return bitmapStream.ToArray();
    }
  }
}
