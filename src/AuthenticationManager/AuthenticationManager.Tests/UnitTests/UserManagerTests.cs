using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class UserManagerTests
    {
        [Test]
        public async Task Test()
        {
            var user = await RepositoryContainer.repository.FindByGuidAsync<User>(Guid.Parse("ad766f82-6d24-4d0e-87f3-66059017c278"));

            Assert.That(user.Id, Is.EqualTo(new Guid("ad766f82-6d24-4d0e-87f3-66059017c278")));
        }
    }
}
