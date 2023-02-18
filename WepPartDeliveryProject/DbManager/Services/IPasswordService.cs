
namespace DbManager.Services
{
    public interface IPasswordService
    {
        byte[] GetPasswordHash(string password);
        bool CheckPassword(string password, byte[] hashFromDb);
    }
}
