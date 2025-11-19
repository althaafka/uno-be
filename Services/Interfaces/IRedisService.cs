namespace Uno.API.Services.Interfaces;
public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
}