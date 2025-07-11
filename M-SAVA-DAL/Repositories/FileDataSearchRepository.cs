using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public class FileDataSearchRepository : IdentifiableRepository<SavedFileDataDB>, IFileDataSearchRepository
    {
        private readonly BaseDataContext _context;

        public FileDataSearchRepository(BaseDataContext context) : base(context)
        {
            _context = context;
        }



        public List<string> GetAllTags()
        {
            return _context.FileData
                .AsNoTracking()
                .SelectMany(file => file.Tags)
                .Distinct()
                .ToList();
        }
        public List<string> GetAllCatagories()
        {
            return _context.FileData
                .AsNoTracking()
                .SelectMany(file => file.Categories)
                .Distinct()
                .ToList();

        }
        public List<string> GetAllName()
        {
            return _context.FileData
                .AsNoTracking()
                .Select(file => file.Name)
                .Distinct()
                .ToList();
        }
        public List<string> GetAllDescriptions()
        {
            return _context.FileData
                .AsNoTracking()
                .Select(file => file.Description)
                .Distinct()
                .ToList();
        }

        public List<Guid> SearchByTag(string tag)
        {
            return _context.FileData
                .Where(f => f.Tags != null && f.Tags.Contains(tag))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> SearchByCatagory(string Catagory)
        {
            return _context.FileData
                .Where(f => f.Categories != null && f.Categories.Contains(Catagory))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> SearchByName(string name)
        {
            return _context.FileData
                .Where(f => f.Name != null && f.Name.Contains(name))
                .Select(f => f.Id)
                .ToList();
        }

        public List<Guid> SearchByDescription(string description)
        {
            return _context.FileData
                .Where(f => f.Description != null && f.Description.Contains(description))
                .Select(f => f.Id)
                .ToList();
        }
        
        


    }

}
    /*public class FileDataSearchRepository : IdentifiableRepository<SavedFileDataDB>, IFileDataSearchRepository
    {
        protected readonly DbSet<SavedFileDataDB> _entities
        {
            _entities = context.SavedFileData;
        }

        public FileDataSearchRepository(BaseDataContext context) : base(context)
        {
        }

        public string[] GetAllTags(SavedFileDataDB Tags)
        {
            if (Tags == null) throw new ArgumentNullException(nameof(Tags), "Tags are empty");
            var query = _entities.AsQueryable();
            var results = query.Where(x =>
                x.FileName.Contains(searchTerm) ||
                x.Tags.Any(tag => tag.Name.Contains(searchTerm))
            );
        }

        /* public string GetByTag(SavedFileDataDB Tags)
         {
             if (Tags == null) throw new ArgumentNullException(nameof(Tags), "Cannot be null");

         } */

