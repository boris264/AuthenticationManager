using System.Linq;
using AuthenticationManager.Services.Common;
using AuthenticationManager.Services.Interfaces;
using AuthenticationManager.Services.Options.User;
using Microsoft.Extensions.Options;

namespace AuthenticationManager.Services.Implementation
{
    public class PasswordValidator : IPasswordValidator
    {
        private readonly PasswordOptions _passwordOptions;

        private const string _specialChars = "!@#$%^&*()<>?/\\";

        public PasswordValidator(IOptions<PasswordOptions> passwordOptions)
        {
            _passwordOptions = passwordOptions.Value;
        }
        
        public OperationResult ValidatePassword(string password)
        {
            OperationResult operationResult = new();

            if (string.IsNullOrEmpty(password))
            {
                operationResult.Invalid("Password cannot be null!");
                return operationResult;
            }

            if (!CheckPasswordLength(password))
            {
                operationResult.Invalid($"Password should be atleast {_passwordOptions.MinimumLength} characters long!");
            }

            if (_passwordOptions.ShouldContainLowercase)
            {
                if (!CheckLowercase(password))
                {
                    operationResult.Invalid("Password should have atleast one lowercase character!");
                }
            }

            if (_passwordOptions.ShouldContainUppercase)
            {
                if (!CheckUppercase(password))
                {
                    operationResult.Invalid("Password should have atleast one uppercase character!");
                }
            }

            if (_passwordOptions.ShouldContainNumbers)
            {
                if (!CheckContainsNumber(password))
                {
                    operationResult.Invalid("Password should have atleast one number!");
                }
            }

            if (_passwordOptions.ShouldContainSpecialCharacters)
            {
                if (!CheckSpecialCharacters(password))
                {
                    operationResult.Invalid("Password should have atleast one special character!");
                }
            }

            return operationResult;
        }

        private bool CheckPasswordLength(string password)
        {
            return password.Length >= _passwordOptions.MinimumLength;
        }

        private bool CheckLowercase(string password)
        {
            return password.Any(char.IsLower);
        }

        private bool CheckUppercase(string password)
        {
            return password.Any(char.IsUpper);
        }

        private bool CheckContainsNumber(string password)
        {
            return password.Any(char.IsNumber);
        }

        private bool CheckSpecialCharacters(string password)
        {
            return password.Any(_specialChars.Contains);
        }
    }
}
