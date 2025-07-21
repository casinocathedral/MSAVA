using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System.IO;
using System.Threading;
using M_SAVA_BLL.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using M_SAVA_BLL.Utils;
using System.Text.Json;
using M_SAVA_INF.Managers;
using M_SAVA_INF.Utils;
using M_SAVA_INF.Models;

namespace M_SAVA_BLL.Services
{
    public class SaveFileService : ISaveFileService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedRefsRepository;
        private readonly IIdentifiableRepository<SavedFileDataDB> _savedDataRepository;
        private readonly FileManager _fileManager;

        public SaveFileService(IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository, IIdentifiableRepository<SavedFileDataDB> savedDataRepository, IUserService userService, FileManager fileManager)
        {
            _savedRefsRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository), "Service: savedFileRepository cannot be null.");
            _savedDataRepository = savedDataRepository ?? throw new ArgumentNullException(nameof(savedDataRepository), "Service: savedDataRepository cannot be null.");
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager), "Service: fileManager cannot be null.");
        }

        public async Task<Guid> CreateFileAsync(FileToSaveDTO dto, Guid sessionUserId, CancellationToken cancellationToken = default)
        {

            SavedFileReferenceDB savedFileDb = MappingUtils.MapSavedFileReferenceDB(dto);
            SavedFileMetaJSON savedFileMetaJSON = MappingUtils.MapSavedFileMetaJSON(savedFileDb);

            await _fileManager.SaveFileContentAsync(savedFileMetaJSON, savedFileDb.FileHash, savedFileDb.FileExtension.ToString(), dto.Stream, cancellationToken);

            var savedFileDataDb = MappingUtils.MapSavedFileDataDB(
                dto,
                savedFileDb,
                sessionUserId,
                sessionUserId
            );
            _savedDataRepository.Insert(savedFileDataDb);
            await _savedDataRepository.CommitAsync();

            savedFileDb.Id = Guid.NewGuid();
            _savedRefsRepository.Insert(savedFileDb);
            await _savedRefsRepository.CommitAsync();

            return savedFileDb.Id;
        }

        public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _savedRefsRepository.DeleteByIdAsync(id);
            await _savedRefsRepository.CommitAsync();
        }
    }
}