using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Contexts;
using System.Linq;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly BaseDataContext _db;
        private readonly IConfiguration _config;

        public LoginController(BaseDataContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            // Basic user lookup (case sensitive)
            var user = _db.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized();

            // TEMPORARY: Basic password check (plain text, NOT for production!)
            var inputPassword = request.Password;
            var storedPassword = Encoding.UTF8.GetString(user.PasswordHash);
            if (inputPassword != storedPassword)
                return Unauthorized();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new LoginResponseDTO { Token = token });
        }

        // REGISTER (for quick local testing)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequestDTO request)
        {
            // Check if username exists
            if (_db.Users.Any(u => u.Username == request.Username))
                return BadRequest("Username already exists.");

            // Store password as bytes (PLAIN TEXT, for dev/testing only)
            var user = new UserDB
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = Encoding.UTF8.GetBytes(request.Password),
                PasswordSalt = Array.Empty<byte>()
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("User registered!");
        }

        // JWT Generator (for demo)
        private string GenerateJwtToken(UserDB user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "TemporarySuperSecretKey"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
