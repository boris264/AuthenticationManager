using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Data.Repositories;
using AuthenticationManager.Services.ClaimServices.Interfaces;
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

        public async Task AddClaimToUser(Claim claim, Guid userId)
        {
            User user = await _repository.FindByGuidAsync<User>(userId);

            if (user != null)
            {
                user.UserClaims.Add(new UserClaim() {
                    User = user,
                    Claim = claim
                });

                try
                {
                    await _repository.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _logger.LogCritical("Failed to add claim to the user!");
                }
            }
        }

        public async Task AddClaimToUser(Claim claim, string username)
        {
            User user = await _repository.All<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                user.UserClaims.Add(new UserClaim() {
                    User = user,
                    Claim = claim
                });

                try
                {
                    await _repository.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _logger.LogCritical("Failed to add claim to the user!");
                }
            }
        }
    }
}