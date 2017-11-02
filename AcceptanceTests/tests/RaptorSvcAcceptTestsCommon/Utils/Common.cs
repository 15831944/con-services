﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RaptorSvcAcceptTestsCommon.Utils
{
    public class Common
    {
        /// <summary>
        /// Convert file to byte array.
        /// </summary>
        /// <param name="input">Name of file (with full path) to convert.</param>
        /// <returns></returns>
        public static byte[] FileToByteArray(string input)
        {
            byte[] output = null;

            FileStream sourceFile = new FileStream(input, FileMode.Open, FileAccess.Read); // Open streamer...

            BinaryReader binReader = new BinaryReader(sourceFile);
            try
            {
                output = binReader.ReadBytes((int)sourceFile.Length);
            }
            finally
            {
                sourceFile.Close(); // Dispose streamer...          
                binReader.Close(); // Dispose reader
            }

            return output;
        }

        /// <summary>
        /// Test whether two lists are equivalent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool ListsAreEqual<T>(List<T> listA, List<T> listB)
        {
            if (listA == null && listB == null)
                return true;
            else if (listA == null || listB == null)
                return false;
            else
            {
                if (listA.Count != listB.Count)
                    return false;

                for (int i = 0; i < listA.Count; ++i)
                {
                    if (!listB.Exists(item => item.Equals(listA[i])))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Decompress a zip archive file - assuming there is only one file in the archive.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            using (var compressedData = new MemoryStream(data))
            {
                ZipArchive archive = new ZipArchive(compressedData);

                using (var decompressedData = archive.Entries[0].Open())
                {
                    using(var ms = new MemoryStream())
                    {
                        decompressedData.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
        }

    /// <summary>
    /// Test whether two arrays of doubles are equivalent.
    /// </summary>
    /// <param name="arrayA"></param>
    /// <param name="arrayB"></param>
    /// /// <param name="precision"></param>
    /// <returns>True or False.</returns>
    public static bool ArraysOfDoublesAreEqual(double[] arrayA, double[] arrayB, int precision = 2)
    {
      if (arrayA == null && arrayB == null)
        return true;
      else if (arrayA == null || arrayB == null)
        return false;
      else
      {
        if (arrayA.Length != arrayB.Length)
          return false;

        for (int i = 0; i < arrayA.Length; ++i)
        {
          if (Math.Round(arrayA[i], precision) != Math.Round(arrayB[i], precision))
            return false;
        }

        return true;
      }
    }
  }
}
