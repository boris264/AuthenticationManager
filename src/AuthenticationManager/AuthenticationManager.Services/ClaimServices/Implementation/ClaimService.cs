using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Data.Repositories;
using AuthenticationManager.Services.ClaimServices.Interfaces;
using AuthenticationManager.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthenticationManager.Serivces.ClaimServices.Implementation
{
    public class ClaimService : IClaimService
    {
        private readonly IAuthManagerRepository _repository;

        private readonly ILogger<ClaimService> _logger;

        public ClaimService(IAuthManagerRepository repository,
                            ILogger<ClaimService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Claim> GetByGuid(Guid guid)
        {
            return await _repository.FindByGuidAsync<Claim>(guid);
        }

        public async Task<IList<Claim>> GetUserClaims(string username)
        {
            return await _repository.All<UserClaim>()
                .Where(uc => uc.User.Username == username)
                .Select(c => c.Claim)
                .ToListAsync();
        }

        public async Task<IList<Claim>> GetUserClaims(Guid guid)
        {
            return await _repository.All<UserClaim>()
                .Where(uc => uc.User.Id == guid)
                .Select(c => c.Claim)
                .ToListAsync();
        }

        public async Task<OperationResult> AddClaimToUser(Claim claim, Guid userId)
        {
            User user = await _repository.FindByGuidAsync<User>(userId);

            return await AddClaim(claim, user);
        }

        public async Task<OperationResult> AddClaimToUser(Claim claim, string username)
        {
            User user = await _repository.All<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            return await AddClaim(claim, user);
        }

        public async Task<OperationResult> AddClaimsToUser(IEnumerable<Claim> claims, string username)
        {
            User user = await _repository.All<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            return await AddClaims(claims, user);
        }

        public async Task<OperationResult> AddClaimsToUser(IEnumerable<Claim> claims, Guid userId)
        {
            User user = await _repository.FindByGuidAsync<User>(userId);

            return await AddClaims(claims, user);
        }

        private async Task<OperationResult> AddClaims(IEnumerable<Claim> claims, User user)
        {
            OperationResult operationResult = new OperationResult();

            var userClaims = await GetUserClaims(user.Username);

            claims = claims
                .Where(c => userClaims.All(uc => c.Name != uc.Name && c.Value != uc.Value))
                .ToList();

            await _repository.AddRangeAsync(claims
                .Select(uc => new UserClaim() {
                        User = user,
                        Claim = uc
                    }
                ).ToList());

            try
            {
                await _repository.SaveChangesAsync();
                operationResult.Success = true;
            }
            catch (DbUpdateException dbu)
            {
                operationResult.AddError($"Failed to add claims to the user! Exception: {dbu.Message}");
            }

            return operationResult;
        }

        private async Task<OperationResult> AddClaim(Claim claim, User user)
        {
            OperationResult operationResult = new OperationResult();

            if (user == null)
            {
                operationResult.AddError("User was not found!");
                return operationResult;
            }

            await _repository.AddAsync(new UserClaim()
            {
                User = user,
                Claim = claim
            });

            try
            {
                await _repository.SaveChangesAsync();
                operationResult.Success = true;
            }
            catch (DbUpdateException)
            {
                operationResult.AddError("Failed to add claim to the user!");
            }

            return operationResult;
        }
    }
}