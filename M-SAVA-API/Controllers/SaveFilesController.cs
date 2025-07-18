using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SaveFilesController : ControllerBase
    {
        private readonly SaveFileService _saveFileService;

        public SaveFilesController(SaveFileService saveFileService)
        {
            _saveFileService = saveFileService ?? throw new ArgumentNullException(nameof(saveFileService));
        }

        [HttpPost]
        [Consumes("application/octet-stream")]
        public async Task<ActionResult<Guid>> CreateFile(
            [FromQuery] string fileName,
            [FromQuery] string fileExtension,
            [FromQuery] List<string>? tags,
            [FromQuery] List<string>? categories,
            [FromQuery] Guid? accessGroup,
            [FromQuery] string? description,
            [FromQuery] bool publicViewing = false,
            [FromQuery] bool publicDownload = false,
            CancellationToken cancellationToken = default)
        {
            FileToSaveDTO dto = new FileToSaveDTO
            {
                FileName = fileName,
                FileExtension = fileExtension,
                Tags = tags,
                Categories = categories,
                AccessGroup = accessGroup,
                Description = description,
                PublicViewing = publicViewing,
                PublicDownload = publicDownload,
                Stream = Request.Body
            };

            Guid sessionUserId = AuthUtils.GetUserId(User);
            Guid id = await _saveFileService.CreateFileAsync(dto, sessionUserId, cancellationToken);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        [Consumes("application/octet-stream")]
        public async Task<IActionResult> UpdateFile(Guid id,
            [FromQuery] string fileName,
            [FromQuery] string fileExtension,
            [FromQuery] List<string>? tags,
            [FromQuery] List<string>? categories,
            [FromQuery] Guid? accessGroup,
            [FromQuery] string? description,
            [FromQuery] bool publicViewing = false,
            [FromQuery] bool publicDownload = false,
            CancellationToken cancellationToken = default)
        {
            FileToSaveDTO dto = new FileToSaveDTO
            {
                Id = id,
                FileName = fileName,
                FileExtension = fileExtension,
                Tags = tags,
                Categories = categories,
                AccessGroup = accessGroup,
                Description = description,
                PublicViewing = publicViewing,
                PublicDownload = publicDownload,
                Stream = Request.Body
            };

            Guid sessionUserId = AuthUtils.GetUserId(User);
            await _saveFileService.UpdateFileAsync(dto, sessionUserId, cancellationToken);
            return NoContent();
        }
    }
}
