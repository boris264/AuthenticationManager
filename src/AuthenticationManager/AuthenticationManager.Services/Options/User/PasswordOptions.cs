namespace AuthenticationManager.Services.Options.User
{
    public class PasswordOptions
    {
        public bool ShouldContainLowercase { get; set; } = true;

        public bool ShouldContainUppercase { get; set; } = false;

        public bool ShouldContainNumbers { get; set; } = false;

        public bool ShouldContainSpecialCharacters { get; set; } = false;

        public int MinimumLength { get; set; } = 8;
    }
}
