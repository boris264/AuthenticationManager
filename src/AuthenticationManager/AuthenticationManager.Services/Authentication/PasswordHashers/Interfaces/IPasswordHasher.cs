namespace AuthenticationManager.Services.Authentication.PasswordHashers.Interfaces
{
    public interface IPasswordHasher
    {
        public string Hash(string password);

        public bool VerifyPassword(string hash, string password);
    }
}