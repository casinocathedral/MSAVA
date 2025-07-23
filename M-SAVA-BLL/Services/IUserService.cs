using M_SAVA_BLL.Models;
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
        UserDTO GetUserById(Guid id);
        Task<UserDTO> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        List<UserDTO> GetAllUsers();
        void DeleteUser(Guid id);
        Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
        UserDTO GetSessionUser();
        UserDB GetSessionUserDB();
        SessionDTO GetSessionClaims();
    }
}
