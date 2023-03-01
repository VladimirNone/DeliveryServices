
namespace DbManager.Services
{
    /// <summary>
    /// Service for generate argon2i hash
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Get Argon2i hash from password
        /// </summary>
        /// <param name="salt">string, which will be used as salt</param>
        /// <param name="password">string, which will be hashed</param>
        /// <returns></returns>
        byte[] GetPasswordHash(string salt, string password);
        /// <summary>
        /// Equels hash password with old hash from Db
        /// </summary>
        /// <param name="salt">string, which will be used as salt</param>
        /// <param name="password">string, which will be hashed</param>
        /// <param name="hashFromDb">hash old password</param>
        /// <returns></returns>
        bool CheckPassword(string salt, string password, byte[] hashFromDb);
    }
}
