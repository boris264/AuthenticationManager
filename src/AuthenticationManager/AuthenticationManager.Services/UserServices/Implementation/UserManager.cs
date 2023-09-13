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
using AuthenticationManager.Services.ClaimServices.Interfaces;
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

        private readonly IClaimService _claimService;

        public UserManager(IAuthManagerRepository repository,
                           IPasswordHasher passwordHasher,
                           ILogger<UserManager<TUser>> logger,
                           IHttpContextAccessor httpContextAccessor,
                           IClaimService claimService)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _claimService = claimService;
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

                await _claimService.AddClaimToUser(new Data.Models.Claim() {
                    Name = "Username",
                    Value = user.Username
                }, newUser.Username);
                await _claimService.AddClaimToUser(new Data.Models.Claim() {
                    Name = "Email",
                    Value = user.Email
                }, newUser.Username);
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
                    await SetUser(user);

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

        private async Task SetUser(TUser user)
        {
            if (user != null)
            {
                ClaimsIdentity userIdentity = new ClaimsIdentity("AuthManagerCookie");
                
                try
                {
                    var userClaims = await _claimService.GetUserClaims(user.Username);

                    foreach (var claim in userClaims)
                    {
                        userIdentity.AddClaim(new System.Security.Claims.Claim(claim.Name, claim.Value));
                    }

                    ClaimsPrincipal userClaimsPrincipal = new ClaimsPrincipal(userIdentity);

                    _httpContextAccessor.HttpContext.User = userClaimsPrincipal;
                }
                catch (ArgumentNullException)
                {
                    _logger.LogError("User is null!");
                }
            }
        }
    }
}