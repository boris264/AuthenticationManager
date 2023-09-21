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
using MockQueryable.NSubstitute;
using System.Text.Json.Serialization;
using AuthenticationManager.Services.Authentication.PasswordHashers.Implementation;

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
            // HashUsersPasswords();
            ParseRolesFromUsers();
            ParseClaimsFromUsers();
            ConfigureMethodAll();
            ConfigureMethodAdd();
            ConfigureMethodFindByGuid();
            ConfigureMethodRemove();

            repository.SaveChangesAsync().Returns(1);
        }

        private static void ConfigureMethodAdd()
        {
            repository.AddAsync(Arg.Any<User>()).Returns(u => AddUser(u.ArgAt<User>(0)));
            repository.AddAsync(Arg.Any<Role>()).Returns(r => AddRole(r.ArgAt<Role>(0)));
            repository.AddAsync(Arg.Any<UserClaim>()).Returns(r => AddUserClaim(r.ArgAt<UserClaim>(0)));
            repository.AddAsync(Arg.Any<UserRole>()).Returns(r => AddUserRole(r.ArgAt<UserRole>(0)));
            repository.AddRangeAsync(Arg.Any<ICollection<User>>()).Returns(u => AddRange(u.ArgAt<ICollection<User>>(0)));
            repository.AddRangeAsync(Arg.Any<ICollection<Role>>()).Returns(u => AddRange(u.ArgAt<ICollection<Role>>(0)));
            repository.AddRangeAsync(Arg.Any<ICollection<UserRole>>()).Returns(u => AddRange(u.ArgAt<ICollection<UserRole>>(0)));
            repository.AddRangeAsync(Arg.Any<ICollection<UserClaim>>()).Returns(u => AddRange(u.ArgAt<ICollection<UserClaim>>(0)));
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
            repository.All<User>().Returns(_users.BuildMock());
            repository.All<Role>().Returns(_roles.BuildMock());
            repository.All<Claim>().Returns(_claims.BuildMock());
            repository.All<UserClaim>().Returns(_users.SelectMany(u => u.UserClaims).BuildMock());
            repository.All<UserRole>().Returns(_users.SelectMany(u => u.UserRoles).BuildMock());
        }

        private static void ConfigureMethodRemove()
        {
            repository.When(r => r.Remove<User>(Arg.Any<User>()))
                .Do(f => Remove(f.ArgAt<User>(0)));
            repository.When(r => r.Remove<Role>(Arg.Any<Role>()))
                .Do(f => Remove(f.ArgAt<Role>(0)));
            repository.When(r => r.Remove<Claim>(Arg.Any<Claim>()))
                .Do(f => Remove(f.ArgAt<Claim>(0)));
            repository.When(r => r.Remove<UserClaim>(Arg.Any<UserClaim>()))
                .Do(f => Remove(f.ArgAt<UserClaim>(0)));
        }

        private static Task AddRange<T>(ICollection<T> values)
        {
            if (typeof(T) == typeof(User))
            {
                _users.AddRange(values as ICollection<User>);
            }
            else if (typeof(T) == typeof(Role))
            {
                _roles.AddRange(values as ICollection<Role>);
            }
            else if (typeof(T) == typeof(Claim))
            {
                _claims.AddRange(values as ICollection<Claim>);
            }
            else if (typeof(T) == typeof(UserClaim))
            {
                var userClaims = values as ICollection<UserClaim>;

                foreach (var userClaim in userClaims)
                {
                    userClaim.ClaimId = userClaim.Claim.Id;
                    userClaim.UserId = userClaim.User.Id;
                    
                    var user = FindByGuid<User>(userClaim.User.Id);

                    if (user != null)
                    {
                        user.UserClaims.Add(userClaim);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static void Remove<T>(T entity)
            where T : class
        {
            if (typeof(T) == typeof(User))
            {
                _users.Remove(entity as User);
            }
            else if (typeof(T) == typeof(Role))
            {
                _roles.Remove(entity as Role);
            }
            else if (typeof(T) == typeof(Claim))
            {
                _claims.Remove(entity as Claim);
            }
            else if (typeof(T) == typeof(UserClaim))
            {
                var userClaim = entity as UserClaim;

                var user = FindByGuid<User>(userClaim.User.Id);

                var uc = user.UserClaims
                    .Where(uc => uc.Claim.Id == userClaim.Claim.Id)
                    .FirstOrDefault();
                
                user.UserClaims.Remove(uc);
            }
            else if (typeof(T) == typeof(UserRole))
            {
                var userRole = entity as UserRole;

                var user = FindByGuid<User>(userRole.User.Id);
                
                var ur = user.UserRoles
                    .Where(uc => uc.Role.Id == userRole.Role.Id)
                    .FirstOrDefault();
                
                user.UserRoles.Remove(ur);
            }
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
            var user = _users.Where(u => u.Id == userClaim.User.Id)
                .FirstOrDefault();

            userClaim.ClaimId = userClaim.Claim.Id;
            userClaim.UserId = userClaim.User.Id;
            
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
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            Stream usersJsonStream = File.Open("users.json", FileMode.Open);
            _users = await JsonSerializer.DeserializeAsync<List<User>>(usersJsonStream, jsonSerializerOptions);
        }

        private static void HashUsersPasswords()
        {
            BCryptPasswordHasher bCryptPasswordHasher = new();

            foreach (var user in _users)
            {
                user.Password = bCryptPasswordHasher.Hash(user.Password);
            }
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
