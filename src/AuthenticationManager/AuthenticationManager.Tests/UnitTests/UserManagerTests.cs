using System;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Authentication.Common;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Serivces.ClaimServices.Implementation;
using AuthenticationManager.Services.Authentication.Constants;
using AuthenticationManager.Services.Authentication.PasswordHashers.Implementation;
using AuthenticationManager.Services.Implementation;
using AuthenticationManager.Services.Options.User;
using AuthenticationManager.Services.UserServices.Implementation;
using AuthenticationManager.Services.UserServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class UserManagerTests
    {
        private IUserManager<User> _userManager;

        private IHttpContextAccessor _httpContextAccessor;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _httpContextAccessor = new HttpContextAccessor();
            _httpContextAccessor.HttpContext = new DefaultHttpContext();

            _userManager = new UserManager<User>(RepositoryContainer.repository, 
                                                 new BCryptPasswordHasher(),
                                                 null,
                                                 _httpContextAccessor,
                                                 new ClaimService(RepositoryContainer.repository, null),
                                                 new UserValidator<User>(Options.Create(new UsernameOptions()), Options.Create(new EmailOptions())),
                                                 new PasswordValidator(Options.Create(new PasswordOptions())));
        }

        [SetUp]
        public void SetUp()
        {
            _httpContextAccessor.HttpContext = new DefaultHttpContext();
        }

        [Test]
        public async Task CheckGetByUsernameReturnsCorrectUser()
        {
            var user = await _userManager.GetByUsernameAsync("Latime");

            Assert.That(user, Is.Not.Null);
            Assert.That(user.Username, Is.EqualTo("Latime"));
            Assert.That(user.Id, Is.EqualTo(Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1")));
        }

        [Test]
        public async Task CheckGetByUsernameReturnsNull()
        {
            var user = await _userManager.GetByUsernameAsync("InvalidUser");

            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task CheckGetByEmailReturnsCorrectUser()
        {
            var user = await _userManager.GetByEmailAsync("rosanna85@yahoo.com");

            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo("rosanna85@yahoo.com"));
            Assert.That(user.Id, Is.EqualTo(Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1")));
        }

        [Test]
        public async Task CheckGetByEmailReturnsNull()
        {
            var user = await _userManager.GetByEmailAsync("InvalidUser");

            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task CheckUserGetsRegisteredSuccessfully()
        {
            Guid userId = Guid.NewGuid();

            User user = new User() {
                Id = userId,
                Username = "John",
                Email = "John@test.com",
                Password = "John1234"
            };

            var registerResult = await _userManager.RegisterAsync(user);
            var userFromRepository = await RepositoryContainer.repository
                .FindByGuidAsync<User>(userId);

            Assert.That(registerResult.Result, Is.EqualTo(AuthenticationState.Success));
            Assert.That(userFromRepository, Is.Not.Null);
            Assert.That(userFromRepository, Is.EqualTo(user));
        }

        [Test]
        public async Task CheckRegistrationFailsWhenUserIsNull()
        {
            var result = await _userManager.RegisterAsync(null);

            Assert.That(result.Result, Is.EqualTo(AuthenticationState.Failure));
            Assert.That(result.ErrorMessages.Contains("User cannot be null!"));
        }

        [Test]
        public async Task CheckRegistrationFailsWhenUsernameIsEmpty()
        {
            User user = new User() {
                Email = "test@a.com",
                Password = "123456789"
            };

            var result = await _userManager.RegisterAsync(user);

            Assert.That(result.Result, Is.EqualTo(AuthenticationState.Failure));
        }

        [Test]
        public async Task CheckRegistrationFailsWhenPasswordIsEmpty()
        {
            User user = new User() {
                Username = "HelloUser",
                Email = "test@a.com",
            };

            var result = await _userManager.RegisterAsync(user);

            Assert.That(result.Result, Is.EqualTo(AuthenticationState.Failure));
        }

        [Test]
        public async Task CheckClaimsAreAddedToUserAfterRegistration()
        {
            Guid userId = Guid.NewGuid();

            User user = new User() {
                Id = userId,
                Username = "John",
                Email = "John@test.com",
                Password = "John1234"
            };

            await _userManager.RegisterAsync(user);
            var userFromRepository = await RepositoryContainer.repository
                .FindByGuidAsync<User>(userId);

            Assert.That(userFromRepository.UserClaims.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task CheckPasswordIsHashedAfterRegistration()
        {
            Guid userId = Guid.NewGuid();

            User user = new User() {
                Id = userId,
                Username = "John",
                Email = "John@test.com",
                Password = "John1234"
            };
            
            await _userManager.RegisterAsync(user);
            var userFromRepository = await RepositoryContainer.repository
                .FindByGuidAsync<User>(userId);

            Assert.That(userFromRepository.Password, Is.EqualTo(user.Password));
            Assert.That(userFromRepository.Password, Is.Not.EqualTo("John1234"));
        }

        [Test]
        public async Task CheckLoginIsSuccessfull()
        {
            var loginResult = await _userManager.SignInAsync("Latime", "qbkgkybh");

            Assert.That(loginResult.Result, Is.EqualTo(AuthenticationState.Success));
        }

        [Test]
        public async Task CheckLoginFails()
        {
            var loginResult = await _userManager.SignInAsync("invalid", "awdwdawda");

            Assert.That(loginResult.Result, Is.EqualTo(AuthenticationState.Failure));
            Assert.That(loginResult.ErrorMessages.Contains(string.Format("invalid", LoginErrorConstants.UserNotFound)));
        }

        [Test]
        public async Task CheckLoginFailReturnsCorrectMessageOnInvalidPassword()
        {
            var loginResult = await _userManager.SignInAsync("Latime", "awdwdawda");

            Assert.That(loginResult.Result, Is.EqualTo(AuthenticationState.Failure));
            Assert.That(loginResult.ErrorMessages.Contains(LoginErrorConstants.InvalidPassword));
        }

        [Test]
        public async Task CheckHttpContextUserIsSetAfterLogin()
        {
            var loginResult = await _userManager.SignInAsync("Latime", "qbkgkybh");

            Assert.That(loginResult.Result, Is.EqualTo(AuthenticationState.Success));
            Assert.That(_httpContextAccessor.HttpContext.User, Is.Not.Null);
            Assert.That(_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated);
        }

        [Test]
        public async Task CheckHttpContextUserIsAuthenticatedNotSetAfterFailedLogin()
        {
            var loginResult = await _userManager.SignInAsync("jtyjyttr", "dawfesajyyjt");

            Assert.That(loginResult.Result, Is.EqualTo(AuthenticationState.Failure));
            Assert.That(!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated);
        }

        [Test]
        public async Task CheckUserClaimsAreAddedToHttpContextUser()
        {
            var loginResult = await _userManager.SignInAsync("Latime", "qbkgkybh");

            Assert.That(_httpContextAccessor.HttpContext.User.Claims.Count(), Is.EqualTo(2));
        }
    }
}
