﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VSS.Productivity3D.Common.Extensions
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
      byte[] data;
      using (var bitmapStream = new MemoryStream())
      {
        bitmap.Save(bitmapStream, ImageFormat.Png);
        bitmapStream.Position = 0;
        data = new byte[bitmapStream.Length];
        bitmapStream.Read(data, 0, (int)bitmapStream.Length);
        bitmapStream.Close();
      }
      return data;
    }
  }
}