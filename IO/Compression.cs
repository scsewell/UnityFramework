using System.IO;
using System.IO.Compression;

namespace Framework.IO
{
    public class Compression
    {
        public static byte[] Compress(byte[] uncompressed)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal, true))
                {
                    dstream.Write(uncompressed, 0, uncompressed.Length);
                }
                return output.ToArray();
            }
        }

        public static byte[] Decompress(byte[] compressed)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        dstream.CopyTo(output);
                    }
                    return output.ToArray();
                }
            }
        }
    }
}
