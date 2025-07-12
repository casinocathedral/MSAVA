using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M_SAVA_DAL.Contexts;

namespace M_SAVA_API.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly BaseDataContext _db;
    public FilesController(BaseDataContext db) => _db = db;

    // GET api/files
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        bool loggedIn = User.Identity?.IsAuthenticated == true;

        // Query from SavedFileDataDB and join with FileReference
        var query = _db.FileData
                       .Include(f => f.FileReference)
                       .AsNoTracking();

        if (!loggedIn)
            query = query.Where(f => f.FileReference.PublicDownload);

        var items = await query
            .Select(f => new
            {
                f.FileReference.Id,
                f.Name,
                f.MimeType,
                PublicDownload = f.FileReference.PublicDownload
            })
            .ToListAsync(ct);

        return Ok(items);
    }
}
