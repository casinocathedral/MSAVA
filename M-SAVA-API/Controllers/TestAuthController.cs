using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace M_SAVA_API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestAuthController : ControllerBase
    {
        // JWT claim type enforcement
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetUserInfo()
        {
            try
            {
                // 1. Check "id" is a valid Guid
                var idClaim = User.FindFirst("id")?.Value;
                if (idClaim == null || !Guid.TryParse(idClaim, out var userId))
                    return Unauthorized();

                // 2. Check "username" is present and is a string
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized();

                // 3. (OPTIONAL) If you have an "isAdmin" claim, validate that it's strictly "true" or "false"
                var isAdminClaim = User.FindFirst("isAdmin")?.Value;
                bool? isAdmin = null;
                if (isAdminClaim != null)
                {
                    if (isAdminClaim == "true")
                        isAdmin = true;
                    else if (isAdminClaim == "false")
                        isAdmin = false;
                    else
                        return Unauthorized(); // not a valid bool string
                }

                // 4. All checks passed, return claims
                return Ok(new
                {
                    message = "JWT claims extracted successfully.",
                    userId,
                    username,
                    isAdmin
                });
            }
            catch
            {
                return Unauthorized();
            }
        }

        // You can leave this as-is (it doesn't require claim validation)
        [HttpGet("headers")]
        public IActionResult GetHeaders()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var customHeader = Request.Headers["X-My-Custom-Header"].FirstOrDefault();

            return Ok(new
            {
                message = "Headers extracted successfully.",
                AuthorizationHeader = authHeader,
                CustomHeader = customHeader
            });
        }
    }
}
