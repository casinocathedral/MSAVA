﻿using M_SAVA_Core.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_INF.Environment;
using M_SAVA_BLL.Utils;
using Microsoft.Extensions.Configuration;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_BLL.Loggers;

namespace M_SAVA_BLL.Services
{
    public class LoginService : ILoginService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IIdentifiableRepository<JwtDB> _jwtRepository;
        private readonly InviteCodeService _inviteCodeService;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly byte[] _jwtKeyBytes;
        private readonly ServiceLogger _serviceLogger;

        public LoginService(
            IIdentifiableRepository<UserDB> userRepository,
            IIdentifiableRepository<JwtDB> jwtRepository,
            InviteCodeService inviteCodeService,
            IConfiguration configuration,
            ILocalEnvironment env,
            ServiceLogger serviceLogger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtRepository = jwtRepository ?? throw new ArgumentNullException(nameof(jwtRepository));
            _jwtIssuer = env.GetValue("jwt_issuer_name");
            _jwtAudience = env.GetValue("jwt_issuer_audience");
            _jwtKeyBytes = env.GetSigningKeyBytes();
            _inviteCodeService = inviteCodeService ?? throw new ArgumentNullException(nameof(inviteCodeService));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            UserDB? user = await _userRepository.GetAllAsReadOnly()
                .Include(u => u.AccessGroups)
                .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user == null || !PasswordUtils.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new InvalidOperationException("Username doesn't exist or password is incorrect.");
            }

            JwtDB token = await GenerateJwtTokenAsync(user);

            _serviceLogger.WriteLog(UserLogAction.SessionLogIn, $"User {user.Username} logged in successfully.", user.Id, null);

            return new LoginResponseDTO { Token = token.TokenString };
        }

        public async Task<JwtDB> GenerateJwtTokenAsync(UserDB user)
        {
            Guid jwtId = Guid.NewGuid();
            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddHours(2);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            };
            
            if (user.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            if (user.IsBanned)
                claims.Add(new Claim(ClaimTypes.Role, "Banned"));
            if (user.IsWhitelisted)
                claims.Add(new Claim(ClaimTypes.Role, "Whitelisted"));
            claims.Add(new Claim("inviteCode", user.InviteCodeId?.ToString() ?? string.Empty));

            List<Guid> accessGroupGuids = user.AccessGroups?.Select(g => g.Id).ToList() ?? new List<Guid>();
            claims.Add(new Claim("accessGroups", string.Join(",", accessGroupGuids)));

            SymmetricSecurityKey key = new SymmetricSecurityKey(_jwtKeyBytes);
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: creds);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            JwtDB jwtDb = new JwtDB
            {
                Id = jwtId,
                UserId = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
                IsWhitelisted = user.IsWhitelisted,
                TokenString = tokenString,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            };
            _jwtRepository.Insert(jwtDb);
            await _jwtRepository.CommitAsync();

            return jwtDb;
        }

        public async Task<Guid> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            bool isValidInviteCode = _inviteCodeService.IsValidInviteCode(request.InviteCode);
            if (!isValidInviteCode)
            {
                throw new InvalidOperationException("Invalid or expired invite code.");
            }

            bool exists = await _userRepository.GetAllAsReadOnly()
                .AnyAsync(u => u.Username == request.Username, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (request.InviteCode == Guid.Empty)
            {
                throw new InvalidOperationException("Invite code is required.");
            }

            byte[] salt = PasswordUtils.GenerateSalt();
            byte[] hash = PasswordUtils.HashPassword(request.Password, salt);

            UserDB user = new UserDB
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsAdmin = false,
                IsBanned = false,
                IsWhitelisted = false,
                InviteCodeId = request.InviteCode,
            };

            _userRepository.Insert(user);
            await _userRepository.CommitAsync();

            _serviceLogger.WriteLog(UserLogAction.AccountRegistered, $"User {user.Username} registered successfully.", user.Id, null);

            return user.Id;
        }
    }
}