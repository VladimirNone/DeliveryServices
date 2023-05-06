
namespace DbManager.Data
{
    public class User : Node
    {
        public string Login { get; set; }
        public List<byte> PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public DateTime? Born { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime RefreshTokenCreated { get; set; }
        public bool IsBlocked { get; set; }
    }
}
