using AuthenticationManager.Services.Authentication.PasswordHashers.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace AuthenticationManager.Services.Authentication.PasswordHashers.Implementation
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            string hashedPassword = BC.HashPassword(password);

            return hashedPassword;
        }

        public bool VerifyPassword(string hash, string password)
        {
            return BC.Verify(password, hash);
        }
    }
}