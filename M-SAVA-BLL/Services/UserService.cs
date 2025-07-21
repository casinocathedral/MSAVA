using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IIdentifiableRepository<UserDB> userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        // Read
        public UserDB GetUserById(Guid id)
        {
            return _userRepository.GetById(id);
        }

        public UserDB GetSessionUser()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            Guid sessionUserId = new();
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                sessionUserId = sessionDto.UserId;
            } else
            {
                throw new UnauthorizedAccessException("SessionDTO not found in HttpContext.");
            }
            return _userRepository.GetById(sessionUserId);
        }

        public async Task<UserDB> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        public List<UserDB> GetAllUsers()
        {
            return _userRepository.GetAllAsReadOnly().ToList();
        }

        // Delete
        public void DeleteUser(Guid id)
        {
            _userRepository.DeleteById(id);
            _userRepository.Commit();
        }

        public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _userRepository.DeleteByIdAsync(id);
            await _userRepository.CommitAsync();
        }
    }
}