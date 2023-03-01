using Konscious.Security.Cryptography;

namespace DbManager.Services
{
    public class PasswordService : IPasswordService
    {
        public byte[] GetPasswordHash(string salt, string password)
        {
            var argon2 = new Argon2i(password.Select(h => ((byte)h)).ToArray());
            argon2.Salt = salt.Select(h => ((byte)h)).ToArray();
            argon2.DegreeOfParallelism = 16;
            argon2.MemorySize = 4096;
            argon2.Iterations = 40;

            var hash = argon2.GetBytes(128);

            return hash;
        }

        public bool CheckPassword(string salt, string password, byte[] hashFromDb)
        {
            var passBytes = GetPasswordHash(salt, password);

            return hashFromDb.SequenceEqual(passBytes);
        }
    }
}
