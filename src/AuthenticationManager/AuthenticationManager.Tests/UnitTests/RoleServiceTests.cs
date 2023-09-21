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
    }
}
