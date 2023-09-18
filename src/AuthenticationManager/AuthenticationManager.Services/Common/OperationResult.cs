using System.Collections.Generic;

namespace AuthenticationManager.Services.Common
{
    public class OperationResult
    {
        public bool Success { get; private set; } = true;

        public List<string> ErrorMessages { get; private set; } = new List<string>();

        public void Invalid(string errorMessage = default)
        {
            Success = false;
            
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessages.Add(errorMessage);
            }
        }
    }
}
