using AuthenticationManager.Services.Common;

namespace AuthenticationManager.Services.Interfaces
{
    public interface IPasswordValidator
    {
        OperationResult ValidatePassword(string password);
    }
}
