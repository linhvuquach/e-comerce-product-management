using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Common.Interfaces;
using StackExchange.Redis;

namespace ProductManagement.Infrastructure.Caching;

internal sealed partial class RedisCacheService(
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            return bytes is null ? null : JsonSerializer.Deserialize<T>(bytes, _jsonOptions);
        }
        catch (Exception ex)
        {
            LogCacheGetFailed(logger, key, ex);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry,
            };
            await cache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            LogCacheSetFailed(logger, key, ex);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            LogCacheRemoveFailed(logger, key, ex);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();

            if (keys.Length > 0)
            {
                var db = redis.GetDatabase();
                await db.KeyDeleteAsync(keys);
            }
        }
        catch (Exception ex)
        {
            LogCacheRemoveByPrefixFailed(logger, prefix, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cache GET failed for key {Key}. Returning null.")]
    private static partial void LogCacheGetFailed(ILogger logger, string key, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cache SET failed for key {Key}. Continuing without cache.")]
    private static partial void LogCacheSetFailed(ILogger logger, string key, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cache REMOVE failed for key {Key}.")]
    private static partial void LogCacheRemoveFailed(ILogger logger, string key, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cache REMOVE BY PREFIX failed for prefix {Prefix}.")]
    private static partial void LogCacheRemoveByPrefixFailed(ILogger logger, string prefix, Exception ex);
}
