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
        public async Task<ActionResult<Guid>> CreateFile([FromForm] FileToSaveDTO dto, CancellationToken cancellationToken)
        {
            if (dto == null)
                return BadRequest("File content is required.");

            Guid id = await _saveFileService.CreateFileAsync(dto, cancellationToken);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFile(Guid id, [FromForm] FileToSaveDTO dto, CancellationToken cancellationToken)
        {
            if (dto == null)
                return BadRequest("File content is required.");

            dto.Id = id;
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
