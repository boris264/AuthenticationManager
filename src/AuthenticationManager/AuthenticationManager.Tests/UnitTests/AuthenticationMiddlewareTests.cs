using System;
using System.Threading.Tasks;
using AuthenticationManager.Services.Middlewares;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using AuthenticationManager.Services.Options;
using Microsoft.AspNetCore.Http;
using System.Linq;
using AuthenticationManager.Services.Cache.Distributed;
using NSubstitute;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class AuthenticationMiddlewareTests
    {
        private Dictionary<string, Dictionary<string, byte[]>> _data
            = new Dictionary<string, Dictionary<string, byte[]>>();

        private IDistributedRedisCache _cache;

        private AuthenticationManagerMiddleware _authenticationManagerMiddleware;

        private HttpContext _httpContext;

        private SessionCookieOptions _sessionCookieOptions;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _cache = Substitute.For<IDistributedRedisCache>();
            _cache.GetFieldAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    if (_data.ContainsKey(callInfo.ArgAt<string>(0)))
                    {
                        return _data[callInfo.ArgAt<string>(0)][callInfo.ArgAt<string>(1)];
                    }
                    else
                    {
                        return null;
                    }
                });
            _cache.SetFieldAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>())
                .Returns(callInfo =>
                {
                    if (_data.ContainsKey(callInfo.ArgAt<string>(0)))
                    {
                        _data[callInfo.ArgAt<string>(0)] = new Dictionary<string, byte[]>()
                        {
                            { callInfo.ArgAt<string>(1), callInfo.ArgAt<byte[]>(2) }
                        };
                    }
                    else
                    {
                        _data.Add(callInfo.ArgAt<string>(0),
                                new Dictionary<string, byte[]>()
                                {
                                    { callInfo.ArgAt<string>(1), callInfo.ArgAt<byte[]>(2) }
                                }
                        );
                    }
                    return Task.CompletedTask;
                });
            
            _cache.DeleteAsync(Arg.Any<string>())
                .Returns(callInfo => 
                {
                    _data.Remove(callInfo.ArgAt<string>(0));
                    return Task.CompletedTask;
                });

            _sessionCookieOptions = new SessionCookieOptions();

            _authenticationManagerMiddleware = new AuthenticationManagerMiddleware(context =>
            {
                return Task.CompletedTask;
            },
            Options.Create(_sessionCookieOptions));

            _httpContext = new DefaultHttpContext();
        }

        [TearDown]
        public void TearDown()
        {
            _authenticationManagerMiddleware = new AuthenticationManagerMiddleware(context =>
            {
                return Task.CompletedTask;
            },
            Options.Create(_sessionCookieOptions));
            _httpContext = new DefaultHttpContext();
            _sessionCookieOptions = new SessionCookieOptions();
            _data = new Dictionary<string, Dictionary<string, byte[]>>();
        }

        [Test]
        public async Task CheckNewSessionIdCookieGetsAddedToResponse()
        {
            await _authenticationManagerMiddleware.InvokeAsync(_httpContext, null);

            var setCookieHeaders = _httpContext.Response.GetTypedHeaders().SetCookie;
            var result = Guid.Empty;

            Assert.That(setCookieHeaders.Any(c => c.Name == _sessionCookieOptions.Name));
            Assert.That(setCookieHeaders.Any(c => Guid.TryParse(c.Value, out result)));
        }

        [Test]
        public async Task CheckClaimsPrincipalGetsSetAndClaimsGetAdded()
        {
            var sessionId = Guid.NewGuid();
            _httpContext.Request.Headers["Cookie"] = $"{_sessionCookieOptions.Name}={sessionId}";

            string claimsJson = JsonSerializer
                .Serialize(new List<Data.Models.Claim>()
                {
                    new Data.Models.Claim() { Name = "claim1", Value = "test1"},
                    new Data.Models.Claim() { Name = "claim2", Value = "test2"},
                });

            await _cache.SetFieldAsync(sessionId.ToString(), "Claims", Encoding.UTF8.GetBytes(claimsJson));

            await _authenticationManagerMiddleware.InvokeAsync(_httpContext, _cache);

            Assert.That(_httpContext.User.Identity.IsAuthenticated);
            Assert.That(_httpContext.User.HasClaim("claim1", "test1"));
            Assert.That(_httpContext.User.HasClaim("claim2", "test2"));
        }

        [Test]
        public async Task CheckClaimsGetAddedToCacheAfterSuccessfullLogin()
        {
            var sessionId = Guid.NewGuid();
            _httpContext.Request.Headers["Cookie"] = $"{_sessionCookieOptions.Name}={sessionId}";

            List<Data.Models.Claim> claims = new List<Data.Models.Claim>()
            {
                new Data.Models.Claim() { Name = "claim1", Value = "test1"},
                new Data.Models.Claim() { Name = "claim2", Value = "test2"},
            };

            ClaimsIdentity loggedInUserIdentity = new ClaimsIdentity("AuthManagerCookie");

            loggedInUserIdentity.AddClaims(claims.Select(c => new Claim(c.Name, c.Value)));

            _httpContext.User = new ClaimsPrincipal(loggedInUserIdentity);

            await _authenticationManagerMiddleware.InvokeAsync(_httpContext, _cache);

            var claimsFromCache = JsonSerializer
                .Deserialize<List<Data.Models.Claim>>(await _cache.GetFieldAsync(sessionId.ToString(), "Claims"));

            Assert.That(claimsFromCache, Is.Not.Null);
            Assert.That(claimsFromCache.Count == 2);
            Assert.That(claimsFromCache.Any(c => c.Name == "claim1"));
            Assert.That(claimsFromCache.Any(c => c.Name == "claim2"));
        }

        [Test]
        public async Task CheckCachedDataForUserGetsRemovedAfterSignOut()
        {
            // Used to simulate logout.
            _authenticationManagerMiddleware = new AuthenticationManagerMiddleware(context =>
            {
                _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.CompletedTask;
            },
            Options.Create(_sessionCookieOptions));

            var sessionId = Guid.NewGuid();
            _httpContext.Request.Headers["Cookie"] = $"{_sessionCookieOptions.Name}={sessionId}";

            string claimsJson = JsonSerializer
                .Serialize(new List<Data.Models.Claim>()
                {
                    new Data.Models.Claim() { Name = "claim1", Value = "test1"},
                    new Data.Models.Claim() { Name = "claim2", Value = "test2"},
                });

            await _cache.SetFieldAsync(sessionId.ToString(), "Claims", Encoding.UTF8.GetBytes(claimsJson));

            await _authenticationManagerMiddleware.InvokeAsync(_httpContext, _cache);

            var claimsFromCache = await _cache.GetFieldAsync(sessionId.ToString(), "Claims");

            Assert.That(claimsFromCache, Is.Null);
            Assert.That(_httpContext.User.Identity, Is.Not.Null);
            Assert.That(!_httpContext.User.Identity.IsAuthenticated);
        }
    }
}
