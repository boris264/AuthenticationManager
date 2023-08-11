using System;
using AuthenticationManager.Data.Context;
using AuthenticationManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationManager.Data.Extensions
{
    public static class AuthenticationManagerExtensions
    {
        public static IServiceCollection AddAuthenticationManager<TContext>(
            this IServiceCollection services, 
            Action<DbContextOptionsBuilder> options)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(options);
            
            return services;
        }
    }
}