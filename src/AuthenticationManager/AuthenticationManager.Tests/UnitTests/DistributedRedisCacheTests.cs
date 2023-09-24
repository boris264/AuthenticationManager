using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AuthenticationManager.Services.Cache.Distributed;
using NUnit.Framework;
using StackExchange.Redis;

namespace AuthenticationManager.Tests.UnitTests
{
    [TestFixture]
    public class DistributedRedisCacheTests
    {
        private ConnectionMultiplexer _connectionMultiplexer 
            = ConnectionMultiplexer.Connect("localhost", options => {options.DefaultDatabase = 1; options.AllowAdmin = true;});
        private IDistributedRedisCache _cache;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _cache = new DistributedRedisCache(_connectionMultiplexer);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _connectionMultiplexer
                .GetServer("localhost:6379")
                .FlushDatabaseAsync(1);
        }

        [Test]
        public async Task CheckGetFieldReturnsCorrectValue()
        {
            await Set("testKey", "testField", ToByteArr("testData"));
            var value = await _cache.GetFieldAsync("testKey", "testField");

            Assert.That(ToString(value) == "testData");
        }

        [Test]
        public async Task CheckSetField()
        {
            await _cache.SetFieldAsync("testKey", "testField", ToByteArr("testData"));

            var value = await _connectionMultiplexer.GetDatabase()
                .HashGetAsync("testKey", "testField");

            Assert.That(ToString(value) == "testData");
        }

        [Test]
        public async Task CheckGetAllFieldsReturnsAllFields()
        {
            for (int i = 0; i < 3; i++)
            {
                await Set($"testKey", $"testField{i}", ToByteArr($"testData{i}"));
            }

            var values = await _cache.GetAllFieldsAsync("testKey");

            int index = 0;

            foreach (var item in values)
            {
                Assert.That(item.Key == $"testField{index}");
                Assert.That(ToString(item.Value) == $"testData{index}");
                index++;
            }
        }

        [Test]
        public async Task CheckSetAllFieldsSetsAllFields()
        {
            await _cache.SetFieldsAsync("testKey", new Dictionary<string, byte[]>()
            {
                {"testField1", ToByteArr("testData1")},
                {"testField2", ToByteArr("testData2")}
            });

            var entries = await _connectionMultiplexer.GetDatabase()
                .HashGetAllAsync("testKey");

            int index = 1;

            foreach (var item in entries)
            {
                Assert.That(item.Name == $"testField{index}");
                Assert.That(item.Value == $"testData{index}");
                index++;
            }
        }

        [Test]
        public async Task CheckGetFieldOnEmptyKey()
        {
            var result = await _cache.GetFieldAsync("", "testkey");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CheckGetFieldOnEmptyField()
        {
            await Set("testKey", "field", ToByteArr("testDataa"));

            var result = await _cache.GetFieldAsync("testKey", "");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CheckDeleteKeyRemovesKey()
        {
            await Set("testKey", "field", ToByteArr("testDataa"));

            await _cache.DeleteAsync("testKey");
            var result = await _cache.GetAllFieldsAsync("testKey");

            Assert.That(result.Count == 0);
        }

        private async Task Set(string key, string field, byte[] data)
        {
            await _connectionMultiplexer.GetDatabase()
                .HashSetAsync(key, new HashEntry[] {new HashEntry(field, data)});
        }

        private string ToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        private byte[] ToByteArr(string val)
        {
            return Encoding.UTF8.GetBytes(val);
        }
    }
}
