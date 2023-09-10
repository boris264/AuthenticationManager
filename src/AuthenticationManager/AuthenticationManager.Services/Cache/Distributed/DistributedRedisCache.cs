using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace AuthenticationManager.Services.Cache.Distributed
{
    public class DistributedRedisCache : IDistributedRedisCache
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public DistributedRedisCache(ConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<byte[]> GetFieldAsync(string key, string field)
        {
            return await GetDb().HashGetAsync(key, field);
        }

        public async Task<Dictionary<string, byte[]>> GetAllFieldsAsync(string key)
        {
            Dictionary<string, byte[]> hashSetEntries = new Dictionary<string, byte[]>();
            HashEntry[] entries = await GetDb().HashGetAllAsync(key);

            foreach (var entry in entries)
            {
                if (!hashSetEntries.ContainsKey(entry.Name))
                {
                    hashSetEntries.Add(entry.Name, entry.Value);
                }
            }

            return hashSetEntries;
        }

        public async Task SetFieldAsync(string key, string field, byte[] data)
        {
            await GetDb().HashSetAsync(key, new HashEntry[] {
                new HashEntry(field, data)
            });
        }

        public async Task SetFieldsAsync(string key, Dictionary<string, byte[]> entries)
        {
            await GetDb().HashSetAsync(key, entries.Select(e => new HashEntry(e.Key, e.Value)).ToArray());
        }

        private IDatabase GetDb()
        {
            return _connectionMultiplexer.GetDatabase();
        }
    }
}