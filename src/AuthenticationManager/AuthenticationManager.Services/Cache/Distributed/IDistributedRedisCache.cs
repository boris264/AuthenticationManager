using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationManager.Services.Cache.Distributed
{
    public interface IDistributedRedisCache
    {
        public Task<byte[]> GetFieldAsync(string key, string field);

        public Task<Dictionary<string, byte[]>> GetAllFieldsAsync(string key);

        public Task SetFieldAsync(string key, string field, byte[] data);

        public Task SetFieldsAsync(string key, Dictionary<string, byte[]> entries);

        public Task DeleteAsync(string key);
    }
}