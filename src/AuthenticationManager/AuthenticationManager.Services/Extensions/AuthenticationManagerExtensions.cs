using System;
using AuthenticationManager.Data.Context;
using AuthenticationManager.Data.Models;
using AuthenticationManager.Data.Repositories;
using AuthenticationManager.Serivces.ClaimServices.Implementation;
using AuthenticationManager.Services.Authentication.PasswordHashers.Implementation;
using AuthenticationManager.Services.Authentication.PasswordHashers.Interfaces;
using AuthenticationManager.Services.Cache.Distributed;
using AuthenticationManager.Services.ClaimServices.Interfaces;
using AuthenticationManager.Services.Implementation;
using AuthenticationManager.Services.Interfaces;
using AuthenticationManager.Services.Middlewares;
using AuthenticationManager.Services.Options;
using AuthenticationManager.Services.UserServices.Implementation;
using AuthenticationManager.Services.UserServices.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AuthenticationManager.Services.Extensions
{
    public static class AuthenticationManagerExtensions
    {
        public static IServiceCollection AddAuthenticationManager<TContext, TUser>(
            this IServiceCollection services, 
            Action<DbContextOptionsBuilder> options,
            Action<SessionCookieOptions> sessionCookieOptions = default)
            where TContext : AuthenticationManagerDbContext<TUser>
            where TUser : User
        {
            services.AddScoped<IAuthManagerRepository, AuthManagerRepository<TContext, TUser>>();
            services.AddScoped<IUserManager<TUser>, UserManager<TUser>>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.AddScoped<IPasswordValidator, PasswordValidator>();

            services.AddDbContext<TContext>(options);
            
            if (sessionCookieOptions != null)
            {
                services.Configure(sessionCookieOptions);
            }

            return services;
        }

        public static IServiceCollection AddDistributedRedisCache(this IServiceCollection services, 
                                                                  string connectionString)
        {
            services.AddSingleton<ConnectionMultiplexer>(service => {
                return ConnectionMultiplexer.Connect(connectionString);
            });
            services.AddScoped<IDistributedRedisCache, DistributedRedisCache>();

            return services;
        }

        public static IApplicationBuilder UseAuthenticationManager(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationManagerMiddleware>();
        }   
    }
}