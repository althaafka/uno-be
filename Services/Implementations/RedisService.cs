using StackExchange.Redis;
using System.Text.Json;
using Uno.API.Services.Interfaces;

namespace Uno.API.Services.Implementations;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    public RedisService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;
        return JsonSerializer.Deserialize<T>(value!);
    }
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        return await _database.StringSetAsync(key, json, expiry);
    }
}