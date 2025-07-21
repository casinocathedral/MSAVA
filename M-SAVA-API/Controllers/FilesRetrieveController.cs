using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [Route("api/files/retrieve")]
    [ApiController]
    [Authorize]
    public class FilesRetrieveController : ControllerBase
    {
        private readonly IReturnFileService _returnFileService;
        private readonly IWebHostEnvironment _env;

        public FilesRetrieveController(IReturnFileService returnFileService, IWebHostEnvironment env)
        {
            _returnFileService = returnFileService ?? throw new ArgumentNullException(nameof(returnFileService));
            _env = env;
        }

        [HttpGet("stream/{refId:guid}")]
        public IActionResult GetFileStreamById(Guid refId)
        {
            return _returnFileService.GetFileStreamById(refId).FileStream;
        }

        [HttpGet("physical/{**fileNameWithExtension}")]
        public IActionResult GetPhysicalFileByPath(string fileNameWithExtension)
        {
            PhysicalReturnFileDTO fileData = _returnFileService.GetPhysicalFileReturnDataByPath(fileNameWithExtension);

            return PhysicalFile(fileData.FilePath, fileData.ContentType, fileData.FileName, enableRangeProcessing: true);
        }
    }
}
