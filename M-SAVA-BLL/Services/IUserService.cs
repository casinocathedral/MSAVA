using M_SAVA_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public interface IUserService
    {
        UserDB GetUserById(Guid id);
        Task<UserDB> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        List<UserDB> GetAllUsers();
        void DeleteUser(Guid id);
        Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
        UserDB GetSessionUser();
    }
}
