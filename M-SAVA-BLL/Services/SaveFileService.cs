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

namespace M_SAVA_BLL.Services
{
    public class SaveFileService
    {
        private readonly IIdentifiableRepository<SavedFileReferenceDB> _savedRefsRepository;
        private readonly IIdentifiableRepository<SavedFileDataDB> _savedDataRepository;
        private readonly UserService _userService;
        private readonly FileManager _fileManager;

        public SaveFileService(IIdentifiableRepository<SavedFileReferenceDB> savedFileRepository, IIdentifiableRepository<SavedFileDataDB> savedDataRepository, UserService userService, FileManager fileManager)
        {
            _savedRefsRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository), "Service: savedFileRepository cannot be null.");
            _savedDataRepository = savedDataRepository ?? throw new ArgumentNullException(nameof(savedDataRepository), "Service: savedDataRepository cannot be null.");
            _userService = userService ?? throw new ArgumentNullException(nameof(userService), "Service: userService cannot be null.");
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager), "Service: fileManager cannot be null.");
        }

        public async Task<Guid> CreateFileAsync(FileToSaveDTO dto, Guid sessionUserId, CancellationToken cancellationToken = default)
        {
            var sessionUser = await _userService.GetUserByIdAsync(sessionUserId, cancellationToken);
            SavedFileReferenceDB savedFileDb = DataMappingUtils.MapSavedFileReferenceDB(dto);

            var savedFileDataDb = DataMappingUtils.MapSavedFileDataDB(
                dto,
                savedFileDb,
                sessionUser,
                sessionUser
            );
            _savedDataRepository.Insert(savedFileDataDb);
            await _savedDataRepository.CommitAsync();

            savedFileDb.Id = Guid.NewGuid();
            _savedRefsRepository.Insert(savedFileDb);
            await _savedRefsRepository.CommitAsync();

            await _fileManager.SaveFileContentAsync(savedFileDb.FileHash, savedFileDb.FileExtension.ToString(), dto.Stream, cancellationToken);

            return savedFileDb.Id;
        }

        public async Task UpdateFileAsync(FileToSaveDTO dto, Guid sessionUserId, CancellationToken cancellationToken = default)
        {
            var sessionUser = await _userService.GetUserByIdAsync(sessionUserId, cancellationToken);
            SavedFileReferenceDB savedFileDb = DataMappingUtils.MapSavedFileReferenceDB(dto);

            var existingData = await _savedDataRepository.GetByIdAsync(dto.Id ?? Guid.Empty, cancellationToken);
            var savedFileDataDb = DataMappingUtils.MapSavedFileDataDB(
                dto,
                savedFileDb,
                existingData.Owner,
                sessionUser
            );
            _savedDataRepository.Update(savedFileDataDb);
            await _savedDataRepository.CommitAsync();

            _savedRefsRepository.Update(savedFileDb);
            await _savedRefsRepository.CommitAsync();

            await _fileManager.SaveFileContentAsync(savedFileDb.FileHash, savedFileDb.FileExtension.ToString(), dto.Stream, cancellationToken, true);
        }

        public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _savedRefsRepository.DeleteByIdAsync(id);
            await _savedRefsRepository.CommitAsync();
        }
    }
}