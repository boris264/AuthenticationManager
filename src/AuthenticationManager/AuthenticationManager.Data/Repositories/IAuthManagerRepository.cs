using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationManager.Data.Context;

namespace AuthenticationManager.Data.Repositories
{
    public interface IAuthManagerRepository
    {
        public IQueryable<T> All<T>() where T : class;

        public Task AddAsync<T>(T entity) where T : class;

        public Task AddRangeAsync<T>(ICollection<T> entities) where T : class;

        public Task<int> SaveChangesAsync();

        public Task<T> FindByGuidAsync<T>(Guid guid) where T : class;

        public void Remove<T>(T entity) where T : class;

        public void RemoveRange<T>(IEnumerable<T> entities) where T : class;
    }
}