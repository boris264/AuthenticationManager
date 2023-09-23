using AuthenticationManager.Data.Models;
using AuthenticationManager.Services.Implementation;
using AuthenticationManager.Services.Interfaces;
using AuthenticationManager.Services.Options.User;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class UserValidatorTests
    {
        private IUserValidator<User> _userValidator;

        private UsernameOptions _usernameOptions;

        private EmailOptions _emailOptions;


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _usernameOptions = new UsernameOptions();
            _emailOptions = new EmailOptions();
            _userValidator = new UserValidator<User>(Options
                .Create(_usernameOptions),
                        Options.Create(_emailOptions));
        }

        [Test]
        public void CheckUserValidatorAgainstNullUser()
        {
            var operationResult = _userValidator.ValidateUser(null);

            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("User cannot be null!"));
        }

        [Test]
        public void CheckUserValidatorAgainstEmptyUsername()
        {
            User user = new User()
            {
                Email = "TestEmail@google.com",
                Password = "Test123333"
            };

            var operationResult = _userValidator.ValidateUser(user);

            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Username is empty!"));
        }

        [Test]
        public void CheckUserValidatorAgainstWrongMinimumCharacters()
        {
            User user = new User()
            {
                Username = "use",
                Email = "TestEmail@google.com",
                Password = "Test123333"
            };

            var operationResult = _userValidator.ValidateUser(user);

            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains($"Username should be atleast {_usernameOptions.MinimumLength} characters long!"));
        }

        [Test]
        public void CheckUserValidatorAgainstWrongAllowedCharacters()
        {
            User user = new User()
            {
                Username = "usee@!",
                Email = "TestEmail@google.com",
                Password = "Test123333"
            };

            var operationResult = _userValidator.ValidateUser(user);

            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains($"Username contained an invalid character! Allowed characters are: {_usernameOptions.AllowedCharacters}"));
        }

        [Test]
        public void CheckUserValidatorForDisallowedCharacterAndSmallerLength()
        {
            User user = new User()
            {
                Username = "u@!",
                Email = "TestEmail@google.com",
                Password = "Test123333"
            };

            var operationResult = _userValidator.ValidateUser(user);

            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains($"Username contained an invalid character! Allowed characters are: {_usernameOptions.AllowedCharacters}"));
            Assert.That(operationResult.ErrorMessages.Contains($"Username should be atleast {_usernameOptions.MinimumLength} characters long!"));
        }

        [Test]
        public void CheckUserValidatorForValidUser()
        {
            User user = new User()
            {
                Username = "testUsername",
                Email = "TestEmail@google.com",
                Password = "Test123333"
            };

            var operationResult = _userValidator.ValidateUser(user);

            Assert.That(operationResult.Success);
        }
    }
}
