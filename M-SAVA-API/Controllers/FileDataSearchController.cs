using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FileDataSearchController : ControllerBase
    {
        private readonly FileDataSearchService _fileDataSearchService;

        public FileDataSearchController(FileDataSearchService fileDataSearchService)
        {
            _fileDataSearchService = fileDataSearchService ?? throw new ArgumentNullException(nameof(fileDataSearchService));
        }

        [HttpGet("getUserByTag")]
        public ActionResult<List<Guid>> GetFileIdsByTag(string tag)
        {
            var result = _fileDataSearchService.GetFileIdByTag(tag);
            return Ok(result);
        }
        [HttpGet("GetAllTags")]
        public ActionResult<List<string>> GetAllTags()
        {
            var result = _fileDataSearchService.GetAllTags();
            return Ok(result);
        }
        [HttpGet("GetAllCataories")]
        public ActionResult<List<string>> GetAllCatagories()
        {
            var result = _fileDataSearchService.GetAllCatagories();
            return Ok(result);
        }
        [HttpGet("GetFileByCatagory")]
        public ActionResult<List<Guid>> GetFileByCatagory(string Catagory)
        {
            var result = _fileDataSearchService.GetFileByCatagory(Catagory);
            return Ok(result);
        }
        [HttpGet("GetFileByName")]
        public ActionResult<List<Guid>> GetFileByName(string name)
        {
            var result = _fileDataSearchService.GetFileByName(name);
            return Ok(result);
        }
        [HttpGet("GetFileByDescription")]
        public ActionResult<List<Guid>> GetFileByDescription(string description)
        {
            var result = _fileDataSearchService.GetFileByDescription(description);
            return Ok(result);
        }
        [HttpGet("GetAllNames")]
        public ActionResult<List<string>> GetAllNames()
        {
            var result = _fileDataSearchService.GetAllNames();
            return Ok(result);
        }
        [HttpGet("GetAllDescriptions")]
        public ActionResult<List<string>> GetAllDescriptions()
        {
            var result = _fileDataSearchService.GetAllDescriptions();
            return Ok(result);
        }

    }
}