using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Data.Repositories;
using AuthenticationManager.Services.Common;
using AuthenticationManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthenticationManager.Services.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly IAuthManagerRepository _repository;

        private readonly ILogger<RoleService> _logger;

        public RoleService(IAuthManagerRepository repository,
                           ILogger<RoleService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<OperationResult> AddRole(string role)
        {
            OperationResult operationResult = new OperationResult();
            bool roleExists = await _repository.All<Role>()
                .Where(r => r.Name == role)
                .AnyAsync();
            
            if (roleExists)
            {
                operationResult.AddError("Role already exists!");
                return operationResult;
            }

            await _repository.AddAsync(new Role() {
                Name = role
            });

            try
            {
                await _repository.SaveChangesAsync();
                operationResult.Success = true;
            }
            catch (DbUpdateException dbUpdateException)
            {
                operationResult.AddError($"Failed to add role! Exception: {dbUpdateException.Message}");
            }

            return operationResult;
        }

        public async Task<OperationResult> AddUserToRole(string username, string role)
        {
            OperationResult operationResult = new OperationResult();

            User user = await _repository.All<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
            
            if (user == null)
            {
                operationResult.AddError($"User {username} was not found!");
                return operationResult;
            }

            Role r = await GetRole(role);

            if (r == null)
            {
                operationResult.AddError($"Role {role} doesn't exist!");
                return operationResult;
            }

            UserRole userRole = await _repository.All<UserRole>()
                .Where(ur => ur.RoleId == r.Id && ur.UserId == user.Id)
                .FirstOrDefaultAsync();
            
            if (userRole != null)
            {
                operationResult.AddError($"User {user.Username} is already in role {r.Name}!");
                return operationResult;
            }

            await _repository.AddAsync(new UserRole() {
                User = user,
                Role = r
            });

            try
            {
                await _repository.SaveChangesAsync();
                operationResult.Success = true;
            }
            catch (DbUpdateException dbUpdateException)
            {
                operationResult.AddError($"Failed to add user to role! Exception: {dbUpdateException.Message}");
            }

            return operationResult;
        }

        public async Task<Role> GetRole(string role)
        {
            return await _repository.All<Role>()
                .Where(r => r.Name == role)
                .FirstOrDefaultAsync();
        }
    }
}