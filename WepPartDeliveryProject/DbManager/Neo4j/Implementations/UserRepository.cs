using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class UserRepository : GeneralRepository<User>, IUserRepository
    {
        public UserRepository(IGraphClient DbContext) : base(DbContext)
        {
        }

        public Dictionary<string, byte> UserRolePriority { get; } = new Dictionary<string, byte>
        {
            {"User", 0},
            {"Client", 1},
            {"KitchenWorker", 2},
            {"Admin", 100},
        };

        private string GetMaxPriorityRole(List<string> roles)
        {
            var maxPriRole = UserRolePriority.First(h=>h.Value == 0).Key;

            foreach (var role in roles)
            {
                if(UserRolePriority.TryGetValue(role, out _))
                {
                    if (UserRolePriority[role] > UserRolePriority[maxPriRole])
                    {
                        maxPriRole = role;
                    }
                }
                else
                {
                    break;
                }
            }

            return maxPriRole;
        }

        public async Task<List<string>> GetUserRoles(string userId)
        {
            var result = await dbContext.Cypher
                .Match($"(node:{typeof(User).Name} {{Id: $id}})")
                .WithParams(new
                {
                    id = userId,
                })
                .ReturnDistinct<List<string>>("labels(node)")
                .ResultsAsync;

            var clearResult = result.First().ToList();

            return clearResult;
        }
    }
}
