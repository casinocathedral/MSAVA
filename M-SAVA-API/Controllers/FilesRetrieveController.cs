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

        public FilesRetrieveController(IReturnFileService returnFileService)
        {
            _returnFileService = returnFileService ?? throw new ArgumentNullException(nameof(returnFileService));
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetFileById(Guid refId)
        {
            return _returnFileService.GetFileById(refId).FileStream;
        }
    }
}
