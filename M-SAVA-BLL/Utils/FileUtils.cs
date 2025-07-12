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
    public static class FileUtils
    {
        public static bool IsFileContentValid(FileStream fileStream, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            extension = extension.Trim().TrimStart('.').ToLowerInvariant();
            fileStream.Position = 0;
            byte[] header = new byte[16];
            int read = fileStream.Read(header, 0, header.Length);
            fileStream.Position = 0;

            return IsFileContentValid(header, read, extension);
        }

        public static bool ValidateFileContent(Stream contentStream, string extension)
        {
            if (contentStream is FileStream fileStream)
            {
                return IsFileContentValid(fileStream, extension);
            }
            else
            {
                // Copy to temp file for validation
                using (var tempFile = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose))
                {
                    if (contentStream.CanSeek)
                        contentStream.Position = 0;
                    contentStream.CopyTo(tempFile);
                    if (tempFile.CanSeek)
                        tempFile.Position = 0;
                    return IsFileContentValid(tempFile, extension);
                }
            }
        }

        public static bool IsFileContentValid(byte[] header, int read, string extension)
        {
            return extension switch
            {
                // Documents
                "pdf" => read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46,
                "docx" or "xlsx" or "pptx" => read >= 4 && header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04,
                "doc" => read >= 8 && header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0,
                "xls" => read >= 8 && header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0,
                "ppt" => read >= 8 && header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0,
                "rtf" => read >= 5 && header[0] == 0x7B && header[1] == 0x5C && header[2] == 0x72 && header[3] == 0x74 && header[4] == 0x66,
                "html" => read >= 6 && (header[0] == 0x3C && header[1] == 0x68 && header[2] == 0x74 && header[3] == 0x6D && header[4] == 0x6C ||
                                        header[0] == 0x3C && header[1] == 0x48 && header[2] == 0x54 && header[3] == 0x4D && header[4] == 0x4C),
                "xml" => read >= 5 && header[0] == 0x3C && header[1] == 0x3F && header[2] == 0x78 && header[3] == 0x6D && header[4] == 0x6C,
                "json" => read >= 1 && (header[0] == 0x7B || header[0] == 0x5B),
                "csv" or "txt" or "log" or "md" or "yaml" or "ini" => true,
                // ODT/ODS/ODP: ZIP-based
                "odt" or "ods" or "odp" => read >= 4 && header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04,

                // Raster images
                "png" => read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
                "jpg" or "jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                "gif" => read >= 3 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46,
                "bmp" or "dib" => read >= 2 && header[0] == 0x42 && header[1] == 0x4D,
                "tiff" => read >= 4 && ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00) ||
                                         (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A)),
                "webp" => read >= 12 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
                "ico" => read >= 4 && header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00,
                "heic" or "heif" => read >= 12 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70,
                "psd" => read >= 4 && header[0] == 0x38 && header[1] == 0x42 && header[2] == 0x50 && header[3] == 0x53,
                "exr" => read >= 4 && header[0] == 0x76 && header[1] == 0x2F && header[2] == 0x31 && header[3] == 0x01,
                "tga" => true,
                "jp2" => read >= 12 && header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x00 && header[3] == 0x0C && header[4] == 0x6A && header[5] == 0x50 && header[6] == 0x20 && header[7] == 0x20,
                "pbm" or "pgm" or "ppm" => read >= 2 && header[0] == 0x50 && (header[1] >= 0x31 && header[1] <= 0x36),
                "xbm" or "xpm" => true,

                // Vector images
                "svg" or "svgz" => read >= 4 && (header[0] == 0x3C && header[1] == 0x3F && header[2] == 0x78 && header[3] == 0x6D),
                "eps" => read >= 4 && header[0] == 0x25 && header[1] == 0x21 && header[2] == 0x50 && header[3] == 0x53,
                "ai" => read >= 4 && header[0] == 0x25 && header[1] == 0x21 && header[2] == 0x50 && header[3] == 0x53,
                "wmf" or "emf" => true,
                "cdr" => read >= 8 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46,
                "cgm" or "dxf" or "dwg" or "sketch" or "fig" or "drw" or "vsd" or "fla" or "swf" or "sai" or "hpgl" or "plt" => true,

                // Audio
                "wav" => read >= 4 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46,
                "flac" => read >= 4 && header[0] == 0x66 && header[1] == 0x4C && header[2] == 0x61 && header[3] == 0x43,
                "mp3" => read >= 3 && ((header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33) || (header[0] == 0xFF && (header[1] & 0xE0) == 0xE0)),
                "aac" => read >= 2 && ((header[0] == 0xFF && (header[1] & 0xF6) == 0xF0)),
                "ogg" => read >= 4 && header[0] == 0x4F && header[1] == 0x67 && header[2] == 0x67 && header[3] == 0x53,
                "m4a" => read >= 8 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70,
                "wma" => read >= 16 && header[0] == 0x30 && header[1] == 0x26 && header[2] == 0xB2 && header[3] == 0x75,
                "aiff" => read >= 4 && header[0] == 0x46 && header[1] == 0x4F && header[2] == 0x52 && header[3] == 0x4D,
                "opus" => read >= 8 && header[0] == 0x4F && header[1] == 0x70 && header[2] == 0x75 && header[3] == 0x73,
                "amr" => read >= 6 && header[0] == 0x23 && header[1] == 0x21 && header[2] == 0x41 && header[3] == 0x4D && header[4] == 0x52 && header[5] == 0x0A,
                "ape" => read >= 4 && header[0] == 0x4D && header[1] == 0x41 && header[2] == 0x43 && header[3] == 0x20,
                "mpc" => read >= 3 && header[0] == 0x4D && header[1] == 0x50 && header[2] == 0x2B,
                "wv" => read >= 4 && header[0] == 0x77 && header[1] == 0x76 && header[2] == 0x70 && header[3] == 0x6B,
                "tta" => read >= 3 && header[0] == 0x54 && header[1] == 0x54 && header[2] == 0x41,
                "dsf" => read >= 4 && header[0] == 0x44 && header[1] == 0x53 && header[2] == 0x44 && header[3] == 0x20,
                "ra" => read >= 4 && header[0] == 0x2E && header[1] == 0x72 && header[2] == 0x61 && header[3] == 0xFD,
                "gsm" or "au" or "vox" => true,

                // Video
                "webm" => read >= 4 && header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3,
                "avi" => read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 && header[8] == 0x41 && header[9] == 0x56 && header[10] == 0x49,
                "mp4" or "m4v" => read >= 8 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70,
                "ogv" => read >= 4 && header[0] == 0x4F && header[1] == 0x67 && header[2] == 0x67 && header[3] == 0x53,
                "mkv" => read >= 4 && header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3,
                "mov" => read >= 8 && header[4] == 0x6D && header[5] == 0x6F && header[6] == 0x6F && header[7] == 0x76,
                "wmv" => read >= 16 && header[0] == 0x30 && header[1] == 0x26 && header[2] == 0xB2 && header[3] == 0x75,
                "flv" => read >= 3 && header[0] == 0x46 && header[1] == 0x4C && header[2] == 0x56,
                "mpg" or "mpeg" => read >= 4 && ((header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && (header[3] == 0xBA || header[3] == 0xB3))),
                "3gp" or "3g2" => read >= 8 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70,
                "asf" => read >= 16 && header[0] == 0x30 && header[1] == 0x26 && header[2] == 0xB2 && header[3] == 0x75,
                "rm" => read >= 4 && header[0] == 0x2E && header[1] == 0x52 && header[2] == 0x4D && header[3] == 0x46,
                "vob" => read >= 4 && header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0xBA,
                "ts" or "mts" or "m2ts" => read >= 1 && header[0] == 0x47,
                "f4v" => read >= 4 && header[0] == 0x46 && header[1] == 0x34 && header[2] == 0x56,

                _ => true
            };
        }

        // Helper to get file bytes for hashing (now only uses Stream)
        public static byte[] GetFileBytesFromStream(Stream dto)
        {
            if (dto.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (dto.CanSeek)
                        dto.Position = 0;
                    dto.CopyTo(ms);
                    return ms.ToArray();
                }
            }

            return Array.Empty<byte>();
        }

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

            FileExtensionType extension = FileUtils.ParseFileExtension(dto.FileExtension);

            AccessGroupDB accessGroup = null;
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

            FileExtensionType extension = FileUtils.ParseFileExtension(dto.FileExtension);

            AccessGroupDB accessGroup = null;
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
            AccessGroupDB accessGroup = null,
            bool publicViewing = false,
            bool publicDownload = false,
            bool restricted = false
        )
        {
            ulong sizeInBytes = (ulong)dto.Stream.Length;

            string checksum = BitConverter.ToString(savedFileDb.FileHash).Replace("-", "").ToLowerInvariant();

            string mimeType = MetadataUtils.GetContentType(dto.FileExtension);

            IQueryable<string> tags = (dto.Tags ?? new List<string>()).AsQueryable();
            IQueryable<string> categories = (dto.Categories ?? new List<string>()).AsQueryable();

            JsonDocument metadata = MetadataUtils.ExtractMetadataFromFileStream(dto.Stream);

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
