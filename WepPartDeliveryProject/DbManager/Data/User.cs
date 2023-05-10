using Neo4jClient;

namespace DbManager.Data
{
    public class User : Node
    {
        public string Login { get; set; }
        public List<byte> PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        [Neo4jDateTime]
        public DateTime? Born { get; set; }
        public Guid RefreshToken { get; set; }
        [Neo4jDateTime]
        public DateTime RefreshTokenCreated { get; set; }
        public bool IsBlocked { get; set; }
    }
}
