using Konscious.Security.Cryptography;

namespace DbManager.Services
{
    public class PasswordService : IPasswordService
    {
        public byte[] GetPasswordHash(string password)
        {
            byte[] passBytes = password.Select(h => ((byte)h)).ToArray();
            var argon2 = new Argon2i(passBytes);
            argon2.DegreeOfParallelism = 16;
            argon2.MemorySize = 4096;
            argon2.Iterations = 40;

            var hash = argon2.GetBytes(128);

            return hash;
        }

        public bool CheckPassword(string password, byte[] hashFromDb)
        {
            var passBytes = GetPasswordHash(password);

            return hashFromDb.SequenceEqual(passBytes);
        }
    }
}
