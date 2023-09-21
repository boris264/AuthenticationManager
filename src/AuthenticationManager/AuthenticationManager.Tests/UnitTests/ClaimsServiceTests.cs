using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Serivces.ClaimServices.Implementation;
using AuthenticationManager.Services.ClaimServices.Interfaces;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class ClaimsServiceTests
    {
        private IClaimService _claimsService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _claimsService = new ClaimService(RepositoryContainer.repository, null);
        }

        [TearDown]
        public void TearDown()
        {
            var userClaims = RepositoryContainer.repository.All<UserClaim>()
                .Where(uc => uc.Claim.Name != "Username" && uc.Claim.Name != "Email")
                .ToList();

            foreach (var uc in userClaims)
            {
                RepositoryContainer.repository.Remove(uc);
            }
        }

        [Test]
        public async Task CheckGetUserClaimsReturnsAllClaimsForUserByUsername()
        {
            var claims = await _claimsService.GetUserClaims("Latime");

            Assert.That(claims, Is.Not.Null);
            Assert.That(claims.Count == 2);
            Assert.That(claims.Any(c => c.Name == "Username"));
            Assert.That(claims.Any(c => c.Name == "Email"));
        }

        [Test]
        public async Task CheckGetUserClaimsReturnsAllClaimsForUserByGuid()
        {
            var claims = await _claimsService.GetUserClaims(Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1"));

            Assert.That(claims, Is.Not.Null);
            Assert.That(claims.Count == 2);
            Assert.That(claims.Any(c => c.Name == "Username"));
            Assert.That(claims.Any(c => c.Name == "Email"));
        }

        [Test]
        public async Task CheckAddClaimToUserByUsernameIsSuccessfull()
        {
            var result = await _claimsService.AddClaimToUser(new Claim() { Name = "TestClaim", Value = "TestValue" }, "Latime");
            var userClaims = await _claimsService.GetUserClaims("Latime");
           
            Assert.That(result.Success);
            Assert.That(userClaims.Count == 3);
            Assert.That(userClaims.Any(c => c.Name == "TestClaim" && c.Value == "TestValue"));
        }

        [Test]
        public async Task CheckAddClaimToUserByGuidIsSuccessfull()
        {
            var result = await _claimsService.AddClaimToUser(new Claim() 
                { 
                    Name = "TestClaim", 
                    Value = "TestValue" 
                }, Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1"));
            var userClaims = await _claimsService.GetUserClaims(Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1"));
           
            Assert.That(result.Success);
            Assert.That(userClaims.Count == 3);
            Assert.That(userClaims.Any(c => c.Name == "TestClaim" && c.Value == "TestValue"));
        }

        [Test]
        public async Task CheckAddClaimsToUserIsSuccessfull()
        {
            var result = await _claimsService.AddClaimsToUser(new List<Claim>() {
               new Claim() 
                { 
                    Name = "TestClaim", 
                    Value = "TestValue" 
                },
                new Claim()
                {
                    Name = "Another Test Claim",
                    Value = "Too Claim to test"
                } 
            }, Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1"));

            var userClaims = await _claimsService.GetUserClaims(Guid.Parse("47977245-3e5f-4155-aa48-8321efd198a1"));
           
            Assert.That(result.Success);
            Assert.That(userClaims.Count == 4);
            Assert.That(userClaims.Any(c => c.Name == "TestClaim" && c.Value == "TestValue"));
            Assert.That(userClaims.Any(c => c.Name == "Another Test Claim" && c.Value == "Too Claim to test"));
        }
    }
}
