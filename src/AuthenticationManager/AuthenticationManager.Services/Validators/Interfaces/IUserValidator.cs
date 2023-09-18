using AuthenticationManager.Data.Models;
using AuthenticationManager.Services.Common;

namespace AuthenticationManager.Services.Interfaces
{
    public interface IUserValidator<TUser> where TUser : User
    {
        OperationResult ValidateUser(TUser user);
    }
}