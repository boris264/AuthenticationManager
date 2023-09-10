using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Context;
using AuthenticationManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationManager.Data.Repositories
{
    public class AuthManagerRepository<TContext, TUser> : IAuthManagerRepository
        where TContext : AuthenticationManagerDbContext<TUser>
        where TUser : User
    {
        private readonly TContext _context;

        public AuthManagerRepository(TContext context)
        {
            _context = context;
        }

        public async Task AddAsync<T>(T entity) where T : class
        {
            await DbSet<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync<T>(ICollection<T> entities) where T : class
        {
            await DbSet<T>().AddRangeAsync(entities);
        }

        public IQueryable<T> All<T>() where T : class
        {
            return DbSet<T>();
        }

        public async Task<T> FindByGuidAsync<T>(Guid guid) where T : class
        {
            var entity = await _context.FindAsync<T>(guid);
            
            return entity;
        }

        public void Remove<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public void RemoveRange<T>(IEnumerable<T> entities) where T : class
        {
            _context.RemoveRange(entities);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        private DbSet<T> DbSet<T>() 
            where T : class
        {
            return _context.Set<T>();
        }
    }
}