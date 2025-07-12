using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;
// using M_SAVA_DAL.Models; // Not needed since _sessionUser is commented
using System;
using System.Collections.Generic;
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

        /*
        // Placeholder user to simulate session data until session reading is implemented
        private readonly UserDB _sessionUser = new UserDB
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Username = "placeholder_user",
            PasswordHash = Array.Empty<byte>(),
            PasswordSalt = Array.Empty<byte>(),
        };
        */

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
            // Commented out temporarily to avoid EF issues
            /*
            var dto = new FileToSaveDTO
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

            Guid id = await _saveFileService.CreateFileAsync(dto, _sessionUser, cancellationToken);
            return Ok(id);
            */

            return Ok("Stubbed CreateFile endpoint (EF logic disabled for now)");
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
            // Commented out temporarily to avoid EF issues
            /*
            var dto = new FileToSaveDTO
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

            await _saveFileService.UpdateFileAsync(dto, _sessionUser, cancellationToken);
            return NoContent();
            */

            return Ok("Stubbed UpdateFile endpoint (EF logic disabled for now)");
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFile(Guid id, CancellationToken cancellationToken)
        {
            // This may still work fine if it doesn’t use the user
            await _saveFileService.DeleteFileAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
