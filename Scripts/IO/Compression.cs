using System.IO;
using System.IO.Compression;

namespace Framework.IO
{
    /// <summary>
    /// A class containing data compression utilities.
    /// </summary>
    public static class Compression
    {
        /// <summary>
        /// Compresses data using the Deflate algorithm.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <param name="level">The amount of compression to apply.</param>
        /// <returns>A new byte array contaning the compressed data.</returns>
        public static byte[] Compress(byte[] data, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var output = new MemoryStream())
            {
                using (var dstream = new DeflateStream(output, level, true))
                {
                    dstream.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// Decompresses data compressed using the Deflate algorithm.
        /// </summary>
        /// <param name="compressedData">The compressed data.</param>
        /// <returns>A new byte array contaning the decompressed data.</returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            using (var input = new MemoryStream(compressedData))
            using (var output = new MemoryStream())
            {
                using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    dstream.CopyTo(output);
                }
                return output.ToArray();
            }
        }
    }
}
