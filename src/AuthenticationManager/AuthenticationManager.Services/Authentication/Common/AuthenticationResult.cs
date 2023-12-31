using System.Collections.Generic;

namespace AuthenticationManager.Authentication.Common
{
    public enum AuthenticationState
    {
        Success,
        Failure,
        TwoFactorRequired
    }

    public class AuthenticationResult
    {
        public AuthenticationState Result { get; set; } = AuthenticationState.Failure;

        public List<string> ErrorMessages { get; } = new List<string>();
    }
}