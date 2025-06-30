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

namespace M_SAVA_BLL.Services
{
    public class SaveFileService
    {
        private readonly ISavedFileRepository _savedFileRepository;

        public SaveFileService(ISavedFileRepository savedFileRepository)
        {
            _savedFileRepository = savedFileRepository ?? throw new ArgumentNullException(nameof(savedFileRepository), "Service: savedFileRepository cannot be null.");
        }

        // Create
        public Guid CreateFile(FileToSaveDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Service: FileToSaveDTO cannot be null.");

            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            if (_savedFileRepository.FileExistsByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension))
            {
                return _savedFileRepository.GetFileIdByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension);
            }

            SaveFileContent(savedFileDb, dto, false);

            savedFileDb.Id = Guid.NewGuid();

            _savedFileRepository.Insert(savedFileDb);
            _savedFileRepository.Commit();

            return savedFileDb.Id;
        }

        // Update
        public void UpdateFile(FileToSaveDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Service: FileToSaveDTO cannot be null.");

            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            SaveFileContent(savedFileDb, dto, true);

            _savedFileRepository.Update(savedFileDb);
            _savedFileRepository.Commit();
        }

        // Delete
        public void DeleteFile(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Service: File id cannot be empty.", nameof(id));

            _savedFileRepository.DeleteById(id);
            _savedFileRepository.Commit();
        }

        // Helper to save file content (sync)
        private void SaveFileContent(SavedFileDB savedFileDb, FileToSaveDTO dto, bool overwrite)
        {
            if (savedFileDb == null)
                throw new ArgumentNullException(nameof(savedFileDb), "Service: SavedFileDB cannot be null.");
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Service: FileToSaveDTO cannot be null.");

            string extension = dto.FileExtension;
            Stream? contentStream = null;
            bool disposeStream = false;

            if (dto.Stream != null && dto.Stream.Length > 0)
            {
                contentStream = dto.Stream;
            }
            else if (dto.FormFile != null && dto.FormFile.Length > 0)
            {
                contentStream = dto.FormFile.OpenReadStream();
                disposeStream = true;
            }
            else if (dto.Bytes != null && dto.Bytes.Length > 0)
            {
                contentStream = new MemoryStream(dto.Bytes);
                disposeStream = true;
            }
            else
            {
                throw new ArgumentException("Service: No file content provided.");
            }

            try
            {
                contentStream.Position = 0;
                bool isValid = FileUtils.ValidateFileContent(contentStream, extension);
                if (!isValid)
                    throw new ArgumentException("Service: File content does not match the provided extension.");
                contentStream.Position = 0;

                _savedFileRepository.SaveFileFromStream(savedFileDb, contentStream, overwrite);
            }
            finally
            {
                if (disposeStream && contentStream != null)
                    contentStream.Dispose();
            }
        }

        // Async CRUD methods
        // Create
        public async Task<Guid> CreateFileAsync(FileToSaveDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Service: FileToSaveDTO cannot be null.");

            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            if (_savedFileRepository.FileExistsByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension))
            {
                return _savedFileRepository.GetFileIdByHashAndExtension(savedFileDb.FileHash, savedFileDb.FileExtension);
            }

            savedFileDb.Id = Guid.NewGuid();

            _savedFileRepository.Insert(savedFileDb);
            await _savedFileRepository.CommitAsync();

            await SaveFileContentAsync(savedFileDb, dto, false, cancellationToken);

            return savedFileDb.Id;
        }

        // Update
        public async Task UpdateFileAsync(FileToSaveDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Service: FileToSaveDTO cannot be null.");

            SavedFileDB savedFileDb = FileUtils.MapFileDTOToDB(dto);

            _savedFileRepository.Update(savedFileDb);
            await _savedFileRepository.CommitAsync();

            await SaveFileContentAsync(savedFileDb, dto, true, cancellationToken);
        }

        // Delete
        public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Service: File id cannot be empty.", nameof(id));

            await _savedFileRepository.DeleteByIdAsync(id);
            await _savedFileRepository.CommitAsync();
        }

        // Helper to save file content (async)
        private async Task SaveFileContentAsync(SavedFileDB savedFileDb, FileToSaveDTO dto, bool overwrite, CancellationToken cancellationToken = default)
        {
            if (savedFileDb == null)
                throw new ArgumentNullException(nameof(savedFileDb), "Service: SavedFileDB cannot be null.");
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Service: FileToSaveDTO cannot be null.");

            string extension = dto.FileExtension;
            Stream? contentStream = null;
            bool disposeStream = false;

            if (dto.Stream != null && dto.Stream.Length > 0)
            {
                contentStream = dto.Stream;
            }
            else if (dto.FormFile != null && dto.FormFile.Length > 0)
            {
                contentStream = dto.FormFile.OpenReadStream();
                disposeStream = true;
            }
            else if (dto.Bytes != null && dto.Bytes.Length > 0)
            {
                contentStream = new MemoryStream(dto.Bytes);
                disposeStream = true;
            }
            else
            {
                throw new ArgumentException("Service: No file content provided.");
            }

            try
            {
                contentStream.Position = 0;
                bool isValid = FileUtils.ValidateFileContent(contentStream, extension);
                if (!isValid)
                    throw new ArgumentException("Service: File content does not match the provided extension.");
                contentStream.Position = 0;

                await _savedFileRepository.SaveFileFromStreamAsync(savedFileDb, contentStream, overwrite, cancellationToken);
            }
            finally
            {
                if (disposeStream && contentStream != null)
                    await contentStream.DisposeAsync();
            }
        }
    }
}