using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Services.Common;

namespace AuthenticationManager.Services.Interfaces
{
    public interface IRoleService
    {
        public Task<Role> GetRole(string role);

        public Task<OperationResult> AddUserToRole(string username, string role);

        public Task<OperationResult> AddRole(string role);
    }
}