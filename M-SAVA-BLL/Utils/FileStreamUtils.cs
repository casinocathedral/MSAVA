using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Utils
{
    public static class FileStreamUtils
    {
        public static async Task<(ulong length, byte[] hash, byte[] fileBytes, MemoryStream memoryStream)> ExtractFileStreamData(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            MemoryStream memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            ulong length = (ulong)fileBytes.Length;
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(fileBytes);
            }
            memoryStream.Position = 0;
            return (length, hash, fileBytes, memoryStream);
        }
    }
}
