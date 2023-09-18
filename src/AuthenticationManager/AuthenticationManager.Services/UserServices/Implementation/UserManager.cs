using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationManager.Authentication.Common;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Data.Repositories;
using AuthenticationManager.Services.Authentication.Constants;
using AuthenticationManager.Services.Authentication.PasswordHashers.Interfaces;
using AuthenticationManager.Services.ClaimServices.Interfaces;
using AuthenticationManager.Services.Common;
using AuthenticationManager.Services.Interfaces;
using AuthenticationManager.Services.UserServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserValidator<TUser> _userValidator;

        private readonly IPasswordValidator _passwordValidator;

        public UserManager(IAuthManagerRepository repository,
                           IPasswordHasher passwordHasher,
                           ILogger<UserManager<TUser>> logger,
                           IHttpContextAccessor httpContextAccessor,
                           IClaimService claimService,
                           IUserValidator<TUser> userValidator,
                           IPasswordValidator passwordValidator)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _claimService = claimService;
            _userValidator = userValidator;
            _passwordValidator = passwordValidator;
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

            OperationResult userValidationResult = _userValidator.ValidateUser(newUser);

            if (!userValidationResult.Success)
            {
                authenticationResult.ErrorMessages.AddRange(userValidationResult.ErrorMessages);
                authenticationResult.Result = AuthenticationState.Failure;

                return authenticationResult;
            }

            OperationResult passwordValidationResult = _passwordValidator.ValidatePassword(newUser.Password);

            if (!passwordValidationResult.Success)
            {
                authenticationResult.ErrorMessages.AddRange(passwordValidationResult.ErrorMessages);
                authenticationResult.Result = AuthenticationState.Failure;

                return authenticationResult;
            }

            newUser.Password = _passwordHasher.Hash(newUser.Password);
            await _repository.AddAsync(newUser);

            try
            {
                await _repository.SaveChangesAsync();
                authenticationResult.Result = AuthenticationState.Success;

                List<Data.Models.Claim> claims = new List<Data.Models.Claim>() {
                    new Data.Models.Claim() { Name = ClaimNames.Username, Value = newUser.Username},
                    new Data.Models.Claim() { Name = ClaimNames.Email, Value = newUser.Email}
                };

                await _claimService.AddClaimsToUser(claims, newUser.Username);
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