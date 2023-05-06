using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Interfaces
{
    public interface IUserRepository : IGeneralRepository<User>
    {
        Dictionary<string, byte> UserRolePriority { get; }
        /// <summary>
        /// Return max priority role of user. For example, if user has Client and Admin roles, then it return Admin
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>string Role</returns>
        Task<List<string>> GetUserRoles(string userId);

        Task<List<(User, List<string>)>> GetUsersForAdmin(int? skipCount, int? limitCount, params string[] orderByProperty);

        Task<List<User>> SearchUsersByIdAndLogin(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty);

        Task<List<(User, List<string>)>> SearchUsersByIdAndLoginForAdmin(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty);
    }
}

