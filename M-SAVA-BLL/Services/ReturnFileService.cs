using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using M_SAVA_INF.Managers;
using M_SAVA_INF.Utils;
using M_SAVA_DAL.Utils;

namespace M_SAVA_BLL.Services
{
    public class ReturnFileService : IReturnFileService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedFileRepository;
        private readonly FileManager _fileManager;

        public ReturnFileService(IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository, FileManager fileManager)
        {
            _savedFileRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        }

        public StreamReturnFileDTO GetFileStreamById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            FileStream? fileStream = _fileManager.GetFileStream(db.FileHash, db.FileExtension.ToString());
            return MappingUtils.MapReturnFileDTO(db, fileStream: fileStream);
        }

        public PhysicalFileResult GetFileByPath(string filePath)
        {
            string extension = Path.GetExtension(filePath)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
            string contentType = MetadataUtils.GetContentType(extension);
            return _fileManager.GetPhysicalFile(filePath, contentType);
        }

        public PhysicalReturnFileDTO GetPhysicalFileReturnData(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
            string contentType = MetadataUtils.GetContentType(extension);
            
            string dataRoot = _fileManager.GetFileRootPath();
            string fullPath = Path.GetFullPath(Path.Combine(dataRoot, filePath));

            return new PhysicalReturnFileDTO
            {
                FilePath = fullPath,
                FileName = fileName,
                ContentType = contentType
            };
        }

        public PhysicalReturnFileDTO GetPhysicalFileReturnDataById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            string fileName = MappingUtils.GetFileName(db);
            string extension = FileExtensionUtils.GetFileExtension(db);
            string contentType = MetadataUtils.GetContentType(extension);
            string dataRoot = _fileManager.GetFileRootPath();
            string fullPath = Path.Combine(dataRoot, db.FileHash + "." + extension);
            return new PhysicalReturnFileDTO
            {
                FilePath = fullPath,
                FileName = fileName,
                ContentType = contentType
            };
        }
    }
}
