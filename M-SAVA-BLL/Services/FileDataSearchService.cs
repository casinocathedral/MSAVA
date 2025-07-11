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
        // Andrew - Your job will be to create a new service in the Business Layer, which is FileDataSearch 
        // service, which will do very basic search functionalities for the tags, categories, description, 
        // file name and properties. It will only need to call SavedFileDataDB from the database 
        // (link (https://github.com/Markinatorina/MSAVA/blob/main/M-SAVA-DAL/Models/SavedFileDataDB.cs)). 
        // This will also require creating a new controller whose purpose will only be access to this service.
        // You can ask me because I have the most experience with search algorithms in this group.
        /*Self note: make methods to search based on; tags categories, descriptions, file name, and properties, not to sure what 
        properties are yet, but maybe starting with file name would make sense, 
        step 1 - I need to access the data, in the database, so I need a dto for the data i need from SavedFileDataDB.
        step 2 - SInce I am just searching what is already there, I suppose I only need the httpGet(tags), httpGet(description), etc. 
        step 3 - In this file write the Linq and or algorithims to search for the specific data and return it, call these methods 
                 in the controller.
        atep 4 - 
        */

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