using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_Core.Models;
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
using M_SAVA_BLL.Services.Interfaces;

namespace M_SAVA_BLL.Services
{
    public class ReturnFileService : IReturnFileService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedFileRepository;
        private readonly FileManager _fileManager;
        private readonly IUserService _userService;
        private readonly AccessGroupService _accessGroupService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReturnFileService(IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository,
            IUserService userService,
            AccessGroupService accessGroupService,
            FileManager fileManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _savedFileRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _accessGroupService = accessGroupService ?? throw new ArgumentNullException(nameof(accessGroupService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public StreamReturnFileDTO GetFileStreamById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            CanSessionUserAccessFile(db);

            FileStream? fileStream = _fileManager.GetFileStream(db.FileHash, db.FileExtension.ToString());
            return MappingUtils.MapReturnFileDTO(db, fileStream: fileStream);
        }

        public PhysicalReturnFileDTO GetPhysicalFileReturnDataById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            CanSessionUserAccessFile(db);

            string fileName = MappingUtils.GetFileName(db);
            string extension = FileExtensionUtils.GetFileExtension(db);
            string contentType = MetadataUtils.GetContentType(extension);
            string fullPath = FileContentUtils.GetFullPath(db.FileHash, extension);
            return new PhysicalReturnFileDTO
            {
                FilePath = fullPath,
                FileName = fileName,
                ContentType = contentType
            };
        }

        public PhysicalReturnFileDTO GetPhysicalFileReturnDataByPath(string fileNameWithExtension)
        {
            CanSessionUserAccessFile(fileNameWithExtension);

            string fileName = Path.GetFileName(fileNameWithExtension);
            string extension = Path.GetExtension(fileName).TrimStart('.');
            string contentType = MetadataUtils.GetContentType(extension);
            string fullPath = FileContentUtils.GetFullPath(fileNameWithExtension);

            return new PhysicalReturnFileDTO
            {
                FilePath = fullPath,
                FileName = fileName,
                ContentType = contentType
            };
        }

        public StreamReturnFileDTO GetFileStreamByPath(string fileNameWithExtension)
        {
            CanSessionUserAccessFile(fileNameWithExtension);

            string fileName = Path.GetFileName(fileNameWithExtension);
            string extension = Path.GetExtension(fileName).TrimStart('.');
            string contentType = MetadataUtils.GetContentType(extension);
            FileStream fileStream = _fileManager.GetFileStream(fileNameWithExtension);

            return new StreamReturnFileDTO
            {
                FileName = fileName,
                FileExtension = extension,
                FileStream = new FileStreamResult(fileStream, contentType)
            };
        }

        private bool CanSessionUserAccessFile(string fileNameWithExtension)
        {
            SessionDTO claims = _userService.GetSessionClaims();
            if (claims.IsAdmin)
            {
                return true;
            }
            try
            {
                return _fileManager.CheckFileAccessByPath(fileNameWithExtension, claims.AccessGroups);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException("User does not have permission to access this file.");
            }
        }

        private bool CanSessionUserAccessFile(SavedFileReferenceDB fileReference)
        {
            SessionDTO claims = _userService.GetSessionClaims();
            if (claims.IsAdmin)
            {
                return true;
            }
            if (fileReference.PublicDownload)
            {
                return true;
            }
            List<Guid> userAccessGroups = claims.AccessGroups ?? new List<Guid>();
            bool canAccess = userAccessGroups.Contains(fileReference.AccessGroupId);
            if (!canAccess)
            {
                throw new UnauthorizedAccessException("User does not have permission to access this file.");
            }
            return canAccess;
        }
    }
}
