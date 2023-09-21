using System;
using System.Threading.Tasks;
using System.Linq;
using AuthenticationManager.Data.Models;
using System.Collections.Generic;
using AuthenticationManager.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using System.IO;
using System.Text.Json;

namespace AuthenticationManager.Tests.UnitTests
{
    [SetUpFixture]
    public static class RepositoryContainer
    {
        private static List<User> _users = new List<User>();

        private static List<Role> _roles = new List<Role>();

        private static List<Claim> _claims = new List<Claim>();

        public static IAuthManagerRepository repository; 

        [OneTimeSetUp]
        public static async Task Setup()
        {
            repository = Substitute.For<IAuthManagerRepository>();
            await ParseUsersJson();
            ParseRolesFromUsers();
            ParseClaimsFromUsers();

            ConfigureMethodAll();
            ConfigureMethodAdd();
            ConfigureMethodFindByGuid();

            repository.SaveChangesAsync().Returns(1);
        }

        private static void ConfigureMethodAdd()
        {
            repository.AddAsync(Arg.Any<User>()).Returns(u => AddUser(u.ArgAt<User>(0)));
            repository.AddAsync(Arg.Any<Role>()).Returns(r => AddRole(r.ArgAt<Role>(0)));
            repository.AddAsync(Arg.Any<UserClaim>()).Returns(r => AddUserClaim(r.ArgAt<UserClaim>(0)));
            repository.AddAsync(Arg.Any<UserRole>()).Returns(r => AddUserRole(r.ArgAt<UserRole>(0)));
        }

        private static void ConfigureMethodFindByGuid()
        {
            repository.FindByGuidAsync<User>(Arg.Any<Guid>())
                .Returns(info => FindByGuid<User>(info.ArgAt<Guid>(0)));
            repository.FindByGuidAsync<Role>(Arg.Any<Guid>())
                .Returns(info => FindByGuid<Role>(info.ArgAt<Guid>(0)));
            repository.FindByGuidAsync<Claim>(Arg.Any<Guid>())
                .Returns(info => FindByGuid<Claim>(info.ArgAt<Guid>(0)));
        }

        private static void ConfigureMethodAll()
        {
            repository.All<User>().Returns(_users.AsQueryable());
            repository.All<Role>().Returns(_roles.AsQueryable());
            repository.All<Claim>().Returns(_claims.AsQueryable());
            repository.All<UserClaim>().Returns(_users.SelectMany(u => u.UserClaims).AsQueryable());
            repository.All<UserRole>().Returns(_users.SelectMany(u => u.UserRoles).AsQueryable());
        }

        private static T FindByGuid<T>(Guid guid)
            where T : class
        {
            if (typeof(T) == typeof(User))
            {
                return _users.Where(u => u.Id == guid)
                    .FirstOrDefault() as T;
            }
            else if (typeof(T) == typeof(Role))
            {
                return _roles.Where(r => r.Id == guid)
                    .FirstOrDefault() as T;
            }
            else if (typeof(T) == typeof(Claim))
            {
                return _claims.Where(c => c.Id == guid)
                    .FirstOrDefault() as T;
            }
            else
            {
                return default;
            }
        }
        private static Task AddUserRole(UserRole userRole)
        {
            var user = _users.Where(u => u.Id == userRole.UserId)
                .FirstOrDefault();
            
            if (user != null)
            {
                user.UserRoles.Add(userRole);
            }

            return Task.CompletedTask;
        }

        private static Task AddUserClaim(UserClaim userClaim)
        {
            var user = _users.Where(u => u.Id == userClaim.UserId)
                .FirstOrDefault();
            
            if (user != null)
            {
                user.UserClaims.Add(userClaim);
            }

            return Task.CompletedTask;
        }

        private static Task AddRole(Role role)
        {
            _roles.Add(role);
            return Task.CompletedTask;
        }

        private static Task AddUser(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        private static async Task ParseUsersJson()
        {
            Stream usersJsonStream = File.Open("users.json", FileMode.Open);
            _users = await JsonSerializer.DeserializeAsync<List<User>>(usersJsonStream);
        }

        private static void ParseRolesFromUsers()
        {
            foreach (var item in _users)
            {
                _roles.AddRange(item.UserRoles.Select(ur => ur.Role));
            }

            _roles = _roles.DistinctBy(r => r.Id)
                .ToList();
        }

        private static void ParseClaimsFromUsers()
        {
            _claims = _users
                .SelectMany(uc => uc.UserClaims)
                .Select(c => c.Claim)
                .ToList();
        }
    }
}
