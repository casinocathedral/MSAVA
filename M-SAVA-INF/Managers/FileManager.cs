using M_SAVA_INF.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_INF.Managers
{
    public class FileManager
    {
        public async Task SaveFileContentAsync(byte[] fileHash, string fileExtension, Stream contentStream, CancellationToken cancellationToken = default, bool overwrite = false)
        {
            if (contentStream == null) throw new ArgumentNullException(nameof(contentStream));
            if (!contentStream.CanSeek)
            {
                MemoryStream ms = new MemoryStream();
                await contentStream.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
                contentStream = ms;
            }
            else
            {
                contentStream.Position = 0;
            }

            bool isValid = FileContentUtils.ValidateFileContent(contentStream, fileExtension);
            if (!isValid)
                throw new ArgumentException("File content does not match the provided extension.");

            if (contentStream.CanSeek)
                contentStream.Position = 0;

            string path = FileContentUtils.GetFilePath(fileHash, fileExtension);
            if (!overwrite && File.Exists(path))
            {
                return;
            }
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await contentStream.CopyToAsync(fileStream, cancellationToken);
            }
        }

        public FileStream? GetFileStream(byte[] fileHash, string fileExtension, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read)
        {
            string path = FileContentUtils.GetFilePath(fileHash, fileExtension);
            if (!File.Exists(path))
                return null;
            return new FileStream(path, mode, access, FileShare.Read, 81920, useAsync: true);
        }

        public bool FileExists(byte[] fileHash, string fileExtension)
        {
            string path = FileContentUtils.GetFilePath(fileHash, fileExtension);
            return File.Exists(path);
        }

        public void DeleteFileContent(byte[] fileHash, string fileExtension)
        {
            string path = FileContentUtils.GetFilePath(fileHash, fileExtension);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
