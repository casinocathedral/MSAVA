using M_SAVA_Core.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_BLL.Loggers;

namespace M_SAVA_BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _serviceLogger;

        public UserService(IIdentifiableRepository<UserDB> userRepository, IHttpContextAccessor httpContextAccessor, ServiceLogger serviceLogger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public UserDTO GetUserById(Guid id)
        {
            var userDb = _userRepository.GetById(id, u => u.AccessGroups);
            return MappingUtils.MapUserDTOWithRelationships(userDb);
        }

        public Guid GetSessionUserId()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                return sessionDto.UserId;
            }
            else
            {
                throw new UnauthorizedAccessException("SessionDTO not found in HttpContext.");
            }
        }
        public SessionDTO GetSessionClaims()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                return sessionDto;
            }
            else
            {
                throw new UnauthorizedAccessException("Session claims not found in HttpContext.");
            }
        }

        public bool IsSessionUserAdmin()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                return sessionDto.IsAdmin;
            }
            else
            {
                throw new UnauthorizedAccessException("SessionDTO not found in HttpContext.");
            }
        }

        public UserDTO GetSessionUser()
        {
            return MappingUtils.MapUserDTOWithRelationships(GetSessionUserDB());
        }

        public UserDB GetSessionUserDB()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            Guid sessionUserId = new();
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                sessionUserId = sessionDto.UserId;
            }
            else
            {
                throw new UnauthorizedAccessException("SessionDTO not found in HttpContext.");
            }
            var userDb = _userRepository.GetById(sessionUserId, u => u.AccessGroups);
            return userDb;
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userDb = await _userRepository.GetByIdAsync(id, u => u.AccessGroups);
            return MappingUtils.MapUserDTOWithRelationships(userDb);
        }

        public List<UserDTO> GetAllUsers()
        {
            var userDbs = _userRepository.GetAllAsReadOnly().ToList();
            return userDbs.Select(MappingUtils.MapUserDTOWithRelationships).ToList();
        }

        public void DeleteUser(Guid id)
        {
            _userRepository.DeleteById(id);
            _userRepository.Commit();
            _serviceLogger.WriteLog(UserLogAction.AccountDeletion, $"User deleted: {id}", id, null);
        }

        public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _userRepository.DeleteByIdAsync(id);
            await _userRepository.CommitAsync();
            _serviceLogger.WriteLog(UserLogAction.AccountDeletion, $"User deleted: {id}", id, null);
        }
    }
}