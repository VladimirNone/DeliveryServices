using DbManager.Data;
using DbManager.Data.DTOs;
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
    public class UserRepository : GeneralNeo4jRepository<User>, IUserRepository
    {
        public UserRepository(BoltGraphClientFactory boltGraphClientFactory, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
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
            var maxPriRole = UserRolePriority.First(h => h.Value == 0).Key;

            foreach (var role in roles)
            {
                if (UserRolePriority.TryGetValue(role, out _))
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
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(GetUserRoles), System.Diagnostics.ActivityKind.Client);
            activity?.SetTag("provider", "neo4j");

            var cypher = _dbContext.Cypher
                .Match($"(node:{typeof(User).Name} {{Id: $id}})")
                .WithParams(new
                {
                    id = userId,
                })
                .ReturnDistinct<List<string>>("labels(node)");

            activity?.SetTag("cypher.query", cypher.Query.QueryText);

            return (await cypher.ResultsAsync).First().ToList();
        }

        public async Task<List<(User, List<string>)>> GetUsersForAdmin(int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(GetUsersForAdmin), System.Diagnostics.ActivityKind.Client);
            activity?.SetTag("provider", "neo4j");

            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var cypher = _dbContext.Cypher
                .Match($"(node:{typeof(User).Name})")
                .With("node, labels(node) as roles")
                .Return((node, roles) => new
                {
                    user = node.As<User>(),
                    userRoles = roles.As<List<string>>()
                })
                .ChangeQueryForPaginationAnonymousType(orderByProperty, skipCount, limitCount);

            activity?.SetTag("cypher.query", cypher.Query.QueryText);

            return (await cypher.ResultsAsync).Select(h => (h.user, h.userRoles)).ToList();
        }

        public async Task<List<(User, List<string>)>> SearchUsersByIdAndLoginForAdmin(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(SearchUsersByIdAndLoginForAdmin), System.Diagnostics.ActivityKind.Client);
            activity?.SetTag("provider", "neo4j");

            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var cypher = _dbContext.Cypher
                .Match($"(node:{typeof(User).Name})")
                .Where($"toLower(node.Id) contains($searchText) or toLower(node.Login) contains($searchText)")
                .WithParams(new
                {
                    searchText
                })
                .With("node, labels(node) as roles")
                .Return((node, roles) => new
                {
                    user = node.As<User>(),
                    userRoles = roles.As<List<string>>()
                })
                .ChangeQueryForPaginationAnonymousType(orderByProperty, skipCount, limitCount);

            activity?.SetTag("cypher.query", cypher.Query.QueryText);

            return (await cypher.ResultsAsync).Select(h => (h.user, h.userRoles)).ToList();
        }

        public async Task<List<User>> SearchUsersByIdAndLogin(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(SearchUsersByIdAndLogin), System.Diagnostics.ActivityKind.Client);
            activity?.SetTag("provider", "neo4j");

            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var cypher = _dbContext.Cypher
                .Match($"(node:{typeof(User).Name})")
                .Where($"toLower(node.Id) contains($searchText) or toLower(node.Login) contains($searchText)")
                .WithParams(new
                {
                    searchText
                })
                .Return((node) => node.As<User>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount);

            activity?.SetTag("cypher.query", cypher.Query.QueryText);

            return (await cypher.ResultsAsync).ToList();
        }
    }
}
