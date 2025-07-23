using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class AccessGroupService
    {
        private readonly IIdentifiableRepository<AccessCodeDB> _accessCodeRepository;
        private readonly IIdentifiableRepository<AccessGroupDB> _accessGroupRepository;
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessGroupService(
            IIdentifiableRepository<AccessCodeDB> accessCodeRepo,
            IIdentifiableRepository<AccessGroupDB> accessGroupRepo,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IIdentifiableRepository<UserDB> userRepository)
        {
            _accessCodeRepository = accessCodeRepo ?? throw new ArgumentNullException(nameof(accessCodeRepo));
            _accessGroupRepository = accessGroupRepo ?? throw new ArgumentNullException(nameof(accessGroupRepo));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userRepository = userRepository;
        }

        public async Task<List<AccessGroupDB>> GetUserAccessGroupsAsync(Guid userId)
        {
            UserDB user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            return user.AccessGroups?.ToList() ?? new List<AccessGroupDB>();
        }

        public async Task<Guid> CreateAccessGroupAsync(string name)
        {
            UserDB user = _userService.GetSessionUserDB();
            var now = DateTime.UtcNow;

            var accessGroup = new AccessGroupDB
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                CreatedAt = now,
                Name = name,
                Users = new List<UserDB>(),
                SubGroups = new List<AccessGroupDB>()
            };
            _accessGroupRepository.Insert(accessGroup);
            await _accessGroupRepository.CommitAsync();

            await AddAccessGroupDbToUserDbAsync(accessGroup, user);

            return accessGroup.Id;
        }

        private async Task AddAccessGroupDbToUserDbAsync(AccessGroupDB accessGroup, UserDB user)
        {
            if (accessGroup.Users == null)
            {
                accessGroup.Users = new List<UserDB>();
            }

            if (accessGroup.Users.Any(u => u.Id == user.Id))
            {
                throw new InvalidOperationException($"User with ID {user.Id} is already in AccessGroup with ID {accessGroup.Id}.");
            }

            user.AccessGroups.Add(accessGroup);

            _userRepository.Update(user);
            await _userRepository.CommitAsync();
        }

        private bool IsUserOwnerOrAdmin(UserDB user, AccessGroupDB group)
        {
            return user.IsAdmin || group.OwnerId == user.Id;
        }

        public async Task AddAccessGroupToUserAsync(Guid accessGroupId, Guid userId)
        {
            AccessGroupDB accessGroup = await _accessGroupRepository.GetByIdAsync(accessGroupId);
            if (accessGroup == null)
            {
                throw new KeyNotFoundException($"AccessGroup with ID {accessGroupId} not found.");
            }
            UserDB user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            if (!IsUserOwnerOrAdmin(_userService.GetSessionUserDB(), accessGroup))
            {
                throw new UnauthorizedAccessException("Only the owner or an admin can add users to this access group.");
            }
            if (accessGroup.Users == null)
            {
                accessGroup.Users = new List<UserDB>();
            }
            if (accessGroup.Users.Any(u => u.Id == user.Id))
            {
                throw new InvalidOperationException($"User with ID {user.Id} is already in AccessGroup with ID {accessGroup.Id}.");
            }
            
            await AddAccessGroupDbToUserDbAsync(accessGroup, user);
        }
    }
}
