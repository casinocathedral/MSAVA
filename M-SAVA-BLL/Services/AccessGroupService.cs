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

        public async Task<Guid> CreateAccessGroupAsync(string name, int maxUses)
        {
            UserDB user = _userService.GetSessionUser();
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

            await AddAccessGroupToUserAsync(accessGroup, user);

            return accessGroup.Id;
        }

        private async Task AddAccessGroupToUserAsync(AccessGroupDB accessGroup, UserDB user)
        {
            if (accessGroup.Users == null)
            {
                accessGroup.Users = new List<UserDB>();
            }
            if (accessGroup.Users.Any(u => u.Id == user.Id))
            {
                throw new InvalidOperationException($"User with ID {user.Id} is already in AccessGroup with ID {accessGroup.Id}.");
            }
            accessGroup.Users.Add(user);

            if (user.AccessGroups == null)
            {
                user.AccessGroups = new List<AccessGroupDB>();
            }
            if (user.AccessGroups.Any(ag => ag.Id == accessGroup.Id))
            {
                throw new InvalidOperationException($"AccessGroup with ID {accessGroup.Id} is already assigned to User with ID {user.Id}.");
            }
            user.AccessGroups.Add(accessGroup);

            _accessGroupRepository.Update(accessGroup);
            await _accessGroupRepository.CommitAsync();

            _userRepository.Update(user);
            await _userRepository.CommitAsync();
        }
    }
}
