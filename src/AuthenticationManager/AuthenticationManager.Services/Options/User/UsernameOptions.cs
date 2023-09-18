namespace AuthenticationManager.Services.Options.User
{
    public class UsernameOptions
    {
        public string AllowedCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public int MinimumLength { get; set; } = 4;
    }
}