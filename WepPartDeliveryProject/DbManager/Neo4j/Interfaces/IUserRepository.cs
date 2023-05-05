using DbManager.Data;
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
    }
}
