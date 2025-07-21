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
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using M_SAVA_INF.Models;

namespace M_SAVA_BLL.Services
{
    public class ReturnFileService : IReturnFileService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedFileRepository;
        private readonly FileManager _fileManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReturnFileService(IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository, FileManager fileManager, IHttpContextAccessor httpContextAccessor)
        {
            _savedFileRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public StreamReturnFileDTO GetFileStreamById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            if (!db.PublicDownload)
            {
                HttpContext httpContext = _httpContextAccessor.HttpContext;
                List<Guid> userAccessGroups = new();
                if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
                {
                    userAccessGroups = sessionDto.AccessGroups ?? new List<Guid>();
                }
                if (!userAccessGroups.Contains(db.AccessGroupId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this file's access group.");
                }
            }
            FileStream? fileStream = _fileManager.GetFileStream(db.FileHash, db.FileExtension.ToString());
            return MappingUtils.MapReturnFileDTO(db, fileStream: fileStream);
        }

        public PhysicalReturnFileDTO GetPhysicalFileReturnDataById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            string fileName = MappingUtils.GetFileName(db);
            string extension = FileExtensionUtils.GetFileExtension(db);
            string contentType = MetadataUtils.GetContentType(extension);
            string dataRoot = _fileManager.GetFileRootPath();
            string fullPath = Path.Combine(dataRoot, db.FileHash + "." + extension);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The file '{fullPath}' does not exist.");
            }
            return new PhysicalReturnFileDTO
            {
                FilePath = fullPath,
                FileName = fileName,
                ContentType = contentType
            };
        }

        public PhysicalReturnFileDTO GetPhysicalFileReturnDataByPath(string path)
        {
            string fileName = Path.GetFileName(path);
            string extension = Path.GetExtension(fileName).TrimStart('.');
            string contentType = MetadataUtils.GetContentType(extension);
            string dataRoot = _fileManager.GetFileRootPath();
            string fullPath = Path.Combine(dataRoot, path);
            var httpContext = _httpContextAccessor.HttpContext;
            List<Guid> userAccessGroups = new();
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                userAccessGroups = sessionDto.AccessGroups ?? new List<Guid>();
            }
            _fileManager.CheckFileAccessByPath(fullPath, userAccessGroups);
            return new PhysicalReturnFileDTO
            {
                FilePath = fullPath,
                FileName = fileName,
                ContentType = contentType
            };
        }
    }
}
