using AuthenticationManager.Data.Models;
using AuthenticationManager.Services.Common;
using AuthenticationManager.Services.Interfaces;
using AuthenticationManager.Services.Options.User;
using Microsoft.Extensions.Options;

namespace AuthenticationManager.Services.Implementation
{
    public class UserValidator<TUser> : IUserValidator<TUser>
        where TUser : User
    {
        private readonly UsernameOptions _usernameOptions;

        private readonly EmailOptions _emailOptions;

        public UserValidator(IOptions<UsernameOptions> usernameOptions,
                             IOptions<EmailOptions> emailOptions)
        {
            _usernameOptions = usernameOptions.Value;
            _emailOptions = emailOptions.Value;
        }
        

        public OperationResult ValidateUser(TUser user)
        {
            OperationResult operationResult = new();

            if (user == null)
            {
                operationResult.Invalid("User cannot be null!");
                return operationResult;
            }

            string username = user.Username;

            if (string.IsNullOrEmpty(username))
            {
                operationResult.Invalid("Username is empty!");
                return operationResult;
            }

            if (!CheckUsernameLength(username))
            {
                operationResult.Invalid($"Username should be atleast {_usernameOptions.MinimumLength} characters long!");
            }

            if (!CheckAllowedCharacters(username))
            {
                operationResult.Invalid($"Username contained an invalid character! Allowed characters are: {_usernameOptions.AllowedCharacters}");
            }

            return operationResult;
        }

        private bool CheckUsernameLength(string username)
        {
            return username.Length >= _usernameOptions.MinimumLength;
        }

        private bool CheckAllowedCharacters(string username)
        {
            foreach (var character in username)
            {
                if (!_usernameOptions.AllowedCharacters.Contains(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
