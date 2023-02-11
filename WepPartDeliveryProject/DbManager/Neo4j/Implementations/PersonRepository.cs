using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class PersonRepository : IPersonRepository
    {
        private INeo4jDataAccess _neo4jDataAccess;

        private ILogger<PersonRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRepository"/> class.
        /// </summary>
        public PersonRepository(INeo4jDataAccess neo4jDataAccess, ILogger<PersonRepository> logger)
        {
            _neo4jDataAccess = neo4jDataAccess;
            _logger = logger;
        }

        /// <summary>
        /// Searches the name of the person.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> SearchPersonsByName(string searchString)
        {
            var query = @"MATCH (p:Person) WHERE toUpper(p.name) CONTAINS toUpper($searchString) 
                                RETURN p{ name: p.name, born: p.born } ORDER BY p.Name LIMIT 5";

            IDictionary<string, object> parameters = new Dictionary<string, object> { { "searchString", searchString } };

            var persons = await _neo4jDataAccess.ExecuteReadDictionaryAsync(query, "p", parameters);

            return persons;
        }

        /// <summary>
        /// Adds a new person
        /// </summary>
        public async Task<bool> AddPerson(DeliveryMan person)
        {
            if (person != null && !string.IsNullOrWhiteSpace(person.Name))
            {
                var query = @"MERGE (p:Person {name: $name}) ON CREATE SET p.born = $born 
                            ON MATCH SET p.born = $born, p.updatedAt = timestamp() RETURN true";
                IDictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "name", person.Name },
                    { "born", person.Born.ToShortDateString() }
                };
                return await _neo4jDataAccess.ExecuteWriteTransactionAsync<bool>(query, parameters);
            }
            else
            {
                throw new System.ArgumentNullException(nameof(person), "Person must not be null");
            }
        }

        /// <summary>
        /// Get count of persons
        /// </summary>
        public async Task<long> GetPersonCount()
        {
            var query = @"Match (p:Person) RETURN count(p) as personCount";
            var count = await _neo4jDataAccess.ExecuteReadScalarAsync<long>(query);
            return count;
        }
    }
}
