using AuthenticationManager.Services.Implementation;
using AuthenticationManager.Services.Interfaces;
using AuthenticationManager.Services.Options.User;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class PasswordValidatorTests
    {
        private PasswordOptions _passwordOptions;

        private IPasswordValidator _passwordValidator;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _passwordOptions = new PasswordOptions();

            _passwordValidator = new PasswordValidator(Options.Create(_passwordOptions));
        }

        [TearDown]
        public void TearDown()
        {
            _passwordOptions = new PasswordOptions();
            _passwordValidator = new PasswordValidator(Options.Create(_passwordOptions));
        }

        [Test]
        public void CheckPasswordValidationForMinimumLength()
        {
            string password = "testpw";
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(!operationResult.Success);
        }

        [Test]
        public void CheckPasswordValidationIsValid()
        {
            string password = "testpw12345";
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(operationResult.Success);
        }

        [Test]
        public void CheckPasswordValidationReturnsErrorOnNoUppercaseChars()
        {
            _passwordOptions.ShouldContainUppercase = true;

            string password = "testpw12345";
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one uppercase character!"));
        }

        [Test]
        public void CheckPasswordValidationReturnsErrorOnNoNumbers()
        {
            _passwordOptions.ShouldContainNumbers = true;

            string password = "testpwAcawAW";
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one number!"));
        }

        [Test]
        public void CheckPasswordValidationReturnsErrorOnNoSpecialChars()
        {
            _passwordOptions.ShouldContainSpecialCharacters = true;

            string password = "testpwAcawAW";
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one special character!"));
        }

        [Test]
        public void CheckPasswordValidationReturnsErrorOnEmptyPassword()
        {
            string password = string.Empty;
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Password cannot be null!"));
        }

        [Test]
        public void CheckPasswordValidationReturnsEveryError()
        {
            _passwordOptions.ShouldContainSpecialCharacters = true;
            _passwordOptions.ShouldContainNumbers = true;
            _passwordOptions.ShouldContainLowercase = true;
            _passwordOptions.ShouldContainUppercase = true;

            string password = ".";
            var operationResult = _passwordValidator.ValidatePassword(password);
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one special character!"));
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one number!"));
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one uppercase character!"));
            Assert.That(operationResult.ErrorMessages.Contains("Password should have atleast one lowercase character!"));
        }
    }
}
