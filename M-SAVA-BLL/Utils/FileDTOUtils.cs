using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Utils
{
    public static class FileDTOUtils
    {
        // Helper to parse file extension string to enum
        public static FileExtensionType ParseFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return FileExtensionType.Unknown;

            string normalized = extension.Trim().TrimStart('.').ToUpperInvariant();
            string enumName = $"_{normalized}";

            if (Enum.TryParse<FileExtensionType>(enumName, out FileExtensionType result))
                return result;

            return FileExtensionType.Unknown;
        }

        // Maps FileToSaveDTO to SavedFileReferenceDB for saving in the database
        public static SavedFileReferenceDB MapFileDTOToDB(FileToSaveDTO dto)
        {
            byte[] fileHash;
            using (SHA256 sha256 = SHA256.Create())
            {
                fileHash = sha256.ComputeHash(dto.Stream);
            }

            FileExtensionType extension = FileDTOUtils.ParseFileExtension(dto.FileExtension);

            AccessGroupDB? accessGroup = null;
            if (dto.AccessGroup.HasValue && dto.AccessGroup.Value != Guid.Empty)
            {
                accessGroup = new AccessGroupDB { Id = dto.AccessGroup.Value };
            }

            SavedFileReferenceDB savedFileDb = new SavedFileReferenceDB
            {
                Id = dto.Id ?? Guid.Empty,
                FileHash = fileHash,
                FileExtension = extension,
                PublicDownload = dto.PublicDownload,
                PublicViewing = dto.PublicViewing,
                AccessGroup = accessGroup
            };

            return savedFileDb;
        }

        public static async Task<SavedFileReferenceDB> MapFileDTOToDBAsync(FileToSaveDTO dto)
        {
            if (dto.Stream.CanSeek)
            {
                dto.Stream.Position = 0;
            }
            using MemoryStream ms = new MemoryStream();
            await dto.Stream.CopyToAsync(ms);
            ms.Position = 0;
            
            byte[] fileHash;
            using (SHA256 sha256 = SHA256.Create())
            {
                fileHash = sha256.ComputeHash(ms);
            }

            FileExtensionType extension = FileDTOUtils.ParseFileExtension(dto.FileExtension);

            AccessGroupDB? accessGroup = null;
            if (dto.AccessGroup.HasValue && dto.AccessGroup.Value != Guid.Empty)
            {
                accessGroup = new AccessGroupDB { Id = dto.AccessGroup.Value };
            }

            SavedFileReferenceDB savedFileDb = new SavedFileReferenceDB
            {
                Id = dto.Id ?? Guid.Empty,
                FileHash = fileHash,
                FileExtension = extension,
                PublicDownload = dto.PublicDownload,
                PublicViewing = dto.PublicViewing,
                AccessGroup = accessGroup
            };

            return savedFileDb;
        }

        // Maps SavedFileDB to ReturnFileDTO for returning to the client
        public static ReturnFileDTO MapDBToReturnFileDTO(SavedFileReferenceDB db, byte[]? fileBytes = null, Stream? fileStream = null)
        {
            string fileName = GetFileName(db);

            string fileExtension = GetFileExtension(db);

            string contentType = MetadataUtils.GetContentType(fileExtension);

            Stream stream;
            if (fileBytes != null && fileBytes.Length > 0)
            {
                stream = new MemoryStream(fileBytes);
            }
            else if (fileStream != null && fileStream.Length > 0)
            {
                stream = fileStream;
                if (stream.CanSeek)
                    stream.Position = 0;
            }
            else
            {
                throw new ArgumentException("Either fileBytes or fileStream must be provided.");
            }

            return new ReturnFileDTO
            {
                Id = db.Id,
                FileName = fileName,
                FileExtension = fileExtension,
                FileStream = new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = fileName
                }
            };
        }

        public static string GetFileName(SavedFileReferenceDB db)
        {
            return BitConverter.ToString(db.FileHash).Replace("-", "").ToLowerInvariant();
        }

        public static string GetFileExtension(SavedFileReferenceDB db)
        {
            return db.FileExtension.ToString().TrimStart('_').ToLowerInvariant();
        }

        public static SavedFileDataDB MapDtoToMetadataDB(
            FileToSaveDTO dto,
            SavedFileReferenceDB savedFileDb,
            UserDB owner,
            UserDB lastModifiedBy,
            AccessGroupDB? accessGroup = null,
            bool publicViewing = false,
            bool publicDownload = false,
            bool restricted = false
        )
        {
            ulong sizeInBytes = (ulong)dto.Stream.Length;

            string checksum = BitConverter.ToString(savedFileDb.FileHash).Replace("-", "").ToLowerInvariant();

            string mimeType = MetadataUtils.GetContentType(dto.FileExtension);

            string[] tags = (dto.Tags ?? new List<string>()).ToArray();
            string[] categories = (dto.Categories ?? new List<string>()).ToArray();

            JsonDocument metadata = MetadataUtils.ExtractMetadataFromFileStream(dto.Stream, dto.FileExtension);

            return new SavedFileDataDB
            {
                Id = dto.Id ?? Guid.NewGuid(),
                FileReference = savedFileDb,
                SizeInBytes = sizeInBytes,
                SavedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = lastModifiedBy,
                Checksum = checksum,
                Name = dto.FileName,
                Description = dto.Description ?? string.Empty,
                MimeType = mimeType,
                FileExtension = dto.FileExtension,
                Tags = tags,
                Categories = categories,
                Owner = owner,
                Metadata = metadata,
                PublicViewing = dto.PublicViewing,
                DownloadCount = 0,
            };
        }
    }
}
