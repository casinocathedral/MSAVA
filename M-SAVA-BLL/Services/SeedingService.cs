using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Repositories;
using M_SAVA_INF.Environment;
using System;
using System.Linq;
using M_SAVA_BLL.Services.Interfaces;

namespace M_SAVA_BLL.Services
{
    public class SeedingService : ISeedingService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepo;
        private readonly IIdentifiableRepository<InviteCodeDB> _inviteCodeRepo;
        private readonly ILocalEnvironment _env;

        public SeedingService(
            IIdentifiableRepository<UserDB> userRepo,
            IIdentifiableRepository<InviteCodeDB> inviteCodeRepo,
            ILocalEnvironment env)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _inviteCodeRepo = inviteCodeRepo ?? throw new ArgumentNullException(nameof(inviteCodeRepo));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public void Seed()
        {
            SeedAdminUser();
        }

        private Guid SeedAdminUser()
        {
            string adminUsername = _env.GetValue("admin_username");
            string adminPassword = _env.GetValue("admin_password");

            UserDB? adminUser = _userRepo.GetAllAsReadOnly().FirstOrDefault(u => u.Username == adminUsername);
            if (adminUser == null)
            {
                byte[] salt = PasswordUtils.GenerateSalt();
                byte[] hash = PasswordUtils.HashPassword(adminPassword, salt);
                adminUser = new UserDB
                {
                    Id = Guid.NewGuid(),
                    Username = adminUsername,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsAdmin = true,
                    IsBanned = false,
                    IsWhitelisted = true,
                    CreatedAt = DateTime.UtcNow
                };
                _userRepo.Insert(adminUser);
                _userRepo.Commit();
            }
            return adminUser.Id;
        }
    }
}
