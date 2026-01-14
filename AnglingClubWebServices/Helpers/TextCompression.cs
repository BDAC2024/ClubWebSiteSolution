using System.IO;
using System.IO.Compression;
using System.Text;

namespace AnglingClubWebServices.Helpers
{
    public static class TextCompression
    {
        public static byte[] GzipCompressUtf8(string text)
        {
            var inputBytes = Encoding.UTF8.GetBytes(text);

            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            {
                gzip.Write(inputBytes, 0, inputBytes.Length);
            }
            return output.ToArray();
        }

        public static string GzipDecompressUtf8(byte[] gzBytes)
        {
            using var input = new MemoryStream(gzBytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
