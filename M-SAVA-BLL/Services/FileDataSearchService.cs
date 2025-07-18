using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class FileDataSearchService
    {
        private readonly IFileDataSearchRepository _FileDataSearchRepository;

        public FileDataSearchService(IFileDataSearchRepository FileDataSearchRepository)
        {
            _FileDataSearchRepository = FileDataSearchRepository ?? throw new ArgumentNullException(nameof(FileDataSearchRepository), "Service: savedFileRepository cannot be null.");
        }

        public List<Guid> GetFileIdByTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("tag must be provided", nameof(tag));
            return _FileDataSearchRepository.SearchByTag(tag);

        }
        public List<Guid> GetFileByCatagory(string Catagory)
        {
            if (string.IsNullOrWhiteSpace(Catagory)) throw new ArgumentException("The catagory must be provided. ", nameof(Catagory));
            return _FileDataSearchRepository.SearchByCatagory(Catagory);
        }
        public List<Guid> GetFileByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The name must be provided. ", nameof(name));
            return _FileDataSearchRepository.SearchByName(name);
        }
        public List<Guid> GetFileByDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("The description must be provided. ", nameof(description));
            return _FileDataSearchRepository.SearchByDescription(description);
        }
        public List<string> GetAllCatagories()
        {
            return _FileDataSearchRepository.GetAllCatagories();
        }

        public List<string> GetAllTags()
        {
            return _FileDataSearchRepository.GetAllTags();
        }

        public List<string> GetAllNames()
        {
            return _FileDataSearchRepository.GetAllName();
        }
        public List<string> GetAllDescriptions()
        {
            return _FileDataSearchRepository.GetAllDescriptions();
        }


    }
}