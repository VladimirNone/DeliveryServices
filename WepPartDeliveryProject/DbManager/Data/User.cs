
namespace DbManager.Data
{
    public abstract class User : Model
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public DateTime? Born { get; set; }
    }
}
