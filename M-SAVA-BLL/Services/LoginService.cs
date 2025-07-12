using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace M_SAVA_BLL.Services
{
    public class LoginService
    {
        private readonly IConfiguration _config;
        private readonly BaseDataContext _db;

        public LoginService(IConfiguration config, BaseDataContext db)
        {
            _config = config;
            _db = db;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            // REPLACE this with your actual user lookup and password hash checking!
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
                return null;

            // Assume passwords are stored in plain text for this demo. DO NOT do this in production!
            if (user.PasswordHash != Encoding.UTF8.GetBytes(request.Password))
                return null;

            var token = GenerateJwtToken(user);
            return new LoginResponseDTO { Token = token };
        }

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
