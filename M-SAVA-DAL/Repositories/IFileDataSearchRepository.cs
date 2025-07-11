using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public interface IFileDataSearchRepository : IBaseRepository<SavedFileDataDB>
    {
        List<string> GetAllTags();
        List<string> GetAllCatagories();
        List<string> GetAllName();
        List<string> GetAllDescriptions();

        List<Guid> SearchByTag(string tag);
        List<Guid> SearchByCatagory(string Catagory);
        List<Guid> SearchByName(string name);
        List<Guid> SearchByDescription(string description);
    }
    
    
}