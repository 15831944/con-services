﻿using System.IO;
using System.IO.Compression;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.Storage.Utilities
{
    /// <summary>
    /// Provides a capability to take a memory stream compress it
    /// </summary>
    public class MemoryStreamCompression
    {
        /// <summary>
        /// Accepts a memory stream containing data to be compressed
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A memory stream containing the compressed result</returns>
        public static MemoryStream Compress(MemoryStream input)
        {
            if (input == null)
            {
                return null;
            }

            // Assume compression will at least halve the size of the data so set initial capacity to this value
            var compressStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();

            input.Position = 0;
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress, true))
            {
                input.CopyTo(compressor);
            }

            compressStream.Position = 0;
            return compressStream;
        }

        /// <summary>
        /// Accepts a memory stream containing data to be compressed
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A memory stream containing the decompressed result</returns>
        public static MemoryStream Decompress(MemoryStream input)
        {
            if (input == null)
            {
                return null;
            }

            // Assume compression will at least halve the size of the data so set initial capacity to this value
            var output = RecyclableMemoryStreamManagerHelper.Manager.GetStream();

            input.Position = 0;
            using (var decompressor = new DeflateStream(input, CompressionMode.Decompress, true))
            {
                decompressor.CopyTo(output);
            }

            output.Position = 0;
            return output;
        }
    }
}
