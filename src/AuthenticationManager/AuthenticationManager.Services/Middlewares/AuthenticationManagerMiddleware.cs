using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AuthenticationManager.Services.Cache.Distributed;
using AuthenticationManager.Services.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthenticationManager.Services.Middlewares
{
    public class AuthenticationManagerMiddleware
    {
        private string _cacheClaimsField = "Claims";

        private readonly RequestDelegate _next;

        private readonly SessionCookieOptions _sessionCookieOptions;

        public AuthenticationManagerMiddleware(RequestDelegate next, 
                                               IOptions<SessionCookieOptions> options)
        {
            _next = next;
            _sessionCookieOptions = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, IDistributedRedisCache cache)
        {
            string userSessionId = context.Request.Cookies.ContainsKey(_sessionCookieOptions.Name) 
                ? context.Request.Cookies[_sessionCookieOptions.Name] : string.Empty;
            
            if (!string.IsNullOrEmpty(userSessionId))
            {
                byte[] claimsFromCache = await cache.GetFieldAsync(userSessionId, _cacheClaimsField);

                if (claimsFromCache != null)
                {
                    List<Data.Models.Claim> claims = JsonSerializer
                        .Deserialize<List<Data.Models.Claim>>(claimsFromCache);
                    
                    ClaimsIdentity loggedInUserIdentity = new ClaimsIdentity("AuthManagerCookie");

                    loggedInUserIdentity.AddClaims(claims.Select(c => new Claim(c.Name, c.Value)));

                    context.User = new ClaimsPrincipal(loggedInUserIdentity);
                } 
            }
            else
            {
                userSessionId = Guid.NewGuid().ToString();
                context.Response.Cookies.Append(_sessionCookieOptions.Name, userSessionId, _sessionCookieOptions);
            }

            await _next(context);

            var authenticationManagerIdentity = context.User.Identities
                .FirstOrDefault(i => i.AuthenticationType == "AuthManagerCookie");
            
            if (authenticationManagerIdentity != null)
            {
                if (authenticationManagerIdentity.IsAuthenticated)
                {
                    string claimsJson = JsonSerializer
                        .Serialize(authenticationManagerIdentity.Claims.Select(c =>
                            new { Name = c.Type, c.Value }
                    ));

                    if (!string.IsNullOrEmpty(userSessionId))
                    {
                        await cache.SetFieldAsync(userSessionId, _cacheClaimsField, ToByteArr(claimsJson));
                    }
                }
            }
        }

        private byte[] ToByteArr(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
    }
}