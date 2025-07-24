using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class InviteCodeService
    {
        private readonly IIdentifiableRepository<InviteCodeDB> _inviteCodeRepository;
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IUserService _userService;

        public InviteCodeService(
            IIdentifiableRepository<InviteCodeDB> inviteCodeRepo,
            IIdentifiableRepository<UserDB> userRepository,
            IUserService userService)
        {
            _inviteCodeRepository = inviteCodeRepo ?? throw new ArgumentNullException(nameof(inviteCodeRepo));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Guid> CreateNewInviteCode(int maxUses, DateTime expiresAt)
        {
            UserDB user = _userService.GetSessionUserDB();
            InviteCodeDB inviteCode = new InviteCodeDB
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                MaxUses = maxUses
            };
            _inviteCodeRepository.Insert(inviteCode);
            await _inviteCodeRepository.CommitAsync();
            return inviteCode.Id;
        }

        public int GetHowManyUses(Guid inviteCode)
        {
            int usedCount = _userRepository.GetAllAsReadOnly().Count(u => u.InviteCode == inviteCode);
            return usedCount;
        }

        public async Task<bool> IsValidInviteCode(Guid inviteCodeId)
        {
            InviteCodeDB inviteCode = await _inviteCodeRepository.GetByIdAsync(inviteCodeId);
            if (inviteCode == null)
            {
                return false;
            }
            if (inviteCode.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }
            int usedCount = GetHowManyUses(inviteCodeId);
            int remainingUses = inviteCode.MaxUses - usedCount;
            return remainingUses > 0;
        }
    }
}
