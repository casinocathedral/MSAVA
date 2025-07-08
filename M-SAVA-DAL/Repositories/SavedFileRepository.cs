using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public class SavedFileRepository : IdentifiableRepository<SavedFileReferenceDB>, ISavedFileRepository
    {
        public static readonly string BackupDirectory =
           Path.Combine(AppContext.BaseDirectory, "Data", "Backups");

        public static readonly string FilesDirectory =
            Path.Combine(AppContext.BaseDirectory, "Data", "Files");

        public SavedFileRepository(BaseDataContext context) : base(context)
        {
        }

        public string GetFilePath(SavedFileReferenceDB file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string hashString = BitConverter.ToString(file.FileHash).Replace("-", "").ToLowerInvariant();
            string extension = file.FileExtension.ToString().TrimStart('_').ToLowerInvariant();
            return Path.Combine(FilesDirectory, $"{hashString}.{extension}");
        }

        public byte[]? GetFileBytes(SavedFileReferenceDB file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string path = GetFilePath(file);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public FileStream? GetFileStream(SavedFileReferenceDB file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string path = GetFilePath(file);
            return File.Exists(path) ? new FileStream(path, mode, access) : null;
        }
        
        public async Task<FileStream?> GetFileStreamAsync(SavedFileReferenceDB file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, CancellationToken cancellationToken = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string path = GetFilePath(file);
            if (!File.Exists(path))
                return null;
            return new FileStream(path, mode, access, FileShare.Read, 81920, useAsync: true);
        }

        public bool FileExists(SavedFileReferenceDB file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string path = GetFilePath(file);
            return File.Exists(path);
        }

        public bool FileExistsByHashAndExtension(byte[] fileHash, FileExtensionType extension)
        {
            if (fileHash == null || fileHash.Length == 0)
                throw new ArgumentNullException(nameof(fileHash), "Repository: fileHash parameter cannot be null or empty.");

            return _context.FileRefs
                .Any(f => f.FileExtension == extension && f.FileHash == fileHash);
        }
        public Guid GetFileIdByHashAndExtension(byte[] fileHash, FileExtensionType extension)
        {
            if (fileHash == null || fileHash.Length == 0)
                throw new ArgumentNullException(nameof(fileHash), "Repository: fileHash parameter cannot be null or empty.");

            SavedFileReferenceDB file = null!;
            try
            {
                file = _context.FileRefs
                    .FirstOrDefault(f => f.FileExtension == extension && f.FileHash == fileHash);
            } catch (Exception ex)
            {
                throw new Exception("Repository: Error retrieving file by hash and extension: " + ex.Message, ex);
            }

            return file.Id;
        }

        public void SaveFileFromBytes(SavedFileReferenceDB file, byte[] content, bool overwrite = false)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            if (content == null) throw new ArgumentNullException(nameof(content), "Repository: content parameter cannot be null.");

            string path = GetFilePath(file);

            if (!overwrite && File.Exists(path))
            {
                return;
            }

            File.WriteAllBytes(path, content);
        }
        public void SaveFileFromStream(SavedFileReferenceDB file, Stream content, bool overwrite = false)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            if (content == null) throw new ArgumentNullException(nameof(content), "Repository: content stream parameter cannot be null.");

            string path = GetFilePath(file);

            if (!overwrite && File.Exists(path))
            {
                return;
            }

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                content.CopyTo(fileStream);
            }
        }

        public async Task SaveFileFromStreamAsync(SavedFileReferenceDB file, Stream content, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            if (content == null) throw new ArgumentNullException(nameof(content), "Repository: content stream parameter cannot be null.");

            string path = GetFilePath(file);

            if (!overwrite && File.Exists(path))
            {
                return;
            }

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }
        }

        public async Task SaveFileFromFormFileAsync(SavedFileReferenceDB file, IFormFile formFile, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            if (formFile == null) throw new ArgumentNullException(nameof(formFile), "Repository: formFile parameter cannot be null.");

            string path = GetFilePath(file);

            if (!overwrite && File.Exists(path))
            {
                return;
            }

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await formFile.CopyToAsync(fileStream, cancellationToken);
            }
        }

        public void DeleteFileContent(SavedFileReferenceDB file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string path = GetFilePath(file);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public async Task DeleteFileContentAsync(SavedFileReferenceDB file, CancellationToken cancellationToken = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file), "Repository: SavedFileDB parameter cannot be null.");
            string path = GetFilePath(file);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path), cancellationToken);
            }
        }
    }
}
