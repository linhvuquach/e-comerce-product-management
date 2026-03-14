using MediatR;
using ProductManagement.Application.Common.Cache;
using ProductManagement.Application.Common.Interfaces;

namespace ProductManagement.Application.Products.Events;

internal sealed class ProductCacheInvalidationHandler(ICacheService cache)
    : INotificationHandler<ProductChangedNotification>
{
    public async Task Handle(ProductChangedNotification notification, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(ProductCacheKeys.Product(notification.ProductId), cancellationToken);
        await cache.RemoveByPrefixAsync(ProductCacheKeys.ListRedisPrefix, cancellationToken);
    }
}
