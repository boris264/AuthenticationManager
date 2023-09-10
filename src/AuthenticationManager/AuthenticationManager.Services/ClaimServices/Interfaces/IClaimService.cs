using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;

namespace AuthenticationManager.Services.ClaimServices.Interfaces
{
    public interface IClaimService
    {
        public Task AddClaimToUser(Claim claim, string username);

        public Task AddClaimToUser(Claim claim, Guid userId);

        public Task<Claim> GetByGuid(Guid guid);

        public Task<IList<Claim>> GetUserClaims(string username);

        public Task<IList<Claim>> GetUserClaims(Guid userId);
    }
}