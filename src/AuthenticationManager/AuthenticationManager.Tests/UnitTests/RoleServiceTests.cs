using System;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Services.Implementation;
using AuthenticationManager.Services.Interfaces;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class RoleServiceTests
    {
        private IRoleService _roleService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _roleService = new RoleService(RepositoryContainer.repository, null);
        }

        [TearDown]
        public void TearDown()
        {
            var roles = RepositoryContainer.repository.All<Role>()
                .Where(role => role.Name != "Administrator" && role.Name != "User")
                .ToList();

            foreach (var role in roles)
            {
                RepositoryContainer.repository.Remove(role);
            }

            var userRoles = RepositoryContainer.repository.All<UserRole>()
                .Where(ur => ur.Role.Name != "Administrator" && ur.Role.Name != "User")
                .ToList();

            foreach (var userRole in userRoles)
            {
                RepositoryContainer.repository.Remove(userRole);
            }
        }

        [Test]
        public async Task CheckAddRoleIsSuccessfull()
        {
            var operationResult = await _roleService.AddRole("TestRole");
            var roleExists = RepositoryContainer.repository.All<Role>()
                .Any(r => r.Name == "TestRole");

            Assert.That(roleExists);
        }

        [Test]
        public async Task CheckGetRoleIsSuccessfull()
        {
            var role = await _roleService.GetRole("Administrator");

            Assert.That(role, Is.Not.Null);
            Assert.That(role.Name == "Administrator");
        }

        [Test]
        public async Task CheckAddUserToRoleIsSuccessfull()
        {
            var operationResult = await _roleService.AddUserToRole("Latime", "Administrator");
            var user = RepositoryContainer.repository.All<User>()
                .Where(u => u.Username == "Latime")
                .FirstOrDefault();

            Assert.That(operationResult.Success);
            Assert.That(user.UserRoles.Any(ur => ur.Role.Name == "Administrator"));
        }

        [Test]
        public async Task CheckAddRoleReturnsErrorForAlreadyExistingRole()
        {
            var operationResult = await _roleService.AddRole("Administrator");
        
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Role already exists!"));
        }

        [Test]
        public async Task CheckAddUserToRoleReturnsErrorForNotExistingUser()
        {
            var operationResult = await _roleService.AddUserToRole("Not found user", "Administrator");
        
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("User Not found user was not found!"));
        }

        [Test]
        public async Task CheckAddUserToRoleReturnsErrorForNotExistingRole()
        {
            var operationResult = await _roleService.AddUserToRole("Latime", "Invalid role");
        
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("Role Invalid role doesn't exist!"));
        }

        [Test]
        public async Task CheckAddUserToRoleReturnsErrorForEmptyArguments()
        {
            var operationResult = await _roleService.AddUserToRole("", "");
        
            Assert.That(!operationResult.Success);
        }

        [Test]
        public async Task CheckAddUserToRoleReturnsErrorIfUserAlreadyInRole()
        {
            var operationResult = await _roleService.AddUserToRole("Latime", "User");
        
            Assert.That(!operationResult.Success);
            Assert.That(operationResult.ErrorMessages.Contains("User Latime is already in role User!"));
        }

        [Test]
        public async Task CheckGetRoleReturnsNullOnInvalidRole()
        {
            var role = await _roleService.GetRole("Invalid");
        
            Assert.That(role, Is.Null);
        }
    }
}
