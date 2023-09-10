using System.Threading.Tasks;
using AuthenticationManager.Authentication.Common;
using AuthenticationManager.Data.Models;

namespace AuthenticationManager.Services.UserServices.Interfaces
{
    public interface IUserManager<TUser> 
        where TUser : User
    {
        Task<TUser> GetByUsernameAsync(string username);

        Task<TUser> GetByEmailAsync(string email);

        Task<AuthenticationResult> SignInAsync(string username, string password, bool rememberMe = false);

        Task<AuthenticationResult> RegisterAsync(TUser user);
    }
}