using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Utils;
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
    public static class DataMappingUtils
    {
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

        public static SavedFileReferenceDB MapSavedFileReferenceDB(FileToSaveDTO dto)
        {
            byte[] fileHash;
            using (SHA256 sha256 = SHA256.Create())
            {
                fileHash = sha256.ComputeHash(dto.Stream);
            }

            FileExtensionType extension = DataMappingUtils.ParseFileExtension(dto.FileExtension);

            SavedFileReferenceDB savedFileDb = new SavedFileReferenceDB
            {
                Id = dto.Id ?? Guid.Empty,
                FileHash = fileHash,
                FileExtension = extension,
                PublicDownload = dto.PublicDownload,
                PublicViewing = dto.PublicViewing,
                AccessGroupId = dto.AccessGroupId
            };

            return savedFileDb;
        }

        public static ReturnFileDTO MapReturnFileDTO(SavedFileReferenceDB db, byte[]? fileBytes = null, Stream? fileStream = null)
        {
            string fileName = GetFileName(db);

            string fileExtension = FileExtensionUtils.GetFileExtension(db);

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

        public static SavedFileDataDB MapSavedFileDataDB(
            FileToSaveDTO dto,
            SavedFileReferenceDB savedFileDb,
            Guid owner,
            Guid lastModifiedBy,
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
                FileReferenceId = savedFileDb.Id,
                SizeInBytes = sizeInBytes,
                SavedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedById = lastModifiedBy,
                Checksum = checksum,
                Name = dto.FileName,
                Description = dto.Description ?? string.Empty,
                MimeType = mimeType,
                FileExtension = dto.FileExtension,
                Tags = tags,
                Categories = categories,
                OwnerId = owner,
                Metadata = metadata,
                PublicViewing = dto.PublicViewing,
                DownloadCount = 0,
            };
        }

        public static SearchFileDataDTO MapSearchFileDataDTO(SavedFileDataDB db)
        {
            return new SearchFileDataDTO
            {
                DataId = db.Id,
                RefId = db.FileReference?.Id ?? Guid.Empty,
                Name = db.Name,
                Description = db.Description,
                MimeType = db.MimeType,
                FileExtension = db.FileExtension,
                Tags = db.Tags,
                Categories = db.Categories,
                SizeInBytes = db.SizeInBytes,
                Checksum = db.Checksum,
                Metadata = db.Metadata,
                PublicViewing = db.PublicViewing,
                DownloadCount = db.DownloadCount,
                SavedAt = db.SavedAt,
                LastModifiedAt = db.LastModifiedAt,
                OwnerId = db.Owner?.Id ?? Guid.Empty,
                LastModifiedById = db.LastModifiedBy?.Id ?? Guid.Empty
            };
        }
    }
}
