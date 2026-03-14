using MediatR;

namespace ProductManagement.Application.Products.Events;

public sealed record ProductChangedNotification(Guid ProductId) : INotification;
