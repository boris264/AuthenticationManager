using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Services.Common;

namespace AuthenticationManager.Services.ClaimServices.Interfaces
{
    public interface IClaimService
    {
        public Task<OperationResult> AddClaimToUser(Claim claim, string username);

        public Task<OperationResult> AddClaimToUser(Claim claim, Guid userId);

        public Task<Claim> GetByGuid(Guid guid);

        public Task<IList<Claim>> GetUserClaims(string username);

        public Task<IList<Claim>> GetUserClaims(Guid userId);
    }
}