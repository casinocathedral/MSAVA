using M_SAVA_BLL.Services;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [Route("api/accessgroups")]
    [ApiController]
    [Authorize]
    public class AccessGroupsController : ControllerBase
    {
        private readonly AccessGroupService _accessGroupService;

        public AccessGroupsController(AccessGroupService accessGroupService)
        {
            _accessGroupService = accessGroupService ?? throw new ArgumentNullException(nameof(accessGroupService));
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> CreateAccessGroup(
            [FromQuery] string name)
        {
            var id = await _accessGroupService.CreateAccessGroupAsync(name);
            return Ok(id);
        }

        [HttpPost("adduser")]
        public async Task<ActionResult> AddUserToAccessGroup(
            [FromQuery] Guid userId,
            [FromQuery] Guid accessGroupId)
        {
            try
            {
                await _accessGroupService.AddAccessGroupToUserAsync(userId, accessGroupId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
