using FluentAssertions;
using MediatR;
using Moq;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Commands.UpdateProduct;
using ProductManagement.Application.Products.Events;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Unit.Tests.Application;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISlugService> _slugService = new();
    private readonly Mock<IPublisher> _publisher = new();

    private UpdateProductCommandHandler CreateHandler() =>
        new(_productRepo.Object, _unitOfWork.Object, _slugService.Object, _publisher.Object);

    private static UpdateProductCommand DefaultCommand(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), "Updated Name", "Updated Brand", Guid.NewGuid(), 149.99m, "USD", "desc", null);

    private static Product BuildProduct()
    {
        return Product.Create(
            "Original Name",
            Slug.Create("original-name"),
            "Original Brand",
            Guid.NewGuid(),
            Money.Of(99.99m, "USD"));
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesProduct()
    {
        var product = BuildProduct();
        var command = DefaultCommand(product.Id);
        var newSlug = Slug.Create("updated-name");

        _productRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newSlug);

        await CreateHandler().Handle(command, CancellationToken.None);

        product.Name.Should().Be("Updated Name");
        product.Brand.Should().Be("Updated Brand");
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        var product = BuildProduct();
        var command = DefaultCommand(product.Id);

        _productRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Slug.Create("updated-name"));

        await CreateHandler().Handle(command, CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesProductChangedNotification()
    {
        var product = BuildProduct();
        var command = DefaultCommand(product.Id);

        _productRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Slug.Create("updated-name"));

        await CreateHandler().Handle(command, CancellationToken.None);

        _publisher.Verify(p => p.Publish(
            It.IsAny<ProductChangedNotification>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsProductNotFoundException()
    {
        var command = DefaultCommand();
        _productRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await CreateHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task Handle_ProductNotFound_DoesNotSaveChanges()
    {
        var command = DefaultCommand();
        _productRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        try { await CreateHandler().Handle(command, CancellationToken.None); } catch { }

        _unitOfWork.Verify(u => u.SaveChangeAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
