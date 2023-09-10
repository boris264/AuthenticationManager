using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationManager.Authentication.Common;
using AuthenticationManager.Data.Context;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Data.Repositories;
using AuthenticationManager.Services.Authentication.Constants;
using AuthenticationManager.Services.Authentication.PasswordHashers.Interfaces;
using AuthenticationManager.Services.UserServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace AuthenticationManager.Services.UserServices.Implementation
{
    public class UserManager<TUser> : IUserManager<TUser>
        where TUser : User
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private readonly IAuthManagerRepository _repository;
        
        private readonly IPasswordHasher _passwordHasher;

        private readonly ILogger<UserManager<TUser>> _logger;

        public UserManager(IAuthManagerRepository repository,
                           IPasswordHasher passwordHasher,
                           ILogger<UserManager<TUser>> logger,
                           IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<TUser> GetByEmailAsync(string email)
        {
            return _repository.All<TUser>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public Task<TUser> GetByUsernameAsync(string username)
        {
            return _repository.All<TUser>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<AuthenticationResult> RegisterAsync(TUser user)
        {
            AuthenticationResult authenticationResult = new();
            TUser newUser = user;

            newUser.Password = _passwordHasher.Hash(newUser.Password);
            await _repository.AddAsync(newUser);

            try
            {
                await _repository.SaveChangesAsync();
                authenticationResult.Result = AuthenticationState.Success;
            }
            catch (DbUpdateException)
            {
                authenticationResult.Result = AuthenticationState.Failure;
            }

            return authenticationResult;
        }

        public async Task<AuthenticationResult> SignInAsync(string username, string password, bool rememberMe = false)
        {
            AuthenticationResult authenticationResult = new();
            
            TUser user = await GetByUsernameAsync(username);

            if (user != null)
            {
                if (_passwordHasher.VerifyPassword(user.Password, password))
                {
                    SetUser(user);

                    authenticationResult.Result = AuthenticationState.Success;
                    return authenticationResult;
                }
                else
                {
                    authenticationResult.ErrorMessages.Add(LoginErrorConstants.InvalidPassword);
                    return authenticationResult;
                }
            }

            authenticationResult.ErrorMessages.Add(string.Format(username, LoginErrorConstants.UserNotFound));
            return authenticationResult;
        }

        private bool SetUser(TUser user)
        {
            if (user != null)
            {
                ClaimsIdentity userIdentity = new ClaimsIdentity("AuthManagerCookie");
                
                try
                {
                    foreach (var claim in user.UserClaims.Select(x => x.Claim))
                    {
                        userIdentity.AddClaim(new System.Security.Claims.Claim(claim.Name, claim.Value));
                    }

                    ClaimsPrincipal userClaimsPrincipal = new ClaimsPrincipal(userIdentity);

                    _httpContextAccessor.HttpContext.User = userClaimsPrincipal;

                    return true;
                }
                catch (ArgumentNullException)
                {
                    _logger.LogError("User is null!");
                }
            }

            return false;
        }
    }
}