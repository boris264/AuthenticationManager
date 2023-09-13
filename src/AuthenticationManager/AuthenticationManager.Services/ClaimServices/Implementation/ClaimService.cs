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
            OperationResult result = await AddClaim(claim, user);
            
            return result;
        }

        public async Task<OperationResult> AddClaimToUser(Claim claim, string username)
        {
            User user = await _repository.All<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            OperationResult result = await AddClaim(claim, user);

            return result;
        }

        private async Task<OperationResult> AddClaim(Claim claim, User user)
        {
            OperationResult operationResult = new OperationResult();

            if (user == null)
            {
                operationResult.AddError("User was not found!");
                return operationResult;
            }

            user.UserClaims.Add(new UserClaim()
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