using System.Collections.Generic;

namespace AuthenticationManager.Services.Common
{
    public class OperationResult
    {
        public bool Success { get; set; } = false;

        public List<string> ErrorMessages { get; private set; } = new List<string>();

        public void AddError(string errorMessage)
        {
            this.ErrorMessages.Add(errorMessage);
        }
    }
}
