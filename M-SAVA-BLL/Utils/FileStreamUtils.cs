using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Utils
{
    public static class FileStreamUtils
    {
        public static async Task<(ulong length, byte[] hash, byte[] fileBytes)> ExtractFileStreamData(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }
            ulong length = (ulong)fileBytes.Length;
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(fileBytes);
            }
            return (length, hash, fileBytes);
        }
    }
}
