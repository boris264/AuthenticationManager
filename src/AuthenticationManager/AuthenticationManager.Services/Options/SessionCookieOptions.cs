using Microsoft.AspNetCore.Http;

namespace AuthenticationManager.Services.Options
{
    public class SessionCookieOptions : CookieOptions
    {
        public string Name { get; set; } = "AuthManagerSessionId";
    }   
}