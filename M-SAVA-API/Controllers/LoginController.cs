using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _loginService.LoginAsync(request);
            return Ok(result);
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var success = await _loginService.RegisterAsync(request);
            return Ok("User registered!");
        }
    }
}
