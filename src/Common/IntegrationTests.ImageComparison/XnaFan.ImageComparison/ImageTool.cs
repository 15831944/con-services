﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// see https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET

// Created in 2012 by Jakob Krarup (www.xnafan.net).
// Use, alter and redistribute this code freely,
// but please leave this comment :)

namespace XnaFan.ImageComparison.Netcore.XnaFan.ImageComparison
{
  /// <summary>
  /// A class with extensionmethods for comparing images
  /// </summary>
  public static class ImageTool
  {
    private static readonly PathGrayscaleTupleComparer comparer = new PathGrayscaleTupleComparer();

    /// <summary>
    /// Gets the difference between two images as a percentage
    /// </summary>
    /// <returns>The difference between the two images as a percentage</returns>
    /// <param name="image1Path">The path to the first image</param>
    /// <param name="image2Path">The path to the second image</param>
    /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
    /// <returns>The difference between the two images as a percentage</returns>
    public static float GetPercentageDifference(string image1Path, string image2Path, byte threshold = 3)
    {
      if (!CheckIfFileExists(image1Path) || !CheckIfFileExists(image2Path))
      {
        return -1;
      }

      var img1 = Image.Load<Rgba32>(image1Path);
      var img2 = Image.Load<Rgba32>(image2Path);

      var difference = img1.PercentageDifference(img2, threshold);

      img1.Dispose();
      img2.Dispose();

      return difference;
    }

    /// <summary>
    /// Gets the difference between two images as a percentage
    /// </summary>
    /// <returns>The difference between the two images as a percentage</returns>
    /// <param name="image1Path">The path to the first image</param>
    /// <param name="image2Path">The path to the second image</param>
    /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
    /// <returns>The difference between the two images as a percentage</returns>
    public static float GetBhattacharyyaDifference(string image1Path, string image2Path)
    {
      if (CheckIfFileExists(image1Path) && CheckIfFileExists(image2Path))
      {
        Image<Rgba32> img1 = Image.Load<Rgba32>(image1Path);
        Image<Rgba32> img2 = Image.Load<Rgba32>(image2Path);

        float difference = img1.BhattacharyyaDifference(img2);

        img1.Dispose();
        img2.Dispose();

        return difference;
      }
      else return -1;
    }


    /// <summary>
    /// Find all duplicate images in a folder, and possibly subfolders
    /// IMPORTANT: this method assumes that all files in the folder(s) are images!
    /// </summary>
    /// <param name="folderPath">The folder to look for duplicates in</param>
    /// <param name="checkSubfolders">Whether to look in subfolders too</param>
    /// <returns>A list of all the duplicates found, collected in separate Lists (one for each distinct image found)</returns>
    public static List<List<string>> GetDuplicateImages(string folderPath, bool checkSubfolders)
    {
      var imagePaths = Directory.GetFiles(folderPath, "*.*", checkSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
      return GetDuplicateImages(imagePaths);
    }

    /// <summary>
    /// Find all duplicate images from in list
    /// </summary>
    /// <param name="pathsOfPossibleDuplicateImages">The paths to the images to check for duplicates</param>
    /// <returns>A list of all the duplicates found, collected in separate Lists (one for each distinct image found)</returns>
    public static List<List<string>> GetDuplicateImages(IEnumerable<string> pathsOfPossibleDuplicateImages)
    {
      var imagePathsAndGrayValues = GetSortedGrayscaleValues(pathsOfPossibleDuplicateImages);
      var duplicateGroups = GetDuplicateGroups(imagePathsAndGrayValues);

      var pathsGroupedByDuplicates = new List<List<string>>();
      foreach (var list in duplicateGroups)
      {
        pathsGroupedByDuplicates.Add(list.Select(tuple => tuple.Item1).ToList());
      }

      return pathsGroupedByDuplicates;
    }

    private static List<Tuple<string, byte[,]>> GetSortedGrayscaleValues(IEnumerable<string> pathsOfPossibleDuplicateImages)
    {
      var imagePathsAndGrayValues = new List<Tuple<string, byte[,]>>();
      foreach (var imagePath in pathsOfPossibleDuplicateImages)
      {
        using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
        {
          byte[,] grayValues = image.GetGrayScaleValues();
          var tuple = new Tuple<string, byte[,]>(imagePath, grayValues);
          imagePathsAndGrayValues.Add(tuple);
        }
      }

      imagePathsAndGrayValues.Sort(comparer);
      return imagePathsAndGrayValues;
    }

    private static List<List<Tuple<string, byte[,]>>> GetDuplicateGroups(List<Tuple<string, byte[,]>> imagePathsAndGrayValues)
    {
      var duplicateGroups = new List<List<Tuple<string, byte[,]>>>();
      var currentDuplicates = new List<Tuple<string, byte[,]>>();

      foreach (Tuple<string, byte[,]> tuple in imagePathsAndGrayValues)
      {
        if (currentDuplicates.Any() && comparer.Compare(currentDuplicates.First(), tuple) != 0)
        {
          if (currentDuplicates.Count > 1)
          {
            duplicateGroups.Add(currentDuplicates);
            currentDuplicates = new List<Tuple<string, byte[,]>>();
          }
          else
          {
            currentDuplicates.Clear();
          }
        }

        currentDuplicates.Add(tuple);
      }
      if (currentDuplicates.Count > 1)
      {
        duplicateGroups.Add(currentDuplicates);
      }
      return duplicateGroups;
    }

    private static bool CheckIfFileExists(string filePath)
    {
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException("File '" + filePath + "' not found!");
      }
      return true;
    }
  }
}
