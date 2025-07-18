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

namespace M_SAVA_BLL.Services
{
    public class SaveFileService
    {
        private readonly ISavedFileRepository _savedRefsRepository;
        private readonly IIdentifiableRepository<SavedFileDataDB> _savedDataRepository;
        private readonly UserService _userService;

        public SaveFileService(ISavedFileRepository savedFileRepository, IIdentifiableRepository<SavedFileDataDB> savedDataRepository, UserService userService)
        {
            _savedRefsRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository), "Service: savedFileRepository cannot be null.");
            _savedDataRepository = savedDataRepository ?? throw new ArgumentNullException(nameof(savedDataRepository), "Service: savedDataRepository cannot be null.");
            _userService = userService ?? throw new ArgumentNullException(nameof(userService), "Service: userService cannot be null.");
        }

        public Guid CreateFile(FileToSaveDTO dto, Guid sessionUserId)
        {
            var sessionUser = _userService.GetUserById(sessionUserId);
            SavedFileReferenceDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            var savedFileDataDb = FileUtils.MapDtoToMetadataDB(
                dto,
                savedFileDb,
                sessionUser,
                sessionUser
            );
            _savedDataRepository.Insert(savedFileDataDb);
            _savedDataRepository.Commit();

            SaveFileContent(savedFileDb, dto, false);

            savedFileDb.Id = Guid.NewGuid();
            _savedRefsRepository.Insert(savedFileDb);
            _savedRefsRepository.Commit();

            return savedFileDb.Id;
        }

        public void UpdateFile(FileToSaveDTO dto, Guid sessionUserId)
        {
            var sessionUser = _userService.GetUserById(sessionUserId);
            SavedFileReferenceDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            var existingData = _savedDataRepository.GetById(dto.Id ?? Guid.Empty);
            var savedFileDataDb = FileUtils.MapDtoToMetadataDB(
                dto,
                savedFileDb,
                existingData.Owner,
                sessionUser
            );
            _savedDataRepository.Update(savedFileDataDb);
            _savedDataRepository.Commit();

            SaveFileContent(savedFileDb, dto, true);

            _savedRefsRepository.Update(savedFileDb);
            _savedRefsRepository.Commit();
        }

        public void DeleteFile(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Service: File id cannot be empty.", nameof(id));

            _savedRefsRepository.DeleteById(id);
            _savedRefsRepository.Commit();
        }

        private void SaveFileContent(SavedFileReferenceDB savedFileDb, FileToSaveDTO dto, bool overwrite)
        {
            string extension = dto.FileExtension;
            bool isValid = FileUtils.ValidateFileContent(dto.Stream, extension);
            if (!isValid)
                throw new ArgumentException("Service: File content does not match the provided extension.");

            dto.Stream.Position = 0;

            _savedRefsRepository.SaveFileFromStream(savedFileDb, dto.Stream, overwrite);
        }

        public async Task<Guid> CreateFileAsync(FileToSaveDTO dto, Guid sessionUserId, CancellationToken cancellationToken = default)
        {
            var sessionUser = await _userService.GetUserByIdAsync(sessionUserId, cancellationToken);
            SavedFileReferenceDB savedFileDb = await FileUtils.MapFileDTOToDBAsync(dto);

            var savedFileDataDb = FileUtils.MapDtoToMetadataDB(
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

            await SaveFileContentAsync(savedFileDb, dto, false, cancellationToken);

            return savedFileDb.Id;
        }

        public async Task UpdateFileAsync(FileToSaveDTO dto, Guid sessionUserId, CancellationToken cancellationToken = default)
        {
            var sessionUser = await _userService.GetUserByIdAsync(sessionUserId, cancellationToken);
            SavedFileReferenceDB savedFileDb = await FileUtils.MapFileDTOToDBAsync(dto);

            var existingData = await _savedDataRepository.GetByIdAsync(dto.Id ?? Guid.Empty, cancellationToken);
            var savedFileDataDb = FileUtils.MapDtoToMetadataDB(
                dto,
                savedFileDb,
                existingData.Owner,
                sessionUser
            );
            _savedDataRepository.Update(savedFileDataDb);
            await _savedDataRepository.CommitAsync();

            _savedRefsRepository.Update(savedFileDb);
            await _savedRefsRepository.CommitAsync();

            await SaveFileContentAsync(savedFileDb, dto, true, cancellationToken);
        }

        public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _savedRefsRepository.DeleteByIdAsync(id);
            await _savedRefsRepository.CommitAsync();
        }

        private async Task SaveFileContentAsync(SavedFileReferenceDB savedFileDb, FileToSaveDTO dto, bool overwrite, CancellationToken cancellationToken = default)
        {
            string extension = dto.FileExtension;
            Stream contentStream = dto.Stream;

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
            
            bool isValid = FileUtils.ValidateFileContent(contentStream, extension);
            if (!isValid)
                throw new ArgumentException("Service: File content does not match the provided extension.");
            
            if (contentStream.CanSeek)
                contentStream.Position = 0;
            
            await _savedRefsRepository.SaveFileFromStreamAsync(savedFileDb, contentStream, overwrite, cancellationToken);
        }
    }
}