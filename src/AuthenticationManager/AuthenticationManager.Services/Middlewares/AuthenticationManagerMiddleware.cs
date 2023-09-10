using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Services.Cache.Distributed;
using Microsoft.AspNetCore.Http;

namespace AuthenticationManager.Services.Middlewares
{
    public class AuthenticationManagerMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationManagerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDistributedRedisCache cache)
        {
            await _next(context);

            var authenticationManagerIdentity = context.User.Identities
                .FirstOrDefault(i => i.AuthenticationType == "AuthManager");

            if (authenticationManagerIdentity != null)
            {
                if (authenticationManagerIdentity.IsAuthenticated)
                {
                    
                }
            }
        }
    }
}