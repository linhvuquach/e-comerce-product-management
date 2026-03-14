namespace ProductManagement.Application.Common.Cache;

public static class ProductCacheKeys
{
    // IDistributedCache keys — "pm:" instance prefix is applied automatically by the cache provider
    public static string Product(Guid id) => $"product:{id}";

    // Raw Redis pattern for server.Keys() scan in RemoveByPrefixAsync (includes "pm:" instance prefix)
    public const string ListRedisPrefix = "pm:products:";

    // IDistributedCache key prefix used when building list cache keys
    internal const string ListKeyPrefix = "products:";
}
