using M_SAVA_INF.Models;
using M_SAVA_INF.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace M_SAVA_INF.Managers
{
    public class FileManager
    {
        public string fileRootPath { get; } = Path.Combine(AppContext.BaseDirectory, "Data");
        public FileManager()
        {
            if (!Directory.Exists(fileRootPath))
            {
                Directory.CreateDirectory(fileRootPath);
            }
        }

        public string GetFileRootPath()
        {
            return fileRootPath;
        }

        public async Task SaveFileContentAsync(SavedFileMetaJSON fileMeta, byte[] fileHash, string fileExtension, Stream contentStream, CancellationToken cancellationToken = default, bool overwrite = false)
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
                throw new IOException($"File already exists at path: {path}");
            }
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await contentStream.CopyToAsync(fileStream, cancellationToken);
            }

            string metaPath = path + ".meta.json";
            var metaJson = System.Text.Json.JsonSerializer.Serialize(fileMeta);
            await File.WriteAllTextAsync(metaPath, metaJson, cancellationToken);
        }

        public FileStream? GetFileStream(byte[] fileHash, string fileExtension, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read)
        {
            string path = FileContentUtils.GetFilePath(fileHash, fileExtension);
            if (!File.Exists(path))
                return null;
            return new FileStream(path, mode, access, FileShare.Read, 81920, useAsync: true);
        }

        public PhysicalFileResult GetPhysicalFile(string filePath, string contentType)
        {
            string fullPath = Path.GetFullPath(Path.Combine(fileRootPath, filePath));

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {filePath}", fullPath);

            var fileName = Path.GetFileName(fullPath);
            return new PhysicalFileResult(fullPath, contentType)
            {
                FileDownloadName = fileName,
                EnableRangeProcessing = true
            };
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

        public bool CheckFileAccessByPath(string fullPath, List<Guid> userAccessGroups)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The file '{fullPath}' does not exist.");
            }

            string metaPath = fullPath + ".meta.json";

            if (!File.Exists(metaPath))
            {
                throw new UnauthorizedAccessException($"Meta file '{metaPath}' does not exist.");
            }

            var metaJson = File.ReadAllText(metaPath);
            var meta = JsonSerializer.Deserialize<SavedFileMetaJSON>(metaJson);

            if (meta == null)
            {
                throw new FileNotFoundException($"Meta file '{metaPath}' is invalid.");
            }
            if (meta.PublicDownload)
            {
                return true;
            }
            if (userAccessGroups == null || !userAccessGroups.Contains(meta.AccessGroupId))
            {
                throw new UnauthorizedAccessException("User does not have access to this file's access group.");
            }

            return true;
        }
    }
}
