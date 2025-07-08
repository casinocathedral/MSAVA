using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [FromQuery] bool restricted = false,
        [FromQuery] bool publicDownload = false,
        CancellationToken cancellationToken = default)
        {
            var dto = new FileToSaveDTO
            {
                FileName = fileName,
                FileExtension = fileExtension,
                Tags = tags,
                Categories = categories,
                AccessGroup = accessGroup,
                Description = description,
                Restricted = restricted,
                PublicDownload = publicDownload,
                Stream = Request.Body // raw stream
            };

            Guid id = await _saveFileService.CreateFileAsync(dto, cancellationToken);
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
            [FromQuery] bool restricted = false,
            [FromQuery] bool publicDownload = false,
            CancellationToken cancellationToken = default)
        {
            var dto = new FileToSaveDTO
            {
                Id = id,
                FileName = fileName,
                FileExtension = fileExtension,
                Tags = tags,
                Categories = categories,
                AccessGroup = accessGroup,
                Description = description,
                Restricted = restricted,
                PublicDownload = publicDownload,
                Stream = Request.Body
            };

            await _saveFileService.UpdateFileAsync(dto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFile(Guid id, CancellationToken cancellationToken)
        {
            await _saveFileService.DeleteFileAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
