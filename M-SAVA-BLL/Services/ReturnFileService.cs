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

        public ReturnFileDTO GetFileById(Guid id)
        {
            SavedFileReferenceDB db = _savedFileRepository.GetById(id);
            FileStream? fileStream = _fileManager.GetFileStream(db.FileHash, db.FileExtension.ToString());
            return DataMappingUtils.MapReturnFileDTO(db, fileStream: fileStream);
        }
    }
}
